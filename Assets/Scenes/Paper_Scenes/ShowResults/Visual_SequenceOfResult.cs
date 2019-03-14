using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual_SequenceOfResult : MonoBehaviour
{


    public Material material;               // The material used in gl lines.
    public VMEstimation script;             // Reference to the script which determines the selected pose to debug.
    public const int MaxFiguresPerLine = 5;
    public Vector3 offsetToCorner = new Vector3(20f, 15f, 0);            // We use this, so we can get to the up left corner of camera.
    public Vector3 offsetBetweenFigures = new Vector3(4f, 0f, 0);      // Space between figures.
    public Vector3 newlineOffset = new Vector3(0f, 4f, 0f);
    public GameObject figurePrefab;


    private Color[] colors = { Color.blue, Color.red, Color.yellow, Color.green, Color.cyan };
    private Color color;
    private int colorIndex = 0;
    private GLDraw gL;                               // GL lines.
    private List<OPFrame> frames;
    private Vector3 pos = Vector3.zero;

    private List<Model3DObject> figures = new List<Model3DObject>();




    void Start()
    {
        gL = new GLDraw(material);
        color = colors[0];
        frames = script.frames;
        displaySequence();
    }

    bool executed = false;

    void Update()
    {
        if (executed)
            return;

        if (frames == null || frames.Count == 0)
        {
            frames = script.frames;
            displaySequence();
            executed=true;
        }
    }

    private void displaySequence()
    {
        int counter = 0;
        for(int i=0; i<frames.Count; i++)
        {
            

            OPFrame frame = frames[i];
            if (frame == null || frame.figures.Count == 0)
                continue;

            OPPose pose = frame.figures[0];
            if (pose.Estimation3D == null || pose.Estimation3D.projection.joints.Length == 0)
                continue;

            if (i > 0 && pose.Estimation3D.projection != frames[i - 1].figures[0].Estimation3D.projection)
                changeColor();

            Model3DObject m = instantiateFigure(pos, pose.Estimation3D.projection.joints);
            figures.Add(m);
            figurePrefab = m.gameObject;
            pos += offsetBetweenFigures;
            counter++;
            if (counter % 36 == 0)
                pos = new Vector3(0f, pos.y - newlineOffset.y, 0f);
        }
    }


    private Model3DObject instantiateFigure(Vector3 position, Vector3[] joints)
    {
        GameObject go = Instantiate(figurePrefab);
        go.transform.position = position;
        Model3DObject m = new Model3DObject(go, joints);
        // m.setColor(color);
        return m;
    }


    private void changeColor()
    {
        colorIndex = (colorIndex + 1) % colors.Length;
        color = colors[colorIndex];
    }
}
