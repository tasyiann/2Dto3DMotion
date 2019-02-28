using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenPose.Example;

public class Visual_Scaling : MonoBehaviour
{

    [Range(1f, 0.01f)]
    public float JSONscale = 0.025f;            // Scaling for the raw input.
    public DataInFrame showEstimationScript;    // Reference to the script which determines the selected pose to debug.
    public Material Material;                   // The material of GL visuals.
    public int maxPrevFigures = 7;              // Max figs to show.
    public int PrevFiguresRate=0;               // Skipping frames.

    // Offsets : Please set in Inspector.
    public Vector3 offsetNormlizedFromCenter;
    public Vector3 offsetStartingPointOfPrevFiguresFromNormalized;
    public Vector3 offsetBetweenPrevFigures;
    public Vector3 offsetBetweenRawNormalized;

    // Colors
    public Color prevColor = Color.gray;
    public Color rawColor = Color.red;
    public Color normColor = Color.yellow;
    

    public bool debugRAW = false;
    public bool ShowRaw = true;
    public bool ShowNorm = true;
    public bool ShowDirectionOfPrev = false;
    public bool ShowBoundings = true;

    // Positions of figures
    private Vector3 startingPointOfPrevFigures;
    private Vector3 posNormalizedCurrentFigure;
    private Vector3 posRawCurrentFigure;
    private Vector3 posOfPrevFigure;

    // Data.
    private int currentFrame;
    private int prevFrame;
    private Queue<OPPose> prevFigures;
    private GLDraw gL;                          // GL visuals.
    private Vector3 center;                     // The center of this visual.              
    private OPPose figureToDebug;               // The figure to debug. From showEstimationScript.
         


    void Start()
    {
        gL = new GLDraw(Material);
        center = transform.position;
        prevFigures = new Queue<OPPose>();
        startingPointOfPrevFigures = center;
        posNormalizedCurrentFigure = center;
        posRawCurrentFigure = posNormalizedCurrentFigure + offsetBetweenRawNormalized;
    }

    void Update()
    {
        figureToDebug = showEstimationScript.selectedPoseToDebug;
        currentFrame = showEstimationScript.currentFrame;

        if(prevFrame != currentFrame)
        {
            if(PrevFiguresRate<=1)
                enQueueFigure(figureToDebug);
           else if( currentFrame%PrevFiguresRate==0 )
                enQueueFigure(figureToDebug);
        }
            


        startingPointOfPrevFigures = center + offsetStartingPointOfPrevFiguresFromNormalized;
        posNormalizedCurrentFigure = center + offsetNormlizedFromCenter;
        posRawCurrentFigure = posNormalizedCurrentFigure + offsetBetweenRawNormalized;


        prevFrame = currentFrame;
    }


    private void rawStyle()
    {

    }


    private void enQueueFigure(OPPose figure)
    {
        if (figure == null || figure.joints == null || figure.joints.Length==0)
            return;

        if (prevFigures.Count >= maxPrevFigures)
            prevFigures.Dequeue();

        prevFigures.Enqueue(figure);
    }


    private void OnPostRender()
    {
        if (figureToDebug == null || figureToDebug.joints==null || figureToDebug.joints.Length==0) 
        {
            return;
        }

        posOfPrevFigure = startingPointOfPrevFigures;

        renderPrevFigures(debugRAW);
        if(ShowNorm)
            renderCurrentNormalizedFigure();
        if(ShowRaw)
            renderCurrentRawFigure();

    }

    private void renderCurrentNormalizedFigure()
    {
        // NORMALIZED DATA
        gL.drawFigure(true, normColor, figureToDebug.joints, figureToDebug.available, posNormalizedCurrentFigure);
        if (ShowBoundings && figureToDebug.available[(int)EnumJoint.Head] && (figureToDebug.available[(int)EnumJoint.RightFoot] || figureToDebug.available[(int)EnumJoint.LeftFoot]))
        {
            drawBoundings(normColor, figureToDebug.joints[(int)EnumJoint.Head], figureToDebug.joints[(int)EnumJoint.RightFoot], figureToDebug.joints[(int)EnumJoint.LeftFoot], 1000, posNormalizedCurrentFigure);
        }
    }


    private void renderCurrentRawFigure()
    {
        gL.drawFigure(true, rawColor, figureToDebug.jointsRAW, figureToDebug.available, posRawCurrentFigure, JSONscale);
        if (ShowBoundings && figureToDebug.available[(int)EnumJoint.Head] && (figureToDebug.available[(int)EnumJoint.RightFoot] || figureToDebug.available[(int)EnumJoint.LeftFoot]))
        {
            drawBoundings(rawColor, figureToDebug.jointsRAW[(int)EnumJoint.Head], figureToDebug.jointsRAW[(int)EnumJoint.RightFoot], figureToDebug.jointsRAW[(int)EnumJoint.LeftFoot], 1000, posRawCurrentFigure, JSONscale);
        }
    }


    private void renderPrevFigures(bool raw)
    {
        foreach (OPPose figure in prevFigures)
        {
            Vector3[] joints;
            float scale;
            if (raw)
            {
                joints = figure.jointsRAW;
                scale = JSONscale;
            }
                
            else
            {
                joints = figure.joints;
                scale = 1f;
            }
                

            gL.drawFigure(ShowDirectionOfPrev, prevColor, joints, figure.available, posOfPrevFigure,scale);
            posOfPrevFigure = new Vector3(posOfPrevFigure.x + offsetBetweenPrevFigures.x, posOfPrevFigure.y, posOfPrevFigure.z);
        }
    }



    private void drawBoundings(Color color, Vector3 head, Vector3 rightFoot, Vector3 leftFoot, float length, Vector3 translation, float scaling = 1f)
    {
        gL.drawHorizontalLine(color, (head * scaling).y + translation.y, length, translation.x);
        gL.drawHorizontalLine(color, minNum((rightFoot * scaling).y + translation.y, (leftFoot * scaling).y + translation.y), length, translation.x);
    }

    private float minNum(float a, float b)
    {
        return a < b ? a : b;
    }


}
