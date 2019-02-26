using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.IO;


/* OPFigure */
public class VMProjections : MonoBehaviour 
{
    public BvhProjection mainFigure;        // Main figure.
    public List<BvhProjection> projections; // Projections of the main figure.
    public Transform model;                 // 

    private Model3D m3d;

    public bool showGL;
    public int ProjectionAngle = 12;
    public Material material;
    public int projectionIndex = 0;
    public Text textInfo;
    private GLDraw gL;

    private Vector3[] FigureToShow_projections;

    private int offset;
    public int clusterIndex;
    private int totalProjections = 0;
    private List<List<BvhProjection>> data = Base.base_not_clustered;

    private void updateText()
    {
        string s = "";
        s += "Projection:"+ projectionIndex + "/" + (data[clusterIndex].Count-1) + "\n";
        s+= "Total projections: "+totalProjections+"\n";
        textInfo.text = s;
    }


    private void Awake()
    {
        gL = new GLDraw(material);
        m3d = new Model3D(model);
        m3d = new Model3D(model);
        totalProjections = Base.base_getNumberOfProjections();
    }

    private void Start()
    {
        FigureToShow_projections = new Vector3[14];
        projections = new List<BvhProjection>();
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
            m3d.moveSkeleton(FigureToShow_projections);
        }
        if (Input.GetKey("s"))
        {
            projectionIndex -= offset;
            updateFigureToShow();
            updateText();
            m3d.moveSkeleton(FigureToShow_projections);
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

        // << Export Data >>
        mainFigure = data[clusterIndex][projectionIndex];
        projections = new List<BvhProjection>();
        for (int i=0; i<offset; i++)
        {
            projections.Add(data[clusterIndex][projectionIndex+i]);
        }

        FigureToShow_projections = mainFigure.joints;

    }




    private void OnPostRender()
    {
        if(showGL)
            gL.drawFigure(true, Color.white, FigureToShow_projections, null, new Vector3(model.transform.position.x, 0, 0));
    }


    
}
