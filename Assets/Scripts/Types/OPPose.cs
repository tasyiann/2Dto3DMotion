using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable()]
public class OPPose
{                                                                           // TODO: Gather all parameters, in one (Base, Algorithms etc).

    private EnumScaleMethod scalingMethod = Base.ScaleMethod;               // Which scaling method is used.
    public static readonly int amountOfPreviousScalingFactors = 4;          // How many frames the interpolation of scaling should be based on.
    private static readonly float threshold_Figures_Differ = 10;            // Threshold : maximum distance between 2 figures of same ID.
    private static int id_counter = -1;                                     // Increase counter when new figure appears in video.

    public static int KEYPOINTS_NUMBER = Base.jointsAmount;
    public static FigureIdentifier figureIdentifier = Base.figureIdentifier;


    public Vector3[] joints;                        // All joints after being processed.
    public Vector3[] jointsRAW;                     // All joints raw from json. 
    public Vector3[] jointsIMAGE;                   // As in image.
    public bool[] available;                        // Is the joint[] available.
    public float scaleFactor;                       // What is the scale factor of this pose.
    public EnumBONES limbFactor;                    // Which limb is used to scale.
    public List<Neighbour> neighbours;              // k-neighbours
    private List<Cluster> selectedClusters;         // Selected Clusters to search in.
    public OPPose prevFigure;                       // Save reference to the previous Pose.
    public Neighbour Estimation3D;                  // The leading neighbour (3D Estimation of that figure).
    public Vector3 translation;                     // The translation of that pose. (not in use)
    public int appearanceOrder;                     // The order that it appears in JSON.
    public int id;                                  // Identification of the figure.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flipImage"></param>
    /// <param name="frames"></param>
    /// <param name="currFrameIndex"></param>
    /// <returns> Returns false if pose is not valid. </returns>
    private void processData(bool flipImage = false, List < OPFrame> frames = null, int currFrameIndex = 0 )
    {
        // id = figureIdentifier.identifyFigure(this);
        convertPositionsToRoot();
        scaleFigure(getPreviousScaleFactors(frames, currFrameIndex, amountOfPreviousScalingFactors, id));
    }

    public OPPose(Keypoints k, List<OPFrame> frames = null, int currFrameIndex = 0, int currentFigureIndex=0)
    {
        initialiseFields();
        fillBodyPositions(k);
        processData(false);

    }

    public OPPose(Vector3[] joints_input, bool[] available_input,  List<OPFrame> frames = null, int currFrameIndex = 0, int currentFigureIndex=0)
    {
        initialiseFields();
        // Get Realtime data: Copy Values into the arrays.
        Array.Copy(joints_input, joints,joints_input.Length);
        Array.Copy(joints_input, jointsRAW, joints_input.Length);
        Array.Copy(available_input, available, available_input.Length);
        processData(true);
    }

    private void initialiseFields()
    {
        // Initialisation
        joints = new Vector3[KEYPOINTS_NUMBER];
        jointsIMAGE = new Vector3[KEYPOINTS_NUMBER];
        jointsRAW = new Vector3[KEYPOINTS_NUMBER];
        available = new bool[KEYPOINTS_NUMBER];
        scaleFactor = 0;
        neighbours = new List<Neighbour>();
        selectedClusters = null;
        Estimation3D = null;
    }

    public Vector2 getRoot_IMG()
    {
        int LeftUpLeg = (int)EnumJoint.LeftUpLeg;
        int RightUpLeg = (int)EnumJoint.RightUpLeg;
        if (!available[LeftUpLeg] || !available[RightUpLeg])
            return Vector2.zero; 
        else
            return (jointsIMAGE[LeftUpLeg] + jointsIMAGE[RightUpLeg]) / 2;
    }

    public float getHipDistance_IMG(OPPose pose)
    {

        int LeftUpLeg = (int)EnumJoint.LeftUpLeg;
        int RightUpLeg = (int)EnumJoint.RightUpLeg;

        if (!available[LeftUpLeg] || !available[RightUpLeg] || !pose.available[LeftUpLeg] || !pose.available[RightUpLeg])
            throw new Exception("Hip couldn't be identified in this figure! ");

        Vector2 rootA = (pose.jointsIMAGE[LeftUpLeg] + pose.jointsIMAGE[RightUpLeg]) / 2;
        Vector2 rootB = (jointsIMAGE[LeftUpLeg] + jointsIMAGE[RightUpLeg]) / 2;
        return Vector2.Distance(rootA,rootB);
    }


   // public float AverageDistance2Djoints_IMG_RAW(OPPose op)
   // {
        //float sum = 0;
        //int countIterations = 0;
       

        //for (int i = 0; i < op.jointsRAW.Length; i++)
        //{
        //    Vector2 j1 = jointsIMAGE[i];
        //    Vector2 j2 = op.jointsIMAGE[i];
        //    // The joint of openpose pose might not be available,
        //    // due to distorted/incompleted openpose output.
        //    if (!op.available[i] || !available[i])
        //    {
        //        continue;
        //    }
        //    // Exclude hands: (Left/Right) Arm, ForeArm, Hand
        //    if (i >= (int)EnumJoint.RightArm && i <= (int)EnumJoint.LeftHand)
        //        continue;

        //    sum += Vector2.Distance(j1, j2);
        //    countIterations++;
        //}
        //return sum/countIterations;
   // }



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
            jointsIMAGE[i / 3] = xy; // << 8/3 assinged
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
    private void scaleFigure(float [] previousScaleFactors)
    {
        // Determine Scaling Factor
        if (scalingMethod == EnumScaleMethod.SCALE_LIMBS)
            Scaling.getGlobalScaleFactor_USING_LIMBS(this.joints, out limbFactor, out scaleFactor, previousScaleFactors);
        else
            scaleFactor = Scaling.getGlobalScaleFactor_USING_HEIGHT(this.joints, previousScaleFactors);
        // Apply scaling
        for (int i=0; i<joints.Length; i++)
        {
            joints[i] *= scaleFactor;
        }

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
        try
        {
            if (currentFrameIndex == 0 || frames[currentFrameIndex - 1].figures.Count < figureID)
                return previousScalingFactors.ToArray();
        }
        catch (ArgumentOutOfRangeException e)
        {
            int i = currentFrameIndex;
            Debug.LogError(e.Message + "\nError in getPreviousScalingFactors:\n" +
                   "The index of frames is:" + i + "\nBut the frames length is:" + frames.Count +
                   (frames.Count <= i ? "\nframes Index is out of range!\n" : "") +
                   (frames.Count > i ? ("\nThe index of figures is:" + figureID +
                    (frames[i].figures != null ? ("\nBut the length of figures is:" + frames[i].figures.Count) : ("figures in frame " + i + " is null!!"))) : "") +
                   "\n\nStack Trace:\n" + e.StackTrace);
        }


        int counter = 0;
        for (int i = currentFrameIndex-1; i >= 0 && counter < amountOfPreviousFrames; i--)
        {
            counter++;
            try
            {
                if(frames[i].figures.Count <= figureID)
                {
                    continue;
                }

                previousScalingFactors.Add(frames[i].figures[figureID].scaleFactor);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e.Message+"\nError in getPreviousScalingFactors:\n"+
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


    public IList<Cluster> getSelectedClusters()
    {
        return selectedClusters;
    }
    public void setSelectedClusters(IList<Cluster> clusters)
    {
        selectedClusters = new List<Cluster>(clusters);
    }

}
