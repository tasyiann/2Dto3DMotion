using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.IO;


/* OPFigure */
public class VM3DModelDebug : MonoBehaviour 
{
    public bool clustered;
    private float distanceFromCluster;
    public Transform model_clusters;
    private Model3D m3d_clusters;

    public Transform model_representative;
    private Model3D m3d_representative;

    public int ProjectionAngle = 12;
    public Material material;
    public int projectionIndex = 0;
    public Text textInfo;
    public Text cluster_title;
    public Text projection_id_text;
    private GLDraw gL;

    private Vector3[] FigureToShow_cluster;
    private Vector3[] FigureToShow_representative;

    private int offset;
    public int clusterIndex;
    private int totalProjections = 0;
    private List<Cluster> clusters = Base.base_clusters;

    private string p_data;
    private string r_data;

    private void updateText()
    {
        string s = "";
        s += "Cluster: " + clusterIndex + "/" + (clusters.Count-1) +"\n";
        s += "Distance from Representative: " + distanceFromCluster+"\n";
        s += "Projection:"+ projectionIndex + "/" + (clusters[clusterIndex].projections.Count-1) + "\n";
        s+= "Total projections: "+totalProjections+"\n";
        textInfo.text = s;
        cluster_title.text = "Cluster: "+clusterIndex+" / "+ (clusters.Count - 1);
        projection_id_text.text = "Projection:" + projectionIndex + "/" + (clusters[clusterIndex].projections.Count - 1);
    }


    private void Awake()
    {
        gL = new GLDraw(material);
        m3d_clusters = new Model3D(model_clusters);
        m3d_representative = new Model3D(model_representative);
        totalProjections = Base.base_getNumberOfProjections();
    }

    private void Start()
    {
        FigureToShow_cluster = new Vector3[14];
        FigureToShow_representative = new Vector3[14];
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
            m3d_clusters.moveSkeleton(FigureToShow_cluster);
            m3d_representative.moveSkeleton(FigureToShow_representative);
        }
        if (Input.GetKey("s"))
        {
            projectionIndex -= offset;
            updateFigureToShow();
            updateText();
            m3d_clusters.moveSkeleton(FigureToShow_cluster);
            m3d_representative.moveSkeleton(FigureToShow_representative);
        }
    }

    private void updateFigureToShow()
    {
        if (projectionIndex >= clusters[clusterIndex].projections.Count)
        {
            if (clusters.Count-1 > clusterIndex)
            {
                projectionIndex -= clusters[clusterIndex].projections.Count;
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
                projectionIndex = clusters[clusterIndex].projections.Count - 1;
            }
            else
                projectionIndex = 0;
        }
        FigureToShow_cluster = clusters[clusterIndex].projections[projectionIndex].joints;
        FigureToShow_representative = clusters[clusterIndex].representative.joints;
        float distance = clusters[clusterIndex].projections[projectionIndex].Distance2D(clusters[clusterIndex].representative);
        if ( distance < 0.1f )
            Debug.Log("<<<<<<< found!>>>>>>>");
        distanceFromCluster = distance;
    }

    private string displayJoints(Vector3[] joints)
    {
        string s = "";
        foreach (Vector3 j in joints )
        {
            s += j.x + " " + j.y + " " + j.z + " ";
        }
        return s;
    }


    private void OnPostRender()
    {
        gL.drawFigure(true, Color.white, FigureToShow_cluster, null, new Vector3(model_clusters.transform.position.x, 0, 0));
        gL.drawFigure(true, Color.white, FigureToShow_representative, null, new Vector3(model_representative.transform.position.x, 0, 0));
    }


    
}
