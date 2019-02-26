using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The purpose of this visual is to show a simple
// grid of the projections of a single frame.

public class V_Projections : MonoBehaviour
{
    public VMProjections script;            // Script to get the data.
    public BvhProjection mainFigure;        // Main figure.
    public List<BvhProjection> projections; // Projections of the main figure.
    public Material material;               // The material used in gl lines.
    public Vector3 offsetToUpLeftCorner;    // Offset from the center.
    public Vector3 offsetBetweenFigures;    // Distance between figures.
    public int maxFiguresPerLine;           // In order to change line.
    public Color color;

    private Vector3 upLeftCorner;           // Position in up left corner.
    private GLDraw gL;                      // GL Lines.
    private Vector3 center;                 // The center point of this visual. (camera's center)
    private Vector3 pos;                    // Position of the projection to be drawn.
    

    void Start()
    {
        gL = new GLDraw(material);
        center = transform.position;
        upLeftCorner = center + offsetToUpLeftCorner;
        pos = upLeftCorner;
        mainFigure = script.mainFigure;
        projections = script.projections;
        color = Color.white;
        maxFiguresPerLine = 12;
    }



    private void OnPostRender()
    {
        if (projections == null)
            return;

        drawSnippet();

    }


    private void drawSnippet()
    {
        int counter = 0;
        foreach(BvhProjection p in projections)
        {
            gL.drawFigure(true, color, p.joints, null, pos);                // Draw the projection.
            pos = new Vector3(pos.x+offsetBetweenFigures.x, pos.y, pos.z);  // Update the pos for the next projection.
            counter++;
            if (counter % maxFiguresPerLine == 0)                           // Change line.
                pos = new Vector3(upLeftCorner.x, pos.y+offsetBetweenFigures.y, pos.z);
        }

    }



    void Update()
    {
        upLeftCorner = center + offsetToUpLeftCorner;
        pos = upLeftCorner;
        mainFigure = script.mainFigure;
        projections = script.projections;
    }
}
