using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The history of each identification might contain null poses.
/// </summary>
public class FigureIdentifier
{

    private static readonly int MAX_HISTORY_NUM_PREV_FRAMES = 5;
    private float averageDistanceThreshold = 10f;
    private Dictionary<int,Identification> identifications;


    public class Identification
    {
        public int id;
        public Queue<OPPose> log;

        public Identification(int id)
        {
            log = new Queue<OPPose>();
            this.id = id; 
        }

        public void Enqueue(OPPose figure)
        {
            log.Enqueue(figure);
            if (log.Count > MAX_HISTORY_NUM_PREV_FRAMES)
                log.Dequeue();
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
    public List<OPPose> sortFiguresInFrame(OPFrame frame)
    {
        if (frame == null || frame.figures == null)
            return null;
        /* 1. Remove any poses that hips can not be identified.      */
        /* 2. Find the id number with the nearest hips (root).       */
        /* 3. Assign the id number to the pose.                      */
        /* 4. Save the pose to its corresponding identification log. */
        /* 5. In each frame, each identification should be assigned a value. pose or null */
        SortedList<int, OPPose> list = new SortedList<int, OPPose>();
        List<int> idsUsedInThisFrame = new List<int>();
        foreach (OPPose figure in frame.figures)
        {
            if (isPoseValid(figure))
            {
                int id = identifyFigure(figure);
                figure.id = id;
                if (id != -1)
                {
                    list.Add(id, figure);
                    SavePoseInPositionHistory(figure, id);
                    idsUsedInThisFrame.Add(id);
                }
            }
        }
        
        for(int i=0; i<identifications.Count; i++)
        {
            if (!idsUsedInThisFrame.Contains(i))
                identifications[i].Enqueue(null);
        }
        Debug.Log(ToString());
        return new List<OPPose>(list.Values);
    }


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

    public int identifyFigure(OPPose entry)
    {
        if (entry == null)
        {
            return -1;
        }
            

        // First entry!:
        if (identifications.Count == 0)
        {
            Identification newIdentification = new Identification(0);
            identifications.Add(0,newIdentification);
            newIdentification.Enqueue(entry);
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
                    /* Find the first not null pose, and compare. */
                    distanceWithThisID = pose.getHipDistance_IMG(entry);
                    break;
                }
            }
            
            // An se ena id, einai sto MAX tou me ola null. Tote na to diagrafw, kai na arithmw ksana ola ta ids apo tin arxi.
            // bad idea, gt ta xalasei i arithmisi twn ids, kai tha iparxei mperdema me ta prosfata anatethimena ids.

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
            int newId = identifications.Count - 1;
            Identification newIdentification = new Identification(newId);
            identifications.Add(newId,newIdentification);
            newIdentification.Enqueue(entry);
            minimumID = newId;
        }
        return minimumID;
    }



    private void SavePoseInPositionHistory(OPPose entry, int id=0)
    {
        // Save null pose in all identifications. None identification has taken this pose.
        if (entry == null)
        {
            return;
        }

        // Save null pose in all identifications, except the identification that has taken this pose.
        Identification log = identifications[id];
        log.Enqueue(entry);
    }

}
