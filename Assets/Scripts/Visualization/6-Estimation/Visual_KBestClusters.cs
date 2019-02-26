using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Apply on camera object.
public class Visual_KBestClusters : MonoBehaviour
{

    public Material material;                       // The material used in gl lines.
    public DataInFrame showEstimationScript;        // Reference to the script which determines the selected pose to debug.
    public bool showBestOne;                        // Different color on the best one
    public Color color;

    private OPPose figureToDebug;                    // The chosen figure to debug, from showEstimationScript.
    private GLDraw gL;                               // GL lines.
    private const int MaxFiguresPerLine = 5;         // Figures per line.
    public Vector3 offsetToCorner = new Vector3(20f, 15f, 0);                     // We use this, so we can get to the up left corner of camera.
    public Vector3 offsetBetweenFigures = new Vector3(10f, 10f, 0);               // Space between figures.
    private float posX, posY;                        // Location of a figure

    void Start()
    {
        gL = new GLDraw(material);
        resetPos();
        color = new Color(1.18f, 0.58f, 0f); // orange
    }

    void Update()
    {
        figureToDebug = showEstimationScript.selectedPoseToDebug;
    }

    private void OnPostRender()
    {
        if(figureToDebug!=null && figureToDebug.neighbours!=null && figureToDebug.selectedClusters!=null)
        {
            // Render 1st figure's best k 2D match
            int neighCounter = 0;
            foreach (Cluster cluster in figureToDebug.selectedClusters)
            {
            
                if (neighCounter >= MaxFiguresPerLine)
                {
                    posY -= offsetBetweenFigures.y;
                    neighCounter = 0;
                    posX = transform.position.x - offsetToCorner.x;
                }

                // Here, we could put another color to the projections that had been selected as the leading one.
                // We could check if neighbour.projection is the same as figureToDebug.selectedN.projection
                Vector3 pos = new Vector2(posX, posY);
                
                
                gL.drawFigure(true, color, cluster.representative.joints, null, pos);
              
                posX += offsetBetweenFigures.x;
                neighCounter++;
            }
            resetPos();
        }

    }

    private void resetPos()
    {
        posX = transform.position.x - offsetToCorner.x;
        posY = transform.position.y + offsetToCorner.y;
    }

}
