using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Linq;
using System.Text;

[System.Serializable()]
public class Scenario  {

    public string inputDir;
    public int k;
    public int m;
    public AlgorithmEstimation algEstimation;
    public AlgorithmSetNeighbours algNeighbours;
    public List<OPFrame> frames;
    public Log log;

    public Scenario(string inputdir, List<OPFrame> knownFrames = null)
    {
        if (knownFrames == null)
            frames = new List<OPFrame>();
        else
            frames = knownFrames;
        inputDir = inputdir;
        k = Base.k;
        m = Base.m;
        algEstimation = Base.Estimation3dAlgorithm;
        algNeighbours = Base.NeighboursAlgorithm;
        log = new Log();
    }


    public void SetInputDir(string dirName)
    {
        inputDir = dirName;
    }

    public void Save()
    {
        // Create directory.
        var myUniqueDirName = string.Format(@"{0}", Guid.NewGuid());
        string dirname = "Scenarios\\" + algEstimation + myUniqueDirName;
        System.IO.Directory.CreateDirectory(dirname);
        Base.base_CurrentDir = dirname;
        // Copy the video in the directory.
        if(File.Exists(@inputDir + "\\video.mp4"))
            File.Copy(@inputDir+"\\video.mp4", dirname+"\\video.mp4");
        // Create and Save the scenario file.
        var myUniqueFileName = string.Format(@"{0}", DateTime.Now.Ticks);

        // Save the log file.
        //SaveLog(this, dirname, myUniqueFileName+".xml");
        //OfflineDataProcessing.WriteToBinaryFile(dirname + @"\Scenario"+myUniqueFileName+".sc",this);
        //OfflineDataProcessing.WriteToBinaryFile(dirname + @"\Estimation" + myUniqueFileName, OfflineDataProcessing.getEstimationArray(0));
        //Debug.Log("Scenario has been saved.");

        // Create and Save bvh file.
        BvhExport export = new BvhExport("TemplateBVH\\"+"template.bvh", OfflineDataProcessing.getEstimationArray(0));
        export.CreateBvhFile(dirname +"\\" + myUniqueFileName);

        // Save ddd.mddd file
        WriteDDDMDDDfile(dirname + "\\UCY_3D.mddd");

        Debug.Log("Bvh has been saved");
    }

    private static void WriteDDDMDDDfile(string fileName)
    {
        Neighbour[] estimation = OfflineDataProcessing.getEstimationArray(0);
        StringBuilder s = new StringBuilder();
        int frameCounter = 0;
        s.AppendLine("University of Cyprus 3D Estimation : Without Filtering");
       
        for(int k=0; k<estimation.Length; k++)
        {
            Neighbour n = estimation[k];
            s.AppendFormat("{0}", frameCounter);
            /*
            while((n == null || n.projection == null || n.projection.joints.Length == 0) && k<estimation.Length )
            {
                n = estimation[++k];
            }
            if (k >= estimation.Length)
                break;
            */
            for (int i = 0; i < n.projection.joints.Length; i++)
            {
                Vector3 joint = n.projection.joints[i];
                s.AppendFormat(", {0}, {1}, {2}", joint.x, joint.y, joint.z);
            }
            s.Append("\n");
            frameCounter++;
        }
        File.WriteAllText(fileName, s.ToString());
    }


    public void SaveLog(Scenario sc, string dir, string filename)
    {
        XDocument logXML = new XDocument(
            new XElement("log",
            new XElement("datetime", sc.log.startDateTime),
            new XElement("input", sc.inputDir),
            new XElement("frames", sc.log.frames),
            new XElement("people", sc.log.people),
            new XElement("kNN", sc.algNeighbours.ToString()),
            new XElement("kNNTime", sc.log.kNNAlgorithmTime),
            new XElement("est", sc.algEstimation.ToString()),
            new XElement("estTime", sc.log.estimationAlgorithmTime),
            new XElement("k", sc.k),
            new XElement("m", sc.m),
            new XElement("clusters", sc.log.NumberOfClusters),
            new XElement("projections", sc.log.NumberOfProjections))
        );
        logXML.Save(dir+"\\"+filename);
        logXML.Save("Scenarios\\"+"Log-Files\\"+filename);
    }

}
