using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


public class DataParsing {

    private static Scenario sc = Base.sc;
    private static List<List<BvhProjection>> base_clusters = Base.base_clusters;
    private static List<BvhProjection> base_representatives = Base.base_representatives;
    private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;

    public static Neighbour[] estimation;      // Temporary for debugs. (review this comment please.)
    private static float kNNAlgorithmTime;
    private static float EstimationAlgorithmTime;
    private static List<OPFrame> frames;

    public static void Calculate3D()
    {
        // Read the OpenPose output (JSON files).
        OpenPoseJSON parser = new OpenPoseJSON();
        frames = parser.parseAllFiles(sc.inputDir);
        sc.SetFrames(frames);



        Debug.Log("Frames (from OpenPose JSON) have been set.");

        try
        {
            // Execute the algorithms for person [0].

            var watch = System.Diagnostics.Stopwatch.StartNew();
            generateNeighbours();
            watch.Stop();
            kNNAlgorithmTime = watch.ElapsedMilliseconds;

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            generateEstimations();
            watch2.Stop();
            EstimationAlgorithmTime = watch2.ElapsedMilliseconds;
        
            // Save the estimation as an array in a variable, so it can be used from other scripts.
            estimation = getEstimationArray();
        }
        catch (Exception e)
        {
            Debug.Log("Error. Couldn't execute algorithms: "+sc.algNeighbours+" "+ sc.algEstimation);
            throw e;
            
        }
        // Set log
        setLog();
        // Save the scenario.
        sc.Save();
        Debug.Log("Done.");
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

    public static void CalculateEstimation()
    {
        // Save the estimation as an array in a variable, so it can be used from other scripts.
        estimation = getEstimationArray();
    }

    private static void generateNeighbours()
    {
        foreach (OPFrame frame in sc.frames)
        {
            // Case 1: None figures found in this frame.
            if (frame.figures.Count == 0)
                continue;
            // Case 2: Figure exists. Use only the figure[0]
            sc.algNeighbours.SetNeighbours(frame.figures[0], sc.k, base_clusters, base_representatives);
        }
    }



    private static void generateEstimations()
    {
        OPPose previous = null;
        for (int i = 0; i < sc.frames.Count; i++)
        {
            // Case 1: None figures found in this frame.
            if (sc.frames[i].figures.Count == 0)
            {
                previous = null;
                continue;
            }
            // Case 2: Figure exists. Use only the figure[0]
            sc.frames[i].figures[0].selectedN = sc.algEstimation.GetEstimation(previous, sc.frames[i].figures[0], sc.m, base_rotationFiles);
            previous = sc.frames[i].figures[0];
        }
    }



    private static Neighbour[] getEstimationArray()
    {
        List<Neighbour> result = new List<Neighbour>();
        // Make sure scenario is the updated one from Base.
        sc = Base.sc;
        foreach (OPFrame frame in sc.frames) // <<<<<<<
        {
            if (frame.figures.Count == 0)
                result.Add(null);
            else
                result.Add(frame.figures[0].selectedN);
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