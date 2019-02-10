using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

/* Shared components between classes. */
public static class Base {

    // Variables
    public static EnumScaleMethod ScaleMethod = EnumScaleMethod.SCALE_LIMBS;
    private static string small_DB = "Databases\\v1-18720";
    private static string big_DB = "Databases\\Big-Database";
    public static string Path = small_DB; 
    public static string Clustering = "500-clusters";
    public static string ClusteringMain = "500-clusters";  // not curr used
    public static int numClustersToSearch = 16;
    public static int numMainClustersToSearch = 2;         // not curr used
    public static bool multiLevelClustering = false;

    // Data
    public static List<BvhProjection> base_representatives;          // All representatives.
    public static List<BvhProjection> base_main_representatives;     // All main representatives.
    public static List<List<Rotations>> base_rotationFiles;          // All rotation files.
    public static List<List<BvhProjection>> base_clusters;           // All clusters.
    public static List<List<BvhProjection>> base_main_clusters;      // All main clusters.
    public static List<List<BvhProjection>> base_not_clustered;      // All projections not clustered.

    public static int metadataInFile = 3;                                            // Metadata in a tuple.
    public static int jointsAmount = Enum.GetNames(typeof(EnumJoint)).Length;        // Joints in a tuple.

    // Current scenario to be displayed.
    public static Scenario sc = null;
    public static void SetCurrentScenario(Scenario s) { sc = s; Debug.Log("Scenario has been set."); }
    public static string base_CurrentDir;
    public static int[] base_orderOfComparableRotations = { 4, 2, 8, 9, 10, 5, 6, 7, 14, 15, 16, 11, 12, 13 };



    public static void initialize()
    {
        base_representatives = InitializeRepresentatives(Path + "\\Clusters\\" + Clustering + "\\Representatives\\Representatives");        // All representatives.
        base_rotationFiles = InitializeRotations();              // All rotation files.
        base_clusters = InitializeClusters();                    // All clusters.
        base_not_clustered = InitializeNotClustered();           // All projections not clustered.

        if(multiLevelClustering == true)
        {
            base_main_representatives = InitializeRepresentatives(Path + "\\MainClusters\\" + ClusteringMain + "\\Representatives\\Representatives"); // All main representatives
            base_main_clusters = InitializeMainClusters();           // All main clusters
        }else
        {
            base_main_representatives = null;   // All main representatives
            base_main_clusters = null;          // All main clusters
        }



        Debug.Log("Initialization is done!");
    }

    /**
     * Each line is a representative of a cluster.
     * 1st line is the representative of the 1st cluster etc...
     * 
     * */
    private static List<BvhProjection> InitializeRepresentatives(string filename)
    {
        try {
            StreamReader sr = File.OpenText(filename);
            string tuple = String.Empty;
            List<BvhProjection> list = new List<BvhProjection>();
            while ((tuple = sr.ReadLine()) != null)
            {
                list.Add(ParseIntoProjection(tuple));
            }
            Debug.Log(">Representatives have been read.");
            sr.Close();
            return list;
        } catch(Exception e)
        {
            Debug.Log("Representatives file not found.");
            return null;
        }
        

    }



    private static string[] sortFilesNumerically(string[] fileEntries, string dirName)
    {
        List<string> list = new List<string>();

        // Keep only the filenames (number).
        foreach (string s in fileEntries)
            list.Add(s.Replace(dirName, ""));

        try
        {
            list.Sort((s, t) => System.Collections.Comparer.Default.Compare(Int32.Parse(s), Int32.Parse(t)));
        }
        catch (Exception e)
        {
            Debug.Log("File names should be just numbers!!");
            foreach (string s in list)
                Debug.Log(s);
            throw (e);
        }

        List<string> list2 = new List<string>();
        foreach (string s in list)
        {
            list2.Add(dirName + s);
        }

        fileEntries = list2.ToArray();
        /* Debug selection of files:
         * foreach (string s in fileEntries)
            Debug.Log(s);
        */
        return list2.ToArray();
    }



    private static List<List<BvhProjection>> InitializeClusters()
    {
        string dirName = Path + "\\Clusters\\" + Clustering+"\\";

        List <List<BvhProjection>> listClusters = new List<List<BvhProjection>>();

        // -- Sort file entries by their numerical name. --
        string[] fileEntries = sortFilesNumerically(Directory.GetFiles(dirName),dirName);


        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null )
            {
                try
                {
                    cluster.Add(ParseIntoProjection(tuple, Int32.Parse(fileName.Replace(dirName, ""))));
                }
                catch (Exception e)
                {
                    Debug.Log("dirName is: " + dirName + "and filename is: "+fileName);
                    throw e;
                }
                
            }
            listClusters.Add(cluster);
            sr.Close();
        }
        Debug.Log(">Clustered Projections have been read.");
        return listClusters;
    }



    private static List<List<BvhProjection>> InitializeMainClusters()
    {
        string dirName = Path + "\\MainClusters\\" + ClusteringMain + "\\";

        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();

        // -- Sort file entries by their numerical name. --
        string[] fileEntries = sortFilesNumerically(Directory.GetFiles(dirName), dirName);


        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null)
            {
                try
                {
                    cluster.Add(ParseIntoProjection(tuple, Int32.Parse(fileName.Replace(dirName, ""))));
                }
                catch (Exception e)
                {
                    Debug.Log("dirName is: " + dirName + "and filename is: " + fileName);
                    throw e;
                }

            }
            listClusters.Add(cluster);
            sr.Close();
        }

        Debug.Log(">Clustered Projections have been read.");
        return listClusters;
    }




    private static List<List<BvhProjection>> InitializeNotClustered()
    {
        string dirName = Path+"\\Projections\\";
        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();
        string[] fileEntries = sortFilesNumerically(Directory.GetFiles(dirName), dirName);

        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null)
            {
                cluster.Add(ParseIntoProjection(tuple, Int32.Parse(fileName.Replace(dirName, ""))));
            }
            sr.Close();
            listClusters.Add(cluster);
        }
        Debug.Log(">Unclustered Projections have been read.");
        return listClusters;
    }

    private static List<List<Rotations>> InitializeRotations()
    {
        List<List<Rotations>> listRotationsFiles = new List<List<Rotations>>();
        string dirname = Path + "\\Rotations\\";
        string[] fileEntries = sortFilesNumerically(Directory.GetFiles(dirname), dirname);
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
            sr.Close();
        }
        Debug.Log(">Rotations have been read.");
        return listRotationsFiles;
    }




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

    public static List<OPFrame> getFrames()
    {
        if (sc == null)
            return null;
        else
            return sc.frames;
    }
}
