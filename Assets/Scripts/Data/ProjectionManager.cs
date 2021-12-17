using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using Winterdust;

/* Installs projections in files. */

public class ProjectionManager : MonoBehaviour {

    public static EnumScaleMethod scalingMethod;
    public EnumScaleMethod ChooseScalingMethod;
    public string @BvhFilesDirectory;
    public string @OutputDirectory;
    public int Degrees;
    private static int filesCounter;


	void Start () {
        scalingMethod = ChooseScalingMethod;
        filesCounter = 0;
        AddInDatabase();
    }
	
    private void AddInDatabase()
    {
        // For each bvh file in the directory, get the projections.
        string[] fileEntries = Directory.GetFiles(BvhFilesDirectory);
        foreach (string bvhFileName in fileEntries)
        {

            if (Path.GetExtension(bvhFileName).CompareTo(".bvh") == 0)
            {
                CreateFiles(bvhFileName);
                filesCounter++;
            }
                
        }
    }


    private void CreateFiles(string bvhfilename)
    {
        if(!Directory.Exists(OutputDirectory))
            Directory.CreateDirectory(OutputDirectory);
        Directory.CreateDirectory(OutputDirectory + "\\Projections\\");
        Directory.CreateDirectory(OutputDirectory + "\\Rotations\\");
        string positionsFilePath = OutputDirectory+ "\\Projections\\" + (filesCounter)+".p";
        string rotationsFilePath = OutputDirectory+ "\\Rotations\\" + (filesCounter);
        SaveProjectionsInFiles(bvhfilename, positionsFilePath, rotationsFilePath);
    }


    private void SaveProjectionsInFiles(string filename, string positionsFilePath, string rotationsFilePath)
    {
        // PUT percentageInfo as a parameter, in order to specify a percentage.
        BVH bvh = new BVH(filename);
        bvh.makeDebugSkeleton();
        int[] order = getOrderOfJoints(bvh);
  
        // Streams used for positions and rotations.
        FileStream positionsFile = new FileStream(positionsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter positionsStreamWriter = new StreamWriter(positionsFile);
        FileStream rotationsFile = new FileStream(rotationsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter rotationsStreamWriter = new StreamWriter(rotationsFile);

        for (int i = 0; i < bvh.frameCount; i++)
        {
            int angle = 0;
            // Rotate by degrees, 360/degrees times.
            for (int j = 0; j < 360 / Degrees; j++)
            {
                // Write in positions stream.
                positionsStreamWriter.Write(createPositionTuple(bvh,i,angle, order));
                // Increase the angle by Degrees.
                angle += Degrees;
            }
            // Write in rotation stream. In case we want to use WinterDust methods.
            //rotationsStreamWriter.Write(createRotationsTupleFULLUsingWinterDust(bvh, i, order)); // CHOICE A
        }
        // Close streams and write the rest in file.
        saveRotationsUsingFile(rotationsStreamWriter, filename); // CHOICE B
        positionsStreamWriter.Close();
        rotationsStreamWriter.Close();
    }


    /* In use. */
    /* We save the rotations in file as z,x,y! */
    /* Later, we should parse them and convert them to x,y,z! */
    private void saveRotationsUsingFile(StreamWriter sw, string filename)
    {
        //string alltext = System.IO.File.ReadAllText(filename);                  // Get the whole text.
        //string motion = alltext.Substring(alltext.LastIndexOf("Frame Time:"));  // Skip the Hierarchy part
        string[] lines = File.ReadAllLines(filename);
        int index = 0;
        while (!lines[index].StartsWith("Frame Time"))
        {
            index++;
        }
        index++; // skip that 'Frame Time' line

        // So now, index points to the first line of rotations.
        //int step = (int)Mathf.Round(120/24);         // In case we want to use steps
        for (int i=index; i<lines.Length; i+=1)
        {
            // remove the translation of the hips (3 first words of the line)
            string[] words = lines[i].Split(null); // null means Whitespace
            string rotations = "";
            for(int k=3; k<words.Length; k++)
                rotations += words[k]+" ";

            sw.Write(rotations+"\n");
        }
    }


    private string createPositionTuple(BVH bvh, int frame, int angle, int [] order)
    {
        string tupleStirng = "";
        Matrix4x4 matrix;
        Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            int index = (int)val;
            matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, frame);
            joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }

        BvhProjection newProjection = new BvhProjection(filesCounter, frame, angle, joints);
        newProjection.convertPositionsToRoot();
        // Now, we should Scale the projection:
        float scalingFactor = 1f;
        EnumBONES limbsUsed;
        if (scalingMethod == EnumScaleMethod.SCALE_LIMBS)
            Scaling.getGlobalScaleFactor_USING_LIMBS(joints, out limbsUsed, out scalingFactor);
        else
            scalingFactor = Scaling.getGlobalScaleFactor_USING_HEIGHT(joints);
        newProjection.scalePositions(scalingFactor);
        newProjection.rotatePositions(angle);

        tupleStirng += filesCounter + " " + newProjection.frameNum + " " + newProjection.angle + " ";
        foreach (Vector3 joint in newProjection.joints)
        {
            tupleStirng += joint.x + " " + joint.y + " " + joint.z + " ";
        }
        tupleStirng += "\n";
        return tupleStirng;
    }






    public static float getScaleFactorBVH(BVH bvh, int[] order)
    {
        Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];
        /* Set the global scale factor from frame -1, which is the T-Pose of the bvh. */
        Matrix4x4 matrix;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            int index = (int)val;
            matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, -1);
            // Save the position of joint
            joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }
        BvhProjection newProjection = new BvhProjection(-1,-1, 0, joints);

        // Determine Scaling Method (No need to use previousFactor parameter because
        // scaling factor is applied once in every animation).
        float scalingFactor; EnumBONES limbUSed;
        Scaling.getGlobalScaleFactor_USING_LIMBS(newProjection.joints, out limbUSed, out scalingFactor);
        return scalingFactor;
    }



    // Get positions joints from bvh
    public static Vector3[] getPositionsfromBvh(BVH bvh, int frame, bool normalized = false)
    {

        // Get the order of selected joints
        int[] order = BvhReader.getOrderOfJoints(bvh);
        Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];

        // Scale bvh.
        if (normalized) { 
            float sf = getScaleFactorBVH(bvh, order);
            bvh.scale(sf);
        }

        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            int index = (int)val;
            Matrix4x4 matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, frame);
            // Save the position of joint
            joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }

        // Convert relative to root.
        if (normalized)
        {
            BvhProjection projection = new BvhProjection(0, 0, 0, joints);
            projection.convertPositionsToRoot();
            return projection.joints;
        }

        return joints;
    }



    public static int[] getOrderOfJoints(BVH bvh)
    {
        bool flag = false;
        int[] order = new int[Enum.GetValues(typeof(EnumJoint)).Length];
        int k = 0;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            for (int j = 0; j < bvh.boneCount; j++)
            {
                if (bvh.allBones[j].getName().CompareTo(val.ToString()) == 0)
                {
                    // if bone found
                    order[k] = j;
                    flag = true;
                }
            }
            if (!flag)
            {
                Debug.Log(bvh.alias + " IS NOT VALID!");
                throw new Exception("bvh file has incorrect joints");
            }
            k++;
        }
        return order;
    }

    private void CountSecondsOfClips(string dir)
    {
        int counterFiles = 0;
        double seconds = 0f;
        // For each bvh file in the directory, get the projections.
        string[] fileEntries = Directory.GetFiles(dir);
        foreach (string bvhFileName in fileEntries)
        {
            if (Path.GetExtension(bvhFileName).CompareTo(".bvh") == 0)
            {
                BVH bvh = new BVH(bvhFileName);
                seconds = seconds + bvh.getDurationSec();
                counterFiles++;
            }

        }
        Debug.Log("Total Seconds: " + seconds + " of " + counterFiles + " files.");
    }
}
