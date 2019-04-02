using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The history of each identification might contain null poses.
/// </summary>
public class FigureIdentifier
{

    private static readonly int MAX_HISTORY_NUM_PREV_FRAMES = 1;
    private static readonly int MAX_FRAMES_BEING_UNUSED = 5;

    private float averageDistanceThreshold = 50f;
    private Dictionary<int,Identification> identifications;


    public class Identification
    {
        public int id;
        public Queue<OPPose> log;
        public int lastFrameUsed;

        public Identification(int id)
        {
            log = new Queue<OPPose>();
            this.id = id; 
        }

        public void Enqueue(OPPose figure, int frame)
        {
            log.Enqueue(figure);
            lastFrameUsed = frame;
            if (log.Count > MAX_HISTORY_NUM_PREV_FRAMES)
                log.Dequeue();
        }
    }


    public void remove_Idle_Identifications(int frame)
    {
        /* Remove idle identifications. */
        List<int> indicesToBeRemoved = new List<int>();
        foreach (KeyValuePair<int, Identification> id in identifications)
        {
            if(id.Value.lastFrameUsed + MAX_FRAMES_BEING_UNUSED <= frame)
            {
                indicesToBeRemoved.Add(id.Key);
            }
        }
        foreach (int idle_id in indicesToBeRemoved)
        {
            identifications.Remove(idle_id);
        }

    }

    public FigureIdentifier()
    {
        identifications = new Dictionary<int, Identification>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frame"></param>
    /// <returns>Sorted list of figures</returns>
    //public List<OPPose> sortFiguresInFrame(OPFrame frame)
    //{
    //    if (frame == null || frame.figures == null)
    //        return null;
    //    /* 1. Remove any poses that hips can not be identified.      */
    //    /* 2. Find the id number with the nearest hips (root).       */
    //    /* 3. Assign the id number to the pose.                      */
    //    /* 4. Save the pose to its corresponding identification log. */
    //    /* 5. In each frame, each identification should be assigned a value. pose or null */
    //    SortedList<int, OPPose> list = new SortedList<int, OPPose>();
    //    List<int> idsUsedInThisFrame = new List<int>();
    //    foreach (OPPose figure in frame.figures)
    //    {
    //        if (isPoseValid(figure))
    //        {
    //            int id = identifyFigure(figure);
    //            figure.id = id;
    //            if (id != -1)
    //            {
    //                list.Add(id, figure);
    //                SavePoseInPositionHistory(figure, id);
    //                idsUsedInThisFrame.Add(id);
    //            }
    //        }
    //    }
        
    //    for(int i=0; i<identifications.Count; i++)
    //    {
    //        if (!idsUsedInThisFrame.Contains(i))
    //            identifications[i].Enqueue(null);
    //    }
    //    Debug.Log(ToString());
    //    return new List<OPPose>(list.Values);
    //}


    public override string ToString()
    {
        string s = "";
        int counter = 0;
        foreach (Identification identification in identifications.Values)
        {
            s += "[id:" + counter + "]->";
            foreach (OPPose figure in identification.log)
            {
                if (figure == null)
                    s += "null ";
                else
                    s += figure.getRoot_IMG()+" ";
            }
            s += "\n";
            counter++;
        }
        return s;
    }
    

    public bool isPoseValid(OPPose pose)
    {
        if (pose.available[(int)EnumJoint.LeftUpLeg] && pose.available[(int)EnumJoint.RightUpLeg])
            return true;
        else
            return false;
    }

    public int identifyFigure(OPPose entry, int frame)
    {
        if (entry == null)
        {
            throw new System.Exception("Can not identify NULL pose!");
        }
            

        // First entry!:
        if (identifications.Count == 0)
        {
            Debug.Log("First entry in ");
            Identification newIdentification = new Identification(0);
            identifications.Add(0,newIdentification);
            newIdentification.Enqueue(entry, frame);
            return 0;
        }


        float minimumDistance = float.MaxValue;
        int minimumID = -1;

        for(int id=0; id<identifications.Count; id++)
        {
            // Calcualte distance with this id.
            Identification identificationLog = identifications[id];
            float distanceWithThisID = 0;
            
            foreach (OPPose pose in identificationLog.log)
            {
                if (pose != null)
                {
                    distanceWithThisID = pose.AverageDistance2Djoints_IMG_RAW(entry);
                }
            }
            

            // Update the minimum id.
            if(distanceWithThisID < minimumDistance)
            {
                minimumDistance = distanceWithThisID;
                minimumID = id;
            }
        }


        // Should we create a new identification for this entry?
        if(minimumID==-1 || minimumDistance >= averageDistanceThreshold)
        {
            int newId = identifications.Count;
            Identification newIdentification = new Identification(newId);
            identifications.Add(newId,newIdentification);
            newIdentification.Enqueue(entry, frame);
            minimumID = newId;
        }
        else
        {
            SavePoseInPositionHistory(entry, minimumID, frame);
        }
        return minimumID;
    }



    private void SavePoseInPositionHistory(OPPose entry, int id, int frame)
    {
        Identification log = identifications[id];
        log.Enqueue(entry, frame);
    }

}
