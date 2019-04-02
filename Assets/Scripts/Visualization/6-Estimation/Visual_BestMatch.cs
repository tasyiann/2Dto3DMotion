using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual_BestMatch : MonoBehaviour
{

    public bool showGrid = true;                // Whether we want to view axes.
    public Material material;                   // Material is used to draw lines with gl.
    public DataInFrame showEstimationScript;

    private OPPose figureToDebug;               // The chosen figure to debug, from showEstimationScript.
    private GLDraw gL;
    private Vector3 center;

    // Start is called before the first frame update
    void Start()
    {
        gL = new GLDraw(material);          // Set the Material.
        center = transform.position;
    }


    void Update()
    {
        figureToDebug = showEstimationScript.selectedPoseToDebug;
    }


    private void OnPostRender()
    {
        /* Make the axes. */
        if (showGrid) { gL.drawAxes(Color.white, center); } // TODO: Make axers relative to the camera.

        /* Draw results. */
        if (figureToDebug!=null && figureToDebug.Estimation3D!=null && figureToDebug.Estimation3D.projection!=null && figureToDebug.Estimation3D.projection.joints!=null)
        {
            if(figureToDebug.Estimation3D.projection.joints.Length!=0) // Check, because in some cases the length is zero...
                gL.drawFigure(true, Color.green, figureToDebug.Estimation3D.projection.joints, null, center);
            if (figureToDebug.joints.Length != 0)                   // Check, because in some cases the length is zero...
                gL.drawFigure(true, Color.yellow, figureToDebug.joints, figureToDebug.available, center);
        }
    }


}
