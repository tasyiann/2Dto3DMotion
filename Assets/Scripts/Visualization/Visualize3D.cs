using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPose.Example;

public class Visualize3D : MonoBehaviour {
    //public Transform model;
    //private Model3D m3d;
    // Plot 2D
    public Material material;
    private GLDraw gL;
    public  Transform mycamera;
    private Vector3 center;
    Vector3[] estimation_to_debug = new Vector3[14];
    OPPose figureToDebug = null;
    Vector3[] rawInputToDebug = new Vector3[14];

    // Use this for initialization
    void Start () {
        //m3d = new Model3D(model);       // Set the 3D Model
        gL = new GLDraw(material);
        center = mycamera.position;
    }


    void Update()
    {
        figureToDebug = OpenPoseUserScript.figureToDebug;
        estimation_to_debug =  OpenPoseUserScript.estimation_to_debug;
        rawInputToDebug = OpenPoseUserScript.rawInputToDebug;
        /*
        
        */

    }

    void OnPostRender()
    {
        if (figureToDebug != null)
        {
            gL.drawFigure(true, Color.white, figureToDebug.joints, figureToDebug.available, center);
            /*
            string s = "";
            foreach (Vector3 v in figureToDebug.joints)
            {
                s += v.x + " " + v.y + " " + v.z + "\n";
            }
            Debug.Log("Figure as processed: \n" + figureToDebug.jointsToString(false));
            */
        }

        if (estimation_to_debug != null)
        {
            gL.drawFigure(true, Color.green, estimation_to_debug, null, center);
            
        }
        /*
        if (rawInputToDebug != null)
        {
            gL.drawFigure(true, Color.yellow, rawInputToDebug, figureToDebug.available, center);
            
            string ss = "";
            foreach (Vector3 v in rawInputToDebug)
            {
                ss += v.x + " " + v.y + " " + v.z + "\n";
            }
            Debug.Log("Figure unprocessed as raw from static variable: \n" + ss +"\n\nFrom figure:\n"+figureToDebug.jointsToString(true));
           
        }
        */
    }
}
