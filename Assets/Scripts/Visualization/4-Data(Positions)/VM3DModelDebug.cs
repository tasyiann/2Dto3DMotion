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
    public bool clustered;
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
    private List<List<BvhProjection>> data;

    private void updateText()
    {
        string s = "";
        s += "Cluster: " + clusterIndex + "/" + (data.Count-1) +"\n\n";
        s += "Projection:"+ projectionIndex + "/" + (data[clusterIndex].Count-1) + "\n";
        s+= "Total projections: "+totalProjections+"\n";
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
        if (clustered)
        {
            data = Base.base_clusters;
        }
        else
        {
            data = Base.base_not_clustered;
        }

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
        if (projectionIndex >= data[clusterIndex].Count)
        {
            if (data.Count-1 > clusterIndex)
            {
                projectionIndex -= data[clusterIndex].Count;
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
                projectionIndex = data[clusterIndex].Count - 1;
            }
            else
                projectionIndex = 0;
        }
        FigureToShow = data[clusterIndex][projectionIndex].joints;
    }

    public Vector3[] getJointsToShow()
    {
        return FigureToShow;
    }
}
