using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VMLog : Base {

    public Text text;
	// Use this for initialization
	void Start () {
        text.text = info();
	}
	
    private string info()
    {
        string s = "";
        s += "Created date time: " + sc.log.startDateTime+"\n";
        s += "Input: " + sc.inputDir +"\n";
        s += "Frames: " + sc.frames.Count + "\n";
        s += "People: " + sc.log.people + "\n";
        s += "Algorithm for kNN: " + sc.algNeighbours.ToString() + "\n";
        s += "    Execution Time:" + sc.log.kNNAlgorithmTime +"(ms)"+ "\n";
        s += "Algorithm for Estimation 3D: " + sc.algEstimation.ToString() + "\n";
        s += "    Execution Time:" + sc.log.estimationAlgorithmTime +"(ms)"+ "\n";
        s += "k = "+sc.k +"\n";
        s += "m = " + sc.m + "\n";
        s += "Database: \n";
        s += "Clusters: " + sc.log.NumberOfClusters + "\n";
        s += "Projections: " + sc.log.NumberOfProjections + "\n";
        return s;


    }

	// Update is called once per frame
	void Update () {
		
	}
}
