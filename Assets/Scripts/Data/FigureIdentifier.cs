using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The history of each identification might contain null poses.
/// </summary>
public class FigureIdentifier
{
    private readonly int MAX_HISTORY_NUM_PREV_FRAMES = 3;
    private float averageDistanceThreshold = 10f;
    private List<Queue<OPPose>> identifications;

    public FigureIdentifier()
    {
        identifications = new List<Queue<OPPose>>();
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
        /* 1. Remove any poses that hips can not be identified. */
        /* 2. Find the id number with the nearest hips.         */
        SortedList<int, OPPose> list = new SortedList<int, OPPose>();
        foreach (OPPose figure in frame.figures)
        {
            if (isPoseValid(figure))
            {
                figure.id = identifyFigure(figure);
                if (figure.id != -1)
                {
                    list.Add(figure.id, figure);
                }
            }
        }
        return new List<OPPose>(list.Values);
    }

    public bool isPoseValid(OPPose pose)
    {
        if (pose.available[(int)EnumJoint.LeftUpLeg] && pose.available[(int)EnumJoint.RightUpLeg])
        {
            return true;
        }
        else
            return false;
    }

    public int identifyFigure(OPPose entry)
    {
        if (entry == null)
        {
            SavePoseInPositionHistory(entry);
            return -1;
        }
            

        // First entry!:
        if (identifications.Count == 0)
        {
            Queue<OPPose> newIdentification = new Queue<OPPose>();
            identifications.Add(newIdentification);
            newIdentification.Enqueue(entry);
            return 0;
        }


        float minimumDistance = float.MaxValue;
        int minimumID = -1;

        for(int id=0; id<identifications.Count; id++)
        {
            // Calcualte distance with this id.
            Queue<OPPose> identificationLog = identifications[id];
            float distanceWithThisID = 0;
            int countNotNullPoses = 0;
            foreach (OPPose pose in identificationLog)
            {
                if (pose == null)
                    continue;

                distanceWithThisID += pose.getHipDistance_IMG_RAW(entry);
                countNotNullPoses++;
            }
            // Get the average:
            if(countNotNullPoses!=0)
                distanceWithThisID = distanceWithThisID / countNotNullPoses;

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
            return -1;
            //Queue<OPPose> newIdentification = new Queue<OPPose>();
            //identifications.Add(newIdentification);
            //newIdentification.Enqueue(entry);
            //minimumID = identifications.Count - 1;
        }
        else // Use the minimumID (Do not create another identification).
        {
            SavePoseInPositionHistory(entry, minimumID);
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
        Queue<OPPose> log = identifications[id];
        log.Enqueue(entry);
        if (log.Count > MAX_HISTORY_NUM_PREV_FRAMES)
            log.Dequeue();
    }

}
