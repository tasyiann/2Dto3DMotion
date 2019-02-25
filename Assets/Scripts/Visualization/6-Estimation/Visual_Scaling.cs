using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenPose.Example;

public class Visual_Scaling : MonoBehaviour
{
    [Range(1f, 0.01f)]
    public float JSONscale = 0.025f;            // Scaling for the raw input.
    [Range(-100f, 100f)]
    public float offset = 10f;                  // Distance between the 2 figures.
    public DataInFrame showEstimationScript;    // Reference to the script which determines the selected pose to debug.
    public Material Material;                   // The material of GL visuals.
 
    private GLDraw gL;                          // GL visuals.
    private Vector3 center;                     // The center of this visual.              
    private float posX;                         // The horizontal position.
    private OPPose figureToDebug;               // The figure to debug. From showEstimationScript.
    private Vector3 pos;                        // Where to place the figure.


    void Start()
    {
        gL = new GLDraw(Material);
        center = transform.position;
        posX = center.x;
    }

    void Update()
    {
        figureToDebug = showEstimationScript.selectedPoseToDebug;
    }


    private void OnPostRender()
    {
        if (figureToDebug == null || figureToDebug.joints==null || figureToDebug.joints.Length==0) 
        {
            return;
        }
        pos = center;
        // NORMALIZED DATA
        gL.drawFigure(true, Color.yellow, figureToDebug.joints, figureToDebug.available, pos);
        if (figureToDebug.available[(int)EnumJoint.Head] && (figureToDebug.available[(int)EnumJoint.RightFoot] || figureToDebug.available[(int)EnumJoint.LeftFoot]))
        {
            drawBoundings(Color.yellow, figureToDebug.joints[(int)EnumJoint.Head], figureToDebug.joints[(int)EnumJoint.RightFoot], figureToDebug.joints[(int)EnumJoint.LeftFoot], 1000, pos);
        }

        // RAW DATA
        pos = new Vector3(center.x + offset, center.y, center.z);
        gL.drawFigure(true, Color.red, figureToDebug.jointsRAW, figureToDebug.available, pos, JSONscale);
        if (figureToDebug.available[(int)EnumJoint.Head] && (figureToDebug.available[(int)EnumJoint.RightFoot] || figureToDebug.available[(int)EnumJoint.LeftFoot]))
        {
            drawBoundings(Color.red, figureToDebug.jointsRAW[(int)EnumJoint.Head], figureToDebug.jointsRAW[(int)EnumJoint.RightFoot], figureToDebug.jointsRAW[(int)EnumJoint.LeftFoot], 1000, pos, JSONscale);
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
