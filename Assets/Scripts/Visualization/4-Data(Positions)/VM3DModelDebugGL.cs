using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VM3DModelDebugGL : MonoBehaviour
{

    public GameObject model;
    public bool showGrid;
    public Material Material;
    VM3DModelDebug modeldebug;
    private GLDraw gL;

    private void Start()
    {
        gL = new GLDraw(Material);
        modeldebug = model.GetComponent(typeof(VM3DModelDebug)) as VM3DModelDebug;
    }

    private void OnPostRender()
    {
        if (showGrid)
            gL.drawAxes(Color.white);
        gL.drawFigure(true,Color.white, modeldebug.getJointsToShow(), null, Vector3.zero);
    }
}
