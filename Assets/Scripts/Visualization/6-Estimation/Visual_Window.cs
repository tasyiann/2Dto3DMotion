﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual_Window : MonoBehaviour
{

    public Material material;             // The material used in gl lines.
    public DataInFrame dataScript;        // Reference to the script which determines the selected pose to debug.
    public bool showSelectedOne;        
    public Vector3 offset;
    public Vector3 offsetToUpLeftCorner;
    public Vector3 previousSelectedWindowPosition; // <<
    public int maxLines = 5;
    public Vector3 CollumnOffset;
    public Color colorOfWindow;

    public OPPose figureToDebug;         // The chosen figure to debug, from showEstimationScript.
    public List<BvhProjection> selectedWin;

    private OPPose previousFigure = null; // Previous figure. Used to show the previous selectedN.
    private GLDraw gL;                    // GL lines.
    private Vector3 center;
    private Vector3 pos;
    private Vector3 columnStartingPoint;
    private int linesCounter = 0;
    private Vector3 pos_prev;

    //private int currentCollumn=1;

    void Start()
    {
        // These values should be saved, because I dont want to lost them form the inspector.
        offset = new Vector3(8.6f, 10.4f, 0f);
        offsetToUpLeftCorner = new Vector3(-75f, 34f, 0f);
        previousSelectedWindowPosition = new Vector3(-34f, 49.5f, 0f);
        maxLines = 10;
        CollumnOffset = new Vector3(82.87f, 0f, 0f);
        gL = new GLDraw(material);
        center = transform.position;
        columnStartingPoint = center + offsetToUpLeftCorner;                                                                                        // The top left point of the current collumn.
        pos = columnStartingPoint;
    }


    void Update()
    {
        figureToDebug = dataScript.selectedPoseToDebug;
        columnStartingPoint = center + offsetToUpLeftCorner;   // The top left point of the current collumn.
        pos = columnStartingPoint;                             // Go to the starting point of the (first) collumn.
        linesCounter = 0;                                      // Reset the line counter.
        pos_prev = previousSelectedWindowPosition;
    }

    private void OnPostRender()
    {

        if (previousFigure == null || previousFigure.selectedN == null || previousFigure.selectedN.windowIn3Dpoints == null)
        {
            // Set current figure as previous.
            previousFigure = figureToDebug;
            return;
        }
            

        // 1. Debug previous selected one window.
        drawWindowOfPrevFrame(previousFigure.selectedN.windowIn3Dpoints, Color.white);

        if (figureToDebug == null || figureToDebug.neighbours == null)
        {
            // Set current figure as previous.
            previousFigure = figureToDebug;
            return;
        }
        // 2. Debug the window of each of its neighbours.
        foreach (Neighbour n in figureToDebug.neighbours)
        {
            drawWindow(n.windowIn3Dpoints);
        }
        // Set current figure as previous.

        // Important! So, we can keep track the previousFigure.
        if (figureToDebug != previousFigure)
            previousFigure = figureToDebug;

    }

    private void drawWindow(List<BvhProjection> window)
    {

        int size = window.Count;
        int counter = 0;
        Color color;
        // One line of figures.
        foreach(BvhProjection figure in window)
        {


            // Color the middle one differently.
            if (counter == size / 2)
                color = Color.green;       
            else
                color = colorOfWindow;
            // Color the selected one differently.
            if (showSelectedOne && figureToDebug.selectedN.windowIn3Dpoints == window)
            {
                color = Color.white;
                selectedWin = window;
            }
                

            gL.drawFigure(true, color, figure.joints, null, pos);
            pos = new Vector3(pos.x + offset.x, pos.y, pos.z);             // Move some offset on the same line.
            counter++;
        }
        // Now, go to the next line.
        pos = new Vector3(columnStartingPoint.x, pos.y - offset.y, pos.z); // X should be at the start of column. Y should be set on the next line.

        linesCounter++;
        // Change to next collumn.
        if(linesCounter % maxLines == 0)
        {
            columnStartingPoint = columnStartingPoint + CollumnOffset;
            pos = new Vector3(columnStartingPoint.x, columnStartingPoint.y, columnStartingPoint.z);
        }
    }


    private void drawWindowOfPrevFrame(List<BvhProjection> window, Color colorBasic)
    {

        int size = window.Count;
        int counter = 0;
        Color color;
        // One line of figures.
        foreach (BvhProjection figure in window)
        {
            // Color the middle one differently.
            if (counter == size / 2)
            {
                color = Color.green;
            }
                
            else
                color = colorBasic;

            gL.drawFigure(true, color, figure.joints, null, pos_prev);
            pos_prev = new Vector3(pos_prev.x + offset.x, pos_prev.y, pos_prev.z);             // Move some offset on the same line.
            counter++;
        }
       
    }

}
