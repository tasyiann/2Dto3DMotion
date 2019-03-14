using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual_ChosenProjection : MonoBehaviour
{
    public Vector3 offset;
    public DataInFrame script;
    public Material m;
    public Color color;

    private Vector3 pos;
    public Vector3 start;

    private List<OPPose> figures;
    private GLDraw gL;

    // Start is called before the first frame update
    void Start()
    {
        gL = new GLDraw(m);
    }

    // Update is called once per frame
    void Update()
    {
        figures = script.allPoses;
        
    }


    private void OnPostRender()
    {
        if (figures == null)
            return;

        pos = start;

        foreach (OPPose figure in figures)
        {
            if (figure.Estimation3D == null || figure.Estimation3D.projection == null || figure.Estimation3D.projection.joints.Length == 0)
                return;

            gL.drawFigure(true, color, figure.Estimation3D.projection.joints, null, pos);
            pos += offset;
        }
    }
}
