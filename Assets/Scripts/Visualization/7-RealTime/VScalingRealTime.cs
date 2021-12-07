//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using OpenPose.Example;

//public class VScalingRealTime : MonoBehaviour
//{


//    // Input is the figure

//    [Range(1f, 0.01f)]
//    public float JSONscale = 0.025f;            // Scaling the raw input.
//    [Range(-100f, 100f)]
//    public float offset = 10f;
//    public Text[] scalingFactorText;

//    OPPose figureToDebug = null;
//    OPPose prevFigure = null;
//    public Material Material;           // The material of GL visuals.
//    private GLDraw gL;                  // GL visuals.
//    public Transform camera;
//    private int sfText_index = 0;

//    private Vector3 center;
//    private int newpos;
//    private OPPose figure;
//    private Vector3 pos;


//    // Use this for initialization
//    void Start()
//    {
//        gL = new GLDraw(Material);
//        center = camera.transform.position;
//        newpos = (int)center.x;
//    }

//    void Update()
//    {
//        figure = OpenPoseUserScript.selectedPoseToDebug;
//        displayScalingFactor();
//        prevFigure = figure;
//    }

//    private void displayScalingFactor()
//    {
//        sfText_index = sfText_index % scalingFactorText.Length; // Cyclic
//        scalingFactorText[sfText_index].color = Color.green;
//        scalingFactorText[sfText_index].text = figure != null ? (figure.limbFactor.ToString()) : "";
//        //float diff = figure == null || prevFigure == null ? 0 : (figure.scaleFactor - prevFigure.scaleFactor);
//        if (figure != null && figure.limbFactor != EnumBONES.UP_TORSO) scalingFactorText[sfText_index].color = Color.red;
//        sfText_index++; // move to the next text
//    }

//    private void OnPostRender()
//    {
//        if (figure == null)
//        {
//            return;
//        }
//        pos = new Vector3(center.x, 0f, 0f);
//        // NORMALIZED DATA
//        gL.drawFigure(true, Color.yellow, figure.joints, figure.available, pos);
//        if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
//        {
//            drawBoundings(Color.yellow, figure.joints[(int)EnumJoint.Head], figure.joints[(int)EnumJoint.RightFoot], figure.joints[(int)EnumJoint.LeftFoot], 1000, pos);
//        }

//        // RAW DATA
//        pos = new Vector3(center.x + offset, 0f, 0f);
//        gL.drawFigure(true, Color.red, figure.jointsRAW, figure.available, pos, JSONscale);
//        if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
//        {
//            drawBoundings(Color.red, figure.jointsRAW[(int)EnumJoint.Head], figure.jointsRAW[(int)EnumJoint.RightFoot], figure.jointsRAW[(int)EnumJoint.LeftFoot], 1000, pos, JSONscale);
//        }
//    }

//    private void drawBoundings(Color color, Vector3 head, Vector3 rightFoot, Vector3 leftFoot, float length, Vector3 translation, float scaling = 1f)
//    {
//        gL.drawHorizontalLine(color, (head * scaling).y + translation.y, length, translation.x);
//        gL.drawHorizontalLine(color, minNum((rightFoot * scaling).y + translation.y, (leftFoot * scaling).y + translation.y), length, translation.x);
//    }

//    private float minNum(float a, float b)
//    {
//        return a < b ? a : b;
//    }


//}
