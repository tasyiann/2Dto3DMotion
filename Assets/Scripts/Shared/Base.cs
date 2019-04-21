using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using UnityEngine.UI;
using System.Threading;
using System.Text;

/* Shared components between classes. */
public static class Base
{
    #region vars
    // State
    public static bool isBaseInitialized = false;

    // Variables
    private static bool alreadyInitialized = false;
    public static string Path;
    public static int numClustersToSearch;
    public static int projectionsPerFrame;
    public static EnumScaleMethod ScaleMethod;
    public static int k;
    public static int m;
    public static AlgorithmEstimation Estimation3dAlgorithm;
    public static AlgorithmSetNeighbours NeighboursAlgorithm;
    public static int[] base_orderOfComparableRotations;
    public static MatlabUnitySocket MatlabSocket;

    // Data
    public static List<List<Rotations>> base_rotationFiles;          // All rotation files.
    public static List<Cluster> base_clusters;                       // All clusters.
    public static List<List<BvhProjection>> base_not_clustered;      // All projections not clustered.

    public static int metadataInFile = 3;                                            // Metadata in a tuple.
    public static int numberOfJoints = Enum.GetNames(typeof(EnumJoint)).Length;        // Joints in a tuple.

    // Current scenario to be displayed.
    public static Scenario sc = null;
    public static void SetCurrentScenario(Scenario s) { sc = s; Debug.Log("NEW Scenario has been set. Input: " + sc.inputDir); }
    public static string base_CurrentDir;

    public static Text progressInfoText;
    public static Thread thread_A;
    public static Thread thread_B;

    #endregion vars

    public static void initializeMatlabCommunication()
    {
        MatlabSocket = new MatlabUnitySocket();
        MatlabSocket.setupSocket();
    }

    private static void initialise_DataBase_Variables()
    {
        alreadyInitialized = false;
        Path = DataBaseParametersReader.Instance.Parameters.databasePath;
        projectionsPerFrame = DataBaseParametersReader.Instance.Parameters.projectionsPerFrame;
        ScaleMethod = DataBaseParametersReader.Instance.Parameters.ScaleMethod;
    }

    private static void initialise_Algorithms_Variables()
    {
        numClustersToSearch = AlgorithmsParametersReader.Instance.Parameters.numberOfClustersToSearch;
        k = AlgorithmsParametersReader.Instance.Parameters.k;
        m = AlgorithmsParametersReader.Instance.Parameters.m;
        Estimation3dAlgorithm = AlgorithmsParametersReader.Instance.Parameters.estimation3dAlgorithm;
        NeighboursAlgorithm = AlgorithmsParametersReader.Instance.Parameters.neighboursAlgorithm;
        base_orderOfComparableRotations = AlgorithmsParametersReader.Instance.Parameters.orderOfComparableRotations;
    }

    public static void Threads_StartInit()
    {
        // Initialize system parameters.
        initialise_DataBase_Variables();    // < important! :)
        initialise_Algorithms_Variables();  // < important! :)

        if (alreadyInitialized) return;

        thread_A = new Thread(new ThreadStart(execute_thread_A));
        thread_A.Start();

        thread_B = new Thread(new ThreadStart(execute_thread_B));
        thread_B.Start();

        alreadyInitialized = true;
    }

    public static bool areThreadsDone()
    {
        return !thread_A.IsAlive && !thread_B.IsAlive;
    }

    public static void writeAnimation3DPointsInCSVFile()
    {
        StringBuilder s = new StringBuilder();
        int counter = 0;
        int max = 5000;
        s.AppendFormat("University of Cyprus 3D Points: Sample from CMU DataBase");
        foreach (List<BvhProjection> file in base_not_clustered)
        {
            if (counter > max)
                break;
            for(int i=0; i<file.Count; i+= projectionsPerFrame)
            {
                s.AppendFormat("{0}", counter);
                Vector3[] joints = file[i].joints;
                foreach(Vector3 joint in joints)
                {
                    s.AppendFormat(", {0}, {1}, {2}", joint.x, joint.y, joint.z);
                }
                s.AppendFormat("\n");
                counter++;
                if (counter > max)
                    break;
            }
        }
        File.WriteAllText("CMU3DPoints.sample", s.ToString());
        Debug.Log("File CMU Sample 3D Points has been exported!");
    }

    public static int getNumberOfRotations()
    {
        int counter = 0;
        foreach (List<Rotations> r in base_rotationFiles)
        {
            counter += r.Count;
        }
        return counter;
    }

    private static void execute_thread_A()
    {
        string projDir = Path + @"\Projections\";
        base_not_clustered = InitializeNotClustered(projDir);
        string rotDir = Path + @"\Rotations\";
        base_rotationFiles = InitializeRotations(rotDir);
    }

    private static void execute_thread_B()
    {
        string clustDir = Path + @"\Clusters\";
        base_clusters = InitializeClusters(clustDir);
    }

    private static string[] sortFilesNumerically(string[] fileEntries, string dirName)
    {
        List<string> list = new List<string>();

        // Keep only the filenames (number).
        foreach (string s in fileEntries)
            list.Add(s.Replace(dirName, "").Replace("\\","").Replace("/",""));
     
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

    public static List<Cluster> InitializeClusters(string clustersDirPath)
    {
        List<Cluster> listClusters = new List<Cluster>();
        string[] fileEntries = Directory.GetFiles(clustersDirPath); // Each file, is a group (as defined in thesis).
        foreach (string fileName in fileEntries)
        {

            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            int clusterNumber = -1;
            List<BvhProjection> listOfProjections = new List<BvhProjection>();
            Cluster cluster = null;
            while ((tuple = sr.ReadLine()) != null)
            {
                // A cluster is initiate with a 'Cx' tag at the top.
                if (tuple.StartsWith("C" + (clusterNumber+1)))
                {
                    // Finish and Sort previous cluster.
                    if(cluster!=null)
                        cluster.SortCluster();

                    // Enter into the new cluster
                    clusterNumber = clusterNumber + 1;
                    // New cluster starts -> initialize listOfProjection which is the content of the cluster.
                    listOfProjections = new List<BvhProjection>();
                    cluster = new Cluster(listOfProjections);
                    listClusters.Add(cluster);
                    // The representative of the cluster is the first tuple, after the 'Cx' tag.
                    tuple = sr.ReadLine();
                    BvhProjection p = ParseIntoProjection(tuple);
                    if (p != null)
                        cluster.setRepresentative(p);
                    else
                        Debug.LogError("Representative of cluster "+clusterNumber+ " is undefined. Can not create cluster without its representative.");
                }
                else
                {
                    listOfProjections.Add(ParseIntoProjection(tuple, clusterNumber));
                }
            }
            sr.Close();
        }
        Debug.Log(">Clustered Projections have been read.");
        return listClusters;
    }

    public static List<List<BvhProjection>> InitializeNotClustered(string dirName)
    {

        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();
        if (!Directory.Exists(dirName))
        {
            Debug.LogWarning("Couldn't find Directory " + dirName);
            return null;
        }
        string[] fileEntries = sortFilesNumerically(Directory.GetFiles(dirName), dirName);

        foreach (string fileName in fileEntries)
        {
            List<BvhProjection> cluster = new List<BvhProjection>();
            StreamReader sr = File.OpenText(fileName);
            string tuple = String.Empty;
            while ((tuple = sr.ReadLine()) != null)
            {
                BvhProjection p = ParseIntoProjection(tuple, Int32.Parse(fileName.Replace(dirName, "")));
                if(p!=null)
                    cluster.Add(p);
            }
            sr.Close();
            listClusters.Add(cluster);
        }
        Debug.Log(">Unclustered Projections have been read.");
        return listClusters;
    }

    public static List<List<Rotations>> InitializeRotations(string dirname)
    {
        List<List<Rotations>> listRotationsFiles = new List<List<Rotations>>();
        if( !Directory.Exists(dirname) )
        {
            Debug.LogWarning("Couldn't find Directory " + dirname);
            return null;
        }
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
        base_rotationFiles = listRotationsFiles;
        return listRotationsFiles;
    }

    private static BvhProjection ParseIntoProjection(string tuple, int clusterID = 0)
    {
        // tuple format: frame rotation joints[]
        int rotationFileID = 0, frame = 0, angle = 0;
        List<Vector3> joints = new List<Vector3>();
        try
        {
            string[] array = tuple.Split(' ');
            rotationFileID = int.Parse(array[0]);
            frame = int.Parse(array[1]);
            angle = int.Parse(array[2]);
            joints = new List<Vector3>();
            // metadata in file are: fileID_matchWithRotationFiles, frame, degrees
            metadataInFile = 3;
            for (int i = metadataInFile; i < metadataInFile + numberOfJoints * 3; i += 3)
            {
                joints.Add(new Vector3(float.Parse(array[i], CultureInfo.InvariantCulture),
                float.Parse(array[i + 1], CultureInfo.InvariantCulture),
                float.Parse(array[i + 2], CultureInfo.InvariantCulture)));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Cluster"+clusterID+" There is an error in this line: " + tuple);
            return null;
        }
        return new BvhProjection(rotationFileID, frame, angle, joints.ToArray(), clusterID);
    }

    public static Rotations StringToRotations(string tuple)
    {
        List<Vector3> rotations = new List<Vector3>();
        string[] array = tuple.Split(' ');

        for (int i = 0; i < array.Length; i += 3)
        {
            if (array[i].CompareTo("") == 0)
                continue;
            try
            {

                rotations.Add(new Vector3(
           float.Parse(array[i + 1], CultureInfo.InvariantCulture),     // i+1 is x
           float.Parse(array[i + 2], CultureInfo.InvariantCulture),     // i+2 is y
           float.Parse(array[i], CultureInfo.InvariantCulture)));       // i   is z

            }
            catch (Exception e)
            {
                Debug.Log("Couldn't convert string: " + array[i] + " _ " + array[i + 1] + " _ " + array[i + 2] + " to float.");
                throw e;
            }


        }
        return new Rotations(rotations);
    }

    public static int base_getNumberOfProjections()
    {
        int counter = 0;
        if (base_not_clustered == null)
            return 0;
        foreach (List<BvhProjection> p in base_not_clustered)
        {
            counter += p.Count;
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

    public static List<BvhProjection> getRepresentativesOnly()
    {
        List<BvhProjection> representatives = new List<BvhProjection>();
        foreach (Cluster cluster in base_clusters)
        {
            representatives.Add(cluster.Representative);
        }
        return representatives;
    }
}
