using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * In this visualisation we visualize the k-Neighbours of each frame.
 */
public class VMNeighbours {


    public int MaxFiguresToShow;        // Max Figures to be presented.
    public float offset = 8.0f;         // The space between neighbours.
    public bool showGrid;               // Show the grid if true.
    public int CurrentFrame;            // The frame to be shown.
    public Vector2 startingPos;         // The position to place the first neighbour.
    public Material Material;           // The material used in gl lines.
    public Text textInfo;               // Text (not in use yet.)
    private GLDraw gL;                  // GL lines.
    List<OPFrame> frames = Base.sc.frames;

    /**
     * Initialisation.
     */
    private void Start()
    {
        startingPos = new Vector2(-30, 40);
        gL = new GLDraw(Material);
    }

    /**
     * (Not in use). We could put some info on the screen.
     */
    private void updateText()
    {
        string s = "";
        if(frames != null)
        {
            s += CurrentFrame + "/" + frames.Count + "\n";
            foreach (var val in Enum.GetValues(typeof(EnumJoint)))
            {
                if (frames[CurrentFrame].figures.Count > 0)
                    s += val.ToString() + ": " + frames[CurrentFrame].figures[0].joints[(int)val] + "\n";
            }
            textInfo.text = s;
        }
    }

    /**
     * Render the figures on the camera.
     */
    private void OnPostRender()
    {
        if (showGrid)
        {
            // X-axis
            GL.Begin(GL.LINES);
            GL.Color(Color.white);
            // Y-axes
            GL.Vertex(new Vector3(startingPos.x, -1000, 0));
            GL.Vertex(new Vector3(startingPos.x, 1000, 0));
            GL.End();
        }


         float newposY = startingPos.y - offset;

        /* For each frame */
        foreach (OPFrame f in frames)
        {
            if (f.figures.Count > 0)
            {
                // Render the OP pose.
                newposY -= offset;
                gL.drawFigure(true,Color.yellow, f.figures[0].joints, f.figures[0].available, new Vector3(-30,newposY,0));

                if (showGrid)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(Color.white);
                    GL.Vertex(new Vector3(-50, newposY, 0));
                    GL.Vertex(new Vector3(1000, newposY, 0));
                    GL.End();
                }

                float newposX = startingPos.x + offset;
                // Render the neighbours.
                int neighCounter=0;
                foreach (Neighbour neighbour in f.figures[0].neighbours)
                {
                    if (neighCounter >= MaxFiguresToShow) break;
                    Vector3 pos = new Vector2(newposX, newposY);
                    gL.drawFigure(true,Color.green, neighbour.projection.joints, null , pos);

                    newposX += offset;
                    neighCounter++;
                }
            }
        }
    }


}
