using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class DisplayAxes : MonoBehaviour
{
    public float axesLength = 12f;
    public float axesWidthMultiplier = 1f;
    public Color fwdColor = Color.yellow;
    public Color upColor = Color.black;
    public Color Xcolor = Color.red;
    public Color Ycolor = Color.green;
    public Color Zcolor = Color.blue;

    private Transform t;
    private List<GameObject> axes;


    void Awake()
    {
        t = GetComponent<Transform>();
        axes = new List<GameObject>();
        displayAxes();
    }

    private void displayAxes()
    {
        axes.Add(setLine(t.position + (t.TransformDirection(Vector3.up) * axesLength), transform.position, Ycolor, false));
        axes.Add(setLine(t.position + (t.TransformDirection(Vector3.forward) * axesLength), transform.position, Zcolor, false));
        axes.Add(setLine(t.position + (t.TransformDirection(Vector3.right) * axesLength), transform.position, Xcolor, false));
    }


    private GameObject setLine(Vector3 _v1, Vector3 _v2, Color color, bool worldSpaceBool)
    {
        GameObject go = new GameObject(string.Format(@"{0}.txt", DateTime.Now.Ticks));
        go.transform.SetParent(transform);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("GUI/Text Shader"));
        lineRenderer.widthMultiplier = axesWidthMultiplier;
        lineRenderer.positionCount = 2;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.useWorldSpace = worldSpaceBool;
        lineRenderer.colorGradient = gradient;
        lineRenderer.SetPositions(new Vector3[] { _v1, _v2 });
        return go;
    }



    private Vector3 _rotAroundGlobal;
    private Vector3 _rotAroundLocal;
    public Vector3 RotAroundGlobal
    {
        get { return _rotAroundGlobal; }
        set
        {
            if(!t)
                t = GetComponent<Transform>();
            
            _rotAroundGlobal = value;

            t.rotation = Quaternion.Euler(value);
            
            _rotAroundLocal = t.localRotation.eulerAngles;
            
        }
    }

    [SerializeField]
    public Vector3 RotAroundLocal
    {
        get { return _rotAroundLocal; }
        set
        {
            if (!t)
                t = GetComponent<Transform>();
            
            _rotAroundLocal = value;
            t.localRotation = Quaternion.Euler(value);

            _rotAroundGlobal = t.rotation.eulerAngles;

        }
    }


    public void removeAxes()
    {
        foreach (GameObject g in axes)
        {
            DestroyImmediate(g);
        }
    }


}

[CustomEditor(typeof(DisplayAxes))]
public class RotationTesterEditor : Editor
{
    private  DisplayAxes tester => target as DisplayAxes;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Remove Axes"))
        {
            tester.removeAxes();
        }
    }
}

