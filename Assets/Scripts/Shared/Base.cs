using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

/* Shared components between classes. */
public class Base : MonoBehaviour {

    public static string Path = "Database_30";
    public static List<BvhProjection> base_representatives = InitializeRepresentatives();       // All representatives.
    public static List<List<Rotations>> base_rotationFiles = InitializeRotations();             // All rotation files.
    public static List<List<BvhProjection>> base_clusters = InitializeClusters();               // All clusters.
    public static List<List<BvhProjection>> base_not_clustered = InitializeNotClustered();      // All projections not clustered.

    public static int metadataInFile = 3;                                                           // Metadata in a tuple.
    public static int jointsAmount = Enum.GetNames(typeof(EnumJoint)).Length;                       // Joints in a tuple.

    // Current scenario to be displayed.
    public static Scenario sc = null;
    public static void SetCurrentScenario(Scenario s) { sc = s; Debug.Log("Scenario has been set."); }
    public static string base_CurrentDir;
    public static int[] base_orderOfComparableRotations = { 4, 2, 8, 9, 10, 5, 6, 7, 14, 15, 16, 11, 12, 13 };

    /*
    private void Awake()
    {
        base_representatives = InitializeRepresentatives();      // All representatives.
        base_rotationFiles = InitializeRotations();              // All rotation files.
        base_clusters = InitializeClusters();                    // All clusters.
        base_not_clustered = InitializeNotClustered();           // All projections not clustered.

    }
    */

/**
 * Each line is a representative of a cluster.
 * 1st line is the representative of the 1st cluster etc...
 * 
 * */
private static List<BvhProjection> InitializeRepresentatives()
    {
        try {
            string fileName = Path + "\\Representatives\\Representatives";
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            List<BvhProjection> list = new List<BvhProjection>();
            while ((tuple = sr.ReadLine()) != null)
            {
                list.Add(ParseIntoProjection(tuple));
            }
            Debug.Log(">Representatives have been read.");
            return list;
        } catch(Exception e)
        {
            Debug.Log("Representatives file not found.");
            return null;
        }

    }

    private static List<List<BvhProjection>> InitializeClusters()
    {
        string dirName = Path+"\\Clusters\\";
        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();
        string[] fileEntries = Directory.GetFiles(dirName);
     
        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null )
            {
                cluster.Add(ParseIntoProjection(tuple,Int32.Parse(fileName.Replace(dirName,""))));
            }
            listClusters.Add(cluster);
        }
        Debug.Log(">Clusters have been read.");
        return listClusters;
    }


    private static List<List<BvhProjection>> InitializeNotClustered()
    {
        string dirName = Path+"\\Projections\\";
        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();
        string[] fileEntries = Directory.GetFiles(dirName);

        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null)
            {
                cluster.Add(ParseIntoProjection(tuple, Int32.Parse(fileName.Replace(dirName, ""))));
            }
            listClusters.Add(cluster);
        }
        Debug.Log(">Clusters have been read.");
        return listClusters;
    }

    private static List<List<Rotations>> InitializeRotations()
    {
        List<List<Rotations>> listRotationsFiles = new List<List<Rotations>>();
        string dirname = Path + "\\Rotations";
        Debug.Log(dirname);
        string[] fileEntries = Directory.GetFiles(dirname);
        foreach (string fileName in fileEntries)
        {
            List<Rotations> rotationsFile = new List<Rotations>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = string.Empty;
            while ((tuple = sr.ReadLine()) != null)
            {
                rotationsFile.Add(StringToRotations(tuple));
            }
            listRotationsFiles.Add(rotationsFile);
        }
        Debug.Log(">Rotations have been read.");
        return listRotationsFiles;
    }

	

    // --
    /*
    public static int getNearestClusterId()
    {
        return 0; /// <<<<<<<<<< TODO
    }*/


    private static BvhProjection ParseIntoProjection(string tuple, int clusterID=0)
    {
        // tuple format: frame rotation joints[]
        string[] array = tuple.Split(' ');
        int rotationFileID = int.Parse(array[0]);
        int frame = int.Parse(array[1]);
        int angle = int.Parse(array[2]);
        List<Vector3> joints = new List<Vector3>();
        // metadata in file are: fileID_matchWithRotationFiles, frame, degrees
        metadataInFile = 3;
        for (int i = metadataInFile; i < metadataInFile + jointsAmount * 3; i += 3)
        {
            joints.Add(new Vector3(float.Parse(array[i], CultureInfo.InvariantCulture),
                float.Parse(array[i + 1], CultureInfo.InvariantCulture),
                float.Parse(array[i + 2], CultureInfo.InvariantCulture)));
        }
        return new BvhProjection(rotationFileID, frame, angle, joints.ToArray(), clusterID);
    }


    private static Rotations StringToRotations(string tuple)
    {
        List<Vector3> rotations = new List<Vector3>();
        string[] array = tuple.Split(' ');
        
        for (int i = 0; i < array.Length; i += 3)
        {
            if (array[i].CompareTo("")==0)
                continue;
            try
            {

                rotations.Add(new Vector3(
           float.Parse(array[i+1], CultureInfo.InvariantCulture),     // i+1 is x
           float.Parse(array[i+2], CultureInfo.InvariantCulture),     // i+2 is y
           float.Parse(array[i], CultureInfo.InvariantCulture)));     // i   is z

            }
            catch (Exception e)
            {
                Debug.Log("Couldn't convert string: "+array[i] +" _ "+array[i+1]+" _ "+array[i+2]+ " to float.");
                throw e;
            }

           
        }
        return new Rotations(rotations);
    }

    public static int base_getNumberOfProjections()
    {
        int counter = 0;
        foreach (List<BvhProjection> list in base_clusters)
        {
            counter += list.Count;
        }
        return counter;
    }


}
