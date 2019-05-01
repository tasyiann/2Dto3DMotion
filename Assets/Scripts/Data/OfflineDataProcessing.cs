using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


public class OfflineDataProcessing
{

    private static Scenario sc;
    private static List<OPFrame> frames;
    private static List<Cluster> base_clusters = Base.base_clusters;                        // Clustered projections.
    private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;      // Rotations.


    private static float kNNAlgorithmTime;            // Execution time of k-BM Algorithm.
    private static float EstimationAlgorithmTime;     // Execution time of Best 3D Algorithm.


    /// <summary> The pipeline of estimating the 3D of OpenPose Output.</summary>
    public static void OFFLINE_Pipeline()
    {
        sc = Base.sc;       // Scenario to be saved.
        frames = sc.frames;
        if (Base.base_clusters == null || Base.base_not_clustered == null || Base.base_rotationFiles == null)
        {
            Debug.LogError("PLEASE RUN THE PROPER SCENE TO LOAD THE DATABASE!");
            return;
        }

        // First thing to do: INITIALISE 
        Debug.Log("Entering OFFLINE mode.");
        // Read next frame OpenPose output (JSON files)
        int frameCounter = 0;
        OpenPoseJSON parser = new OpenPoseJSON(frames);
        string[] fileEntries = Directory.GetFiles(sc.inputDir);
        foreach (string fileName in fileEntries)
        {
            if (Path.GetExtension(fileName).CompareTo(".json") == 0)
            {

                OPFrame currFrame = parser.Parsefile(fileName, frameCounter);
                frames.Add(currFrame);
                // For each figure in the frame, calculate its 3D:

                foreach (OPPose currFigure in currFrame.figures)
                {
                    // STEP_A: Find k-BM.
                    sc.algNeighbours.SetNeighbours(currFigure, sc.k, base_clusters);
                    // Assign the previous figure of the current figure.
                    currFigure.prevFigure = setPreviousFigure(frameCounter, currFigure);
                    // STEP_B: Find Best 3D.
                    currFigure.Estimation3D = sc.algEstimation.SetEstimation(currFigure, sc.m, base_rotationFiles);
                    // Offline implementation is done.
                }
                frameCounter++;
            }
        }
        Debug.Log("Estimation Done.");
        coverEmptyEstimations(0);
        Debug.Log("Removed null estimations for person '0'");
        // write3DEstimation_POSITIONS_PER_JOINT_inFile(@"SGolayTests\Test2-Without-Restriction");
        // ApplySGOLAYViaMatlab();
        // Set log
        setLog();
        // Save the scenario.
        sc.Save();
        Debug.Log("Saving BVH Done.");
    }


    private static OPPose setPreviousFigure(int frameCounter, OPPose currFigure)
    {
        if (frameCounter != 0)
        {
            // Check if figure exists in that frame.
            if (frames[frameCounter - 1].figures.Count <= currFigure.id)
            {
                Debug.Log("ID: "+currFigure.id+", PREV IS NULL. SORRY!");
                return null;
            }
                
            else
                return frames[frameCounter - 1].figures[currFigure.id];
        }
        return null;
    }

    private static void ApplySGOLAYViaMatlab()
    {
        string dir3DEstimation = Path.GetFullPath(@"3DEstimations");
        write3DEstimation_POSITIONS_PER_JOINT_inFile(dir3DEstimation);
        // << SGOLAY MATLAB >>
        Base.initializeMatlabCommunication();
        Sgolay sgolay = new Sgolay(Base.MatlabSocket);
        sgolay.ApplySgolay(dir3DEstimation);
    }



    private static void write3DEstimation_POSITIONS_inFile(string FilePath)
    {
        // Iterate frames, and create a list of the 3D estimation frames, for each figure appeared in video.
        Neighbour[] estimation = getEstimationArray(0);
        string s = "";
        foreach (Neighbour n in estimation)
        {
            Vector3[] joints = n.projection.joints;
            foreach (Vector3 joint in joints)
            {
                s += joint.x + " " + joint.y + " " + joint.z;
            }
            s += "\n";
        }
        File.WriteAllText(FilePath, s);
    }


    private static void write3DEstimation_POSITIONS_PER_JOINT_inFile(string DirectoryPath)
    {
        // Iterate frames, and create a list of the 3D estimation frames, for each figure appeared in video.
        Neighbour[] estimation = getEstimationArray(0);
        int num_of_joints = Enum.GetNames(typeof(EnumJoint)).Length;
        string[] s = new string[num_of_joints];
        foreach (Neighbour n in estimation)
        {
            Vector3[] joints = n.projection.joints;
            for (int i = 0; i < num_of_joints; i++)
            {
                Vector3 joint = joints[i];
                s[i] += joint.x + " " + joint.y + " " + joint.z + "\n";
            }
        }

        for (int i = 0; i < num_of_joints; i++)
        {
            File.WriteAllText(DirectoryPath + @"\" + i + "_joint.3D", s[i]);
        }
    }





    private void write3DEstimation_ROTATIONS_inFile()
    {

    }


    public static void setLog()
    {
        sc.log.startDateTime = DateTime.Now.ToString("yyyy MM dd HH mm ss");
        sc.log.frames = frames.Count;
        sc.log.people = 1;
        sc.log.NumberOfClusters = base_clusters.Count;
        sc.log.NumberOfProjections = Base.base_getNumberOfProjections();
        sc.log.kNNAlgorithmTime = kNNAlgorithmTime;
        sc.log.estimationAlgorithmTime = EstimationAlgorithmTime;
    }


    /// <summary>
    /// Please call this method for 0, after 1, after 2 as person_index.
    /// </summary>
    /// <param name="person_index"></param>
    private static void coverEmptyEstimations(int person_index)
    {
        //foreach (OPFrame frame in frames)
        for (int k = 0; k < frames.Count; k++)
        {
            int currentIndex = k;
            OPFrame frame = frames[k];

            // If there is no estimation for person_index in this frame, then assigned the next existing one.
            Neighbour n;

            while ( currentIndex < frames.Count &&
                (frames[currentIndex].figures.Count <= person_index ||
                frames[currentIndex].figures[person_index] == null ||
                (n = frames[currentIndex].figures[person_index].Estimation3D) == null ||
                n.projection == null || n.projection.joints.Length == 0) 
                )
            {
                currentIndex++;
            }

            if (currentIndex >= frames.Count)
                break;

            // Add figures if missing.
            if (frame.figures.Count <= person_index)
                frame.figures.Add(frames[currentIndex].figures[person_index]);

            frame.figures[person_index].Estimation3D = frames[currentIndex].figures[person_index].Estimation3D;
        }
    }



    public static Neighbour[] getEstimationArray(int person_index)
    {
        List<Neighbour> result = new List<Neighbour>();
        // Make sure scenario is the updated one from Base. <<< Why?
        foreach (OPFrame frame in frames)
        {
            // CHECK IF THIS ID EXIST IN THE FRAME! fIND A waY tO do ThaT
            // Debug.Log("Frame:" + frame.number + "\nFigures count: " + frame.figures.Count + "\nIndex: " + person_index);
            // Check if figure exist in that frame.
            if (frame.figures.Count <= person_index || frame.figures[person_index] == null)
                result.Add(null);
            else
                result.Add(frame.figures[person_index].Estimation3D);
        }
        return result.ToArray();
    }





    // Please delete this
    private static void saveClustersAndNeighbours()
    {
        WriteToBinaryFile(@"Temp-ClustersAndNeighbours\CRAZY.bin", frames[16].figures[0]);
        WriteToBinaryFile(@"Temp-ClustersAndNeighbours\clusters.bin", frames[16].figures[0].selectedClusters);
        WriteToBinaryFile(@"Temp-ClustersAndNeighbours\neighbours.bin", frames[16].figures[0].neighbours);
    }


    public static void WriteToBinaryFile(string fileName, System.Object obj)
    {

        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
        QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSS);
        bf.SurrogateSelector = surrogateSelector;

        FileStream file = File.Create(fileName);

        bf.Serialize(file, obj);
        file.Close();
    }


    public static System.Object readBinaryfile(string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();
        QuaternionSerializationSurrogate quaternionSS = new QuaternionSerializationSurrogate();


        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSS);
        bf.SurrogateSelector = surrogateSelector;

        FileStream file = File.Open(fileName, FileMode.Open);
        object result = bf.Deserialize(file);
        file.Close();
        return result;
    }



}







