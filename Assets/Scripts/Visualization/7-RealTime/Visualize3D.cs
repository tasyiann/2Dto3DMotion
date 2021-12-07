//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using OpenPose.Example;

//public class Visualize3D : MonoBehaviour
//{

//    public Material material;
//    private GLDraw gL;
//    public Transform mycamera;
//    private Vector3 center;
//    Vector3[] estimation_to_debug = new Vector3[14];
//    OPPose figureToDebug = null;


//    void Start()
//    {
//        gL = new GLDraw(material);
//        center = mycamera.position;
//    }


//    void Update()
//    {
//        figureToDebug = OpenPoseUserScript.selectedPoseToDebug;
//        estimation_to_debug = OpenPoseUserScript.estimation_to_debug;
//    }

//    void OnPostRender()
//    {
//        // Display figures as sticks
//        if (figureToDebug != null)
//        {
//            gL.drawFigure(true, Color.white, figureToDebug.joints, figureToDebug.available, center);
//        }

//        if (estimation_to_debug != null)
//        {
//            gL.drawFigure(true, Color.green, estimation_to_debug, null, center);

//        }
//    }
//}
