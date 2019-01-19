using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using Winterdust;

/* Installs projections in files. */

public class Start_InstallProjections : MonoBehaviour {

    private EnumScaleMethod scalingMethod = Base.ScaleMethod;

    public string BvhFilesDirectory;
    public string DatabaseDirectory;
    public int Degrees;
    // public int PercentageImport; // NOT USED. All of the frames from bvh file is used
    // in order to avoid asynchronisations between rotations and projections.
    private static int lastFileID;


	void Start () {
        CountSecondsOfClips("Big-Database-smaller\\bvh");
        //lastFileID = getFileCounterFromLogFile();
        //AddInDatabase();
        //logLastID();
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
                lastFileID++;
            }
                
        }
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
        Debug.Log("Total Seconds: "+ seconds+" of "+counterFiles+" files.");
    }



    private void CreateFiles(string bvhfilename)
    {
        string positionsFilePath = DatabaseDirectory+ "\\Projections\\" + (lastFileID)+".p";
        string rotationsFilePath = DatabaseDirectory+ "\\Rotations\\" + (lastFileID);
        SaveProjectionsInFiles(bvhfilename, positionsFilePath, rotationsFilePath);
        //SaveONEProjectionInFiles(bvhfilename, positionsFilePath, rotationsFilePath);
    }


    private void SaveProjectionsInFiles(string filename, string positionsFilePath, string rotationsFilePath)
    {
        // PUT percentageInfo as a parameter, in order to specify a percentage.
        BVH bvh = new BVH(filename);
        int[] order = getOrderOfJoints(bvh);
  
        // Streams used for positions and rotations.
        FileStream positionsFile = new FileStream(positionsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter positionsStreamWriter = new StreamWriter(positionsFile);
        FileStream rotationsFile = new FileStream(rotationsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter rotationsStreamWriter = new StreamWriter(rotationsFile);
        
        // Scale the animation (same scaling as OpenPose).
        bvh.scale(getScaleFactor(bvh, order));
      
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


    private void SaveONEProjectionInFiles(string filename, string positionsFilePath, string rotationsFilePath)
    {
        // PUT percentageInfo as a parameter, in order to specify a percentage.
        BVH bvh = new BVH(filename);
        int[] order = getOrderOfJoints(bvh);

        // Streams used for positions and rotations.
        FileStream positionsFile = new FileStream(positionsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter positionsStreamWriter = new StreamWriter(positionsFile);
        FileStream rotationsFile = new FileStream(rotationsFilePath, FileMode.Append, FileAccess.Write);
        StreamWriter rotationsStreamWriter = new StreamWriter(rotationsFile);

        bvh.scale(getScaleFactor(bvh, order));

        for (int i = 0; i < bvh.frameCount; i++)
        {
            int angle = 0;
                // Write in positions stream.
                positionsStreamWriter.Write(createPositionTuple(bvh, i, angle, order));
                // Increase the angle by Degrees.
                angle += Degrees;
            // Write in rotation stream. In case we want to use WinterDust methods.
            //rotationsStreamWriter.Write(createRotationsTupleFULLUsingWinterDust(bvh, i, order)); // CHOICE A
        }
        // Close streams and write the rest in file.
        saveRotationsUsingFile(rotationsStreamWriter, filename); // CHOICE B
        positionsStreamWriter.Close();
        rotationsStreamWriter.Close();
    }


    /* Not in use. */
    private string createRotationsTuple(BVH bvh, int frame, int [] order)
    {
        string tupleString = "";
        int size = Enum.GetValues(typeof(EnumJoint)).Length;
        for(int i=0; i<size; i++)
        {
            Vector3 rotation = bvh.allBones[order[i]].localFrameRotations[frame].eulerAngles;
            tupleString += rotation.x + " " + rotation.y + " " + rotation.z;
            if (i != size - 1)
                tupleString += " ";
        }
        tupleString += "\n";
        return tupleString;
    }
    /* Commented.*/
    private string createRotationsTupleFULLUsingWinterDust(BVH bvh, int frame, int[] order)
    {
        string tupleString = "";
        int size = bvh.boneCount;
        for (int i = 0; i < size; i++)
        {
            Vector3 rotation = bvh.allBones[i].localFrameRotations[frame].eulerAngles;
            tupleString += rotation.x + " " + rotation.y + " " + rotation.z;
            if (i != size - 1)
                tupleString += " ";
        }
        tupleString += "\n";
        return tupleString;
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

        BvhProjection newProjection = new BvhProjection(lastFileID, frame, angle, joints);
        newProjection.convertPositionsToRoot();
        newProjection.rotatePositions(angle);

        tupleStirng += lastFileID + " " + newProjection.frameNum + " " + newProjection.angle + " ";
        foreach (Vector3 joint in newProjection.joints)
        {
            tupleStirng += joint.x + " " + joint.y + " " + joint.z + " ";
        }
        tupleStirng += "\n";
        return tupleStirng;
    }




    private void logLastID()
    {
        System.IO.File.WriteAllText(DatabaseDirectory + "\\log.txt", (lastFileID)+"");
    }

    private int getFileCounterFromLogFile()
    {
        using (TextReader reader = File.OpenText(DatabaseDirectory+"\\log.txt"))
        {
            return int.Parse(reader.ReadLine());
        }
     
    }




    public float getScaleFactor(BVH bvh, int[] order)
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
        if (scalingMethod == EnumScaleMethod.SCALE_LIMBS)
            return Scaling.getGlobalScaleFactor_USING_LIMBS(newProjection.joints);
        else
            return Scaling.getGlobalScaleFactor_USING_HEIGHT(newProjection.joints);
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


}
