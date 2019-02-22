using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Apply on camera object.
public class Visual_KBest2DFigures : MonoBehaviour
{

    public Material material;                        // The material used in gl lines.
    public DataInFrame showEstimationScript;        // Reference to the script which determines the selected pose to debug.

    private OPPose figureToDebug;                    // The chosen figure to debug, from showEstimationScript.
    private GLDraw gL;                               // GL lines.
    private const int MaxFiguresPerLine = 5;         // Figures per line.
    private float offsetToCorner = 20f;              // We use this, so we can get to the up left corner of camera.
    private float offset = 10f;                      // Space between figures.
    private float posX, posY;                        // Location of a figure.

    void Start()
    {
        gL = new GLDraw(material);
        resetPos();
    }

    void Update()
    {
        figureToDebug = showEstimationScript.selectedPoseToDebug;
    }

    private void OnPostRender()
    {
        if(figureToDebug!=null)
        {
            // Render 1st figure's best k 2D match
            int neighCounter = 0;
            foreach (Neighbour neighbour in figureToDebug.neighbours)
            {
            
                if (neighCounter >= MaxFiguresPerLine)
                {
                    posY -= offset;
                    neighCounter = 0;
                    posX = transform.position.x - offsetToCorner;
                }
                    
                // Here, we could put another color to the projections that had been selected as the leading one.
                // We could check if neighbour.projection is the same as neighbour.selectedN.projection
                Vector3 pos = new Vector2(posX, posY);
                Color color = Color.green;
                gL.drawFigure(true, color, neighbour.projection.joints, null, pos);
              
                posX += offset;
                neighCounter++;
            }
            resetPos();
        }

    }

    private void resetPos()
    {
        posX = transform.position.x - offsetToCorner;
        posY = transform.position.y + offsetToCorner - 5f;
    }

}
