using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable()]
public class OPPose
{
    private EnumScaleMethod scalingMethod = Base.ScaleMethod;              // Which scaling method is used.
    public static readonly int amountOfPreviousScalingFactors = 4;        // How many frames the interpolation of scaling should be based on.
    private static readonly int amountOfPreviousFigureForIdentifying = 10;  // How many frames the identification of a figure should be based on.
    private static readonly float threshold_Figures_Differ = 10;           // Threshold : maximum distance between 2 figures of same ID.
    private static int id_counter = -1;                                    // Increase counter when new figure appears in video.

    public static int KEYPOINTS_NUMBER = Base.jointsAmount;
    public Vector3[] joints;                        // All joints after being processed.
    public Vector3[] jointsRAW;                     // All joints raw from json. 
    public bool[] available;                        // Is the joint[] available.
    public float scaleFactor;                       // What is the scale factor of this pose.
    public List<Neighbour> neighbours;              // k-neighbours
    public Neighbour selectedN;                     // The leading neighbour.
    public Vector3 translation;                     // The translation of that pose. (not in use)
    public int appearanceOrder;                     // The order that it appears in JSON.
    public int id;                                  // Identification of the figure.

    public OPPose(Keypoints k, List<OPFrame> frames = null, int currFrameIndex = 0)
    {
        // Initialisation
        joints = new Vector3[KEYPOINTS_NUMBER];
        jointsRAW = new Vector3[KEYPOINTS_NUMBER];
        available = new bool[KEYPOINTS_NUMBER];
        scaleFactor = 0;
        neighbours = new List<Neighbour>();
        selectedN = null;
        id = identifyFigure(frames, currFrameIndex);
        // Normalize data
        fillBodyPositions(k);
        convertPositionsToRoot();
        scaleFactor = scalePositions(getPreviousScaleFactors(frames, currFrameIndex, amountOfPreviousScalingFactors, id));
    }

    public OPPose(Vector3[] joints_input, bool[] available_input,  List<OPFrame> frames = null, int currFrameIndex = 0)
    {
        // Initialisation
        joints = new Vector3[KEYPOINTS_NUMBER];
        jointsRAW = new Vector3[KEYPOINTS_NUMBER];
        available = new bool[KEYPOINTS_NUMBER];
        Array.Copy(joints_input, joints,joints_input.Length);
        Array.Copy(joints_input, jointsRAW, joints_input.Length);
        Array.Copy(available_input, available, available_input.Length);
        scaleFactor = 0;
        neighbours = new List<Neighbour>();
        selectedN = null;
        id = 0; // << SET FIGURE ID TO ZERO FOR ALL
                // There is a problem because Openpose is asychronous
        // Normalize data
        convertPositionsToRoot(true);
        scaleFactor = scalePositions(getPreviousScaleFactors(frames, currFrameIndex, amountOfPreviousScalingFactors, id));
    }


    // To identification na ginetai joint to joint 2D eucledian? 
    private int identifyFigure(List<OPFrame> frames, int currFrameIndex)
    {
        if (frames == null)
        {
            Debug.Log("Error. Figure couldn't be identified due to missing frames.");
            return 0;
        }

        // Go to previous frames and get the distance from those figures. Is the figure similar to a previous one?
        // Use jointsRAW, because Identification should be done before Scaling due to the interpolation of scaling.
        int prevframeCounter = 0;
        int minimum_ID = -1;
        float minimum_distance = float.MaxValue;
        float distance = 0f;

        // If it is the first frame -> assign a unique id!
        if (currFrameIndex==0)
        {
            return ++id_counter;
        }

        // Calculate distance jointsRAW, based only on last frame
        foreach (OPPose figure in frames[currFrameIndex-1].figures)
        {
            distance = Distance2D_JOINTS_RAW(figure);
            if (distance < minimum_distance)
            {
                minimum_distance = distance;
                minimum_ID = figure.id;
            }
        }


        // Calculate Velocity
        /*
        for (int i = currFrameIndex - 1; i >= 0 && prevframeCounter < amountOfPreviousFigureForIdentifying; i--)
        {
            foreach(OPPose figure in frames[i].figures)
            {
                // calculate velocity here
            }
            prevframeCounter++;
        }
        */
        //  > Now it's time to decide the ID <
        // The ID that will finally be assigned to the figure is an old one:
        if (distance < threshold_Figures_Differ && minimum_ID>=0)
        {
            return minimum_ID;
        }
        // The ID that wil finally be assigned to the figure is a new one
        return ++id_counter;
    }


    public float Distance2D_JOINTS_RAW(OPPose op)
    {
        float sum = 0;
        for (int i = 0; i < op.jointsRAW.Length; i++)
        {
            Vector2 j1 = jointsRAW[i];
            Vector2 j2 = op.jointsRAW[i];
            // The joint of openpose pose might not be available,
            // due to distorted/uncompleted openpose output.
            if (!op.available[i] || !available[i]) continue;
            sum += Vector2.Distance(j1, j2);
        }
        return sum;
    }



    /* Sets the jsonpositions of a person, from Json file. */
    private void fillBodyPositions(Keypoints k)
    {
        /* Fill person's json positions*/
        for (int i = 0; i < KEYPOINTS_NUMBER * 3; i += 3)
        {
            float x = k.pose_keypoints_2d[i];
            float y = k.pose_keypoints_2d[i + 1] * -1.0f; // y axis is reversed
            Vector2 xy = new Vector2(x, y);
            jointsRAW[i / 3] = xy;
            /* Check if joint is available in json */
            if (x == 0 && y == 0)
                available[i / 3] = false;
            else
                available[i / 3] = true;
        }
    }




    /* Convert Position from image positions to positions from root. */
    private void convertPositionsToRoot(bool flipImage=false)
    {
        /* Joint - Root */
        Vector3 rootRAW = (jointsRAW[8] + jointsRAW[11]) / 2;
        for (int i=0; i<joints.Length; i++)
        {
            if (available[i]==false)
                joints[i] = new Vector3(0, 0, 0);
            else
            {
                joints[i] = jointsRAW[i] - rootRAW;

                if (flipImage)
                {
                    joints[i].y = -joints[i].y;
                }
                jointsRAW[i] = joints[i]; // Translate also the RAW joints.
            }
                
        }
    }

    /* Scale positions, by multiplying with a scaleFactor. */
    private float scalePositions(float [] previousScaleFactors)
    {
        // Determine Scaling Factor
        if (scalingMethod == EnumScaleMethod.SCALE_LIMBS)
            scaleFactor = Scaling.getGlobalScaleFactor_USING_LIMBS(this.joints, previousScaleFactors);
        else
            scaleFactor = Scaling.getGlobalScaleFactor_USING_HEIGHT(this.joints, previousScaleFactors);
        // Apply scaling
        for (int i=0; i<joints.Length; i++)
        {
            joints[i] *= scaleFactor;
        }
        return scaleFactor;
    }

    /// <summary>
    /// Returns the previous scale factors.
    /// </summary>
    /// <param name="currentFrameIndex"></param>
    /// <param name="amountOfPreviousFrames"></param>
    /// <returns></returns>
    public float[] getPreviousScaleFactors(List<OPFrame> frames, int currentFrameIndex, int amountOfPreviousFrames, int figureID)
    {
        List<float> previousScalingFactors = new List<float>();
        // The first frame does not have any previous scale factors, or there arent any figures in prev frame
        if (currentFrameIndex == 0 || frames[currentFrameIndex-1].figures.Count==0)
            return previousScalingFactors.ToArray();

        int counter = 0;
        for (int i = currentFrameIndex-1; i >= 0 && counter < amountOfPreviousFrames; i--)
        {
            try
            {
                previousScalingFactors.Add(frames[i].figures[figureID].scaleFactor);
                counter++;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.Log(e.Message+"\nError in getPreviousScalingFactors:\n"+
                    "The index of frames is:"+i+"\nBut the frames length is:"+frames.Count+
                    (frames.Count<=i?"\nframes Index is out of range!\n":"")+
                    (frames.Count > i?("\nThe index of figures is:"+figureID+ 
                     (frames[i].figures != null?("\nBut the length of figures is:" +frames[i].figures.Count):("figures in frame "+i+" is null!!"))):"")+
                    "\n\nStack Trace:\n"+e.StackTrace);
            }

        }
        return previousScalingFactors.ToArray();
    }

    
    public string jointsToString(bool isRaw)
    {
        string s = "";
        Vector3[] j;
        if (isRaw)
            j = jointsRAW;
        else
            j= joints; 
        for(int i=0; i<j.Length; i++)
        {
            s += j[i].x + " " + j[i].y + " " + j[i].z + (available[i]==true?"":" N\\A") + "\n";
        }
        return s;
    }
}
