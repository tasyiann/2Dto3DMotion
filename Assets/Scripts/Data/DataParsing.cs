using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


public class DataParsing
{

    private static Scenario sc = Base.sc;                                                   // Scenario to be saved.
    private static List<OPFrame> frames = Base.getFrames();                                 // The same instance as scenario (Please review this).
    private static List<List<BvhProjection>> base_clusters = Base.base_clusters;            // Clustered projections.
    private static List<BvhProjection> base_representatives = Base.base_representatives;    // Representatives.
    public static List<List<BvhProjection>> base_main_clusters = Base.base_main_clusters;   // All main clusters.
    public static List<BvhProjection> base_main_representatives = Base.base_main_representatives;// All main representatives.
    private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;      // Rotations.

    public static Neighbour[] estimation;             // Estimation to Debug. We will not debug all figures at the same time.
    private static float kNNAlgorithmTime;            // Execution time of k-BM Algorithm.
    private static float EstimationAlgorithmTime;     // Execution time of Best 3D Algorithm.


    /// <summary> The pipeline of estimating the 3D of OpenPose Output.</summary>
    public static void OFFLINE_Pipeline()
    {
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
                    sc.algNeighbours.SetNeighbours(currFigure, sc.k, base_clusters, base_representatives, base_main_representatives, base_main_clusters);
                    // STEP_B: Find Best 3D.
                    OPPose prevFigure = null;
                    if (frameCounter != 0)
                    {
                        // Get access to the previous frame figure with the same ID. How? Figure it out!
                        // << Attention. It might be null (not existed in prev frame).
                        // I Need to handle this.
                        try
                        {
                            // Check if figure exists in that frame.
                            if (frames[frameCounter - 1].figures.Count <= currFigure.id)
                                prevFigure = null;
                            else
                                prevFigure = frames[frameCounter - 1].figures[currFigure.id];
                        }
                        catch(ArgumentOutOfRangeException e)
                        {
                            Debug.Log(e.Message + "\n" + e.StackTrace);
                            prevFigure = null;
                        }
                        
                    }
                    currFigure.selectedN = sc.algEstimation.GetEstimation(prevFigure, currFigure, sc.m, base_rotationFiles);
                    // Offline implementation is done. But with real-time, we need to display each frame.
                    // So, we need somehow to render the current 3D on screen. (On every input from pipes).
                }
                frameCounter++;
            }
        }
        // Iterate frames, and create a list of the 3D estimation frames, for each figure appeared in video.
        estimation = getEstimationArray(0);
        Debug.Log("Estimation Done.");
        // Set log
        setLog();
        // Save the scenario.
        sc.Save();
        Debug.Log("Saving Done.");
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



    private static Neighbour[] getEstimationArray(int person_index)
    {
        List<Neighbour> result = new List<Neighbour>();
        // Make sure scenario is the updated one from Base. <<< Why?
        foreach (OPFrame frame in frames)
        {
            // CHECK IF THIS ID EXIST IN THE FRAME! fIND A waY tO do ThaT
            Debug.Log("Frame:" + frame.number + "\nFigures count: " + frame.figures.Count + "\nIndex: " + person_index);
            // Check if figure exist in that frame.
            if (frame.figures.Count <= person_index || frame.figures[person_index] == null)
                result.Add(null);
            else
                result.Add(frame.figures[person_index].selectedN);
        }
        return result.ToArray();
    }





    public static void WriteToBinaryFile(string fileName, System.Object obj)
    {

        BinaryFormatter bf = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
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

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        bf.SurrogateSelector = surrogateSelector;

        FileStream file = File.Open(fileName, FileMode.Open);
        return bf.Deserialize(file);
    }



}