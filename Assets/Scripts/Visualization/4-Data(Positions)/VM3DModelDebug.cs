using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.IO;


/* OPFigure */
public class VM3DModelDebug : Base 
{
    public Transform model;
    private Model3D m3d;
    public int ProjectionAngle = 10;
    public Material material;
    private int projectionIndex = 0;
    public Text textInfo;
    private GLDraw gL;
    private Vector3[] FigureToShow;
    private int offset;
    private int clusterIndex;
    private int totalProjections = 0;

    private void updateText()
    {
        string s = "Total projections: "+totalProjections+"\n";
        s += "Cluster: " + clusterIndex + "/" + (base_clusters.Count-1) +"\n";
        s += "Projection:"+ projectionIndex + "/" + (base_clusters[clusterIndex].Count-1) + "\n";
        textInfo.text = s;
    }


    private void Awake()
    {
        gL = new GLDraw(material);
        m3d = new Model3D(model);
        totalProjections = base_getNumberOfProjections();
    }

    private void Start()
    {
        FigureToShow = new Vector3[14];
    }

    void Update()
    {
        if (ProjectionAngle == 0 || ProjectionAngle == 1) ProjectionAngle = 360; // so it will go to the next immediate projection
        offset = 360 / ProjectionAngle;

        if(Input.GetKey("w"))
        {
            projectionIndex += offset;
            updateFigureToShow();
            updateText();
            m3d.moveSkeleton(FigureToShow);
        }
        if (Input.GetKey("s"))
        {
            projectionIndex -= offset;
            updateFigureToShow();
            updateText();
            m3d.moveSkeleton(FigureToShow);
        }
    }

    private void updateFigureToShow()
    {
        if (projectionIndex >= base_clusters[clusterIndex].Count)
        {
            if (base_clusters.Count-1 > clusterIndex)
            {
                projectionIndex -= base_clusters[clusterIndex].Count;
                clusterIndex++;
            }
            else
            {
                projectionIndex = 0;
                clusterIndex = 0;
            }
        }
        if(projectionIndex < 0)
        {
            if (clusterIndex != 0)
            {
                clusterIndex--;
                projectionIndex = base_clusters[clusterIndex].Count - 1;
            }
            else
                projectionIndex = 0;
        }
        FigureToShow = base_clusters[clusterIndex][projectionIndex].joints;
    }

    public Vector3[] getJointsToShow()
    {
        return FigureToShow;
    }
}
