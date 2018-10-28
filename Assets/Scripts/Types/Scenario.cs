using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Linq;

[System.Serializable()]
public class Scenario  {

    public string inputDir;
    public int k;
    public int m;
    public AlgorithmEstimation algEstimation;
    public AlgorithmSetNeighbours algNeighbours;
    public List<OPFrame> frames;
    public Log log;

    public Scenario(string inputdir, int kNN, int mWindow, AlgorithmEstimation algEst, AlgorithmSetNeighbours algNN)
    {
        inputDir = inputdir;
        k = kNN;
        m = mWindow;
        algEstimation = algEst;
        algNeighbours = algNN;
        log = new Log();
    }

    public void SetFrames(List<OPFrame> framesFromOpenPose)
    {
        frames = framesFromOpenPose;
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
        DataParsing.WriteToBinaryFile(dirname+"\\"+myUniqueFileName+".sc", this);
        // Save the log file.
        SaveLog(this, dirname, myUniqueFileName+".xml");
        Debug.Log("Scenario has been saved.");
        // Create and Save bvh file.
        BvhExport export = new BvhExport("TemplateBVH\\"+"template.bvh", DataParsing.estimation);
        export.CreateBvhFile(dirname +"\\" + myUniqueFileName+".bvh");
        Debug.Log("Bvh has been saved");
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
