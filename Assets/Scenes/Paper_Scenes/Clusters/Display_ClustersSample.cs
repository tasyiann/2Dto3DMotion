using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display_ClustersSample : MonoBehaviour
{
    public VM3DModelDebug script;
    public int currentCluster;
    public float startingRadius;
    public float radiusOffset;
    public float perCircleINIT =20f;
    public float perCircleOFFSET= 0.1f;

    public Material mat;

    public float representativeScaling=1f;
    public float projectionsScaling=1f;
    

    public BvhProjection representative;
    public Cluster cluster;
    public Color projectionsColor;
    public Color representativeColor;
    public Vector3 representativePosition;
    public Vector3 projectionsPosition;

    private GLDraw gL;



    private Vector3 center = Vector3.zero;


    void Start()
    {
        gL = new GLDraw(mat);
    }

    private float radius;
    private float FiguresPerCircle;
    private void OnPostRender()
    {
        if (cluster == null || representative == null)
        {
            return;
        }

        radius = startingRadius;
        FiguresPerCircle = perCircleINIT;
        gL.drawFigure(true, representativeColor, representative.joints, null, representativePosition, representativeScaling);

        int i = 0;
        foreach (BvhProjection p in cluster.projections)
        {
            
            float angle = i * Mathf.PI * 2f / FiguresPerCircle;
            projectionsPosition = new Vector3(center.x + Mathf.Sin(angle) * radius, center.y + Mathf.Cos(angle) * radius, center.z);  // Update the pos for the next projection.
            gL.drawFigure(true, projectionsColor, p.joints, null, projectionsPosition, projectionsScaling);                           // Draw the projection.

            radius += radiusOffset;
            FiguresPerCircle += perCircleOFFSET;
            i++;
        }
    }


    



    void Update()
    {
        cluster = script.selectedCluster;
        if(cluster!=null)
            representative = cluster.Representative;
    }




}
