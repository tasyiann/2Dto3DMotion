using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display_SingleMoreInfo : MonoBehaviour
{
    public DataInFrame script;
    public float speed = 1f;
    public float startingRadius;
    public Vector3 startingPosition = Vector3.zero;
    public float radiusOffset;
    public float perCircleINIT =20f;
    public float perCircleOFFSET= 0.1f;
    public Material mat;

    public float representativeScaling=1f;
    public float projectionsScaling=1f;
    

 
    
    public Color projectionsColor;
    public Color representativeColor;
    public Color neighbourColor = Color.blue;
    public Color winnerColor = Color.green;
    public Vector3 representativePosition;
    public Vector3 projectionsPosition;

    private OPPose figure;
    private GLDraw gL;
    private List<Cluster> clusters;




    void Start()
    {
        gL = new GLDraw(mat);
    }

    private void Update()
    {
        figure = script.selectedPoseToDebug;
        if (figure != null)
        {
            clusters = figure.selectedClusters;
        }



        if (Input.GetKey("d"))
        {
            transform.position += new Vector3(speed, 0f, 0f);
        }
        if (Input.GetKey("a"))
        {
            transform.position -= new Vector3(speed, 0f, 0f);
        }
    }







    private float radius;
    private float FiguresPerCircle;
    private float extraOffset = 50f;
    private void OnPostRender()
    {
        Vector3 newCenter = startingPosition;
        Vector3 offsetToTheNextCluster = Vector3.zero;
        

        if (clusters == null)
        {
            Debug.LogError("Clusters is null!");
            return;
        }

        //Vector3 X_bounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,0f,0f));

        foreach (Cluster c in clusters)
        {
            drawCluster(c, newCenter, ref offsetToTheNextCluster);
            newCenter += new Vector3(offsetToTheNextCluster.x, 0f, 0f);
        }

        
    }

    private void drawCluster(Cluster cluster, Vector3 newCenter, ref Vector3 offsetToTheNextCluster)
    {
        if (cluster == null || cluster.Representative == null)
            return;

        BvhProjection representative = cluster.Representative;
        radius = startingRadius;
        FiguresPerCircle = perCircleINIT;
        gL.drawFigure(true, representativeColor, representative.joints, null, newCenter, representativeScaling);

        int i = 0;
        foreach (BvhProjection p in cluster.projections)
        {
            Color color = determineProjectionColor(p);

            float angle = i * Mathf.PI * 2f / FiguresPerCircle;
            projectionsPosition = new Vector3(newCenter.x + Mathf.Sin(angle) * radius, newCenter.y + Mathf.Cos(angle) * radius, newCenter.z);  // Update the pos for the next projection.
            gL.drawFigure(true, color, p.joints, null, projectionsPosition, projectionsScaling);                           // Draw the projection.

            radius += radiusOffset;
            FiguresPerCircle += perCircleOFFSET;
            i++;
        }
        float boundingBoxValue = Vector3.Distance(newCenter, projectionsPosition) + extraOffset;
        offsetToTheNextCluster = new Vector3(boundingBoxValue,boundingBoxValue,0f);
    }

    private Color determineProjectionColor(BvhProjection p)
    {
        List<Neighbour> neighbours = figure.neighbours;
        foreach (Neighbour n in neighbours)
        {
            if (figure.Estimation3D.projection == p)
            {
                return winnerColor;
            }
            if (n.projection == p)
            {
                return neighbourColor;
            }
        }
        return projectionsColor;
    }

}
