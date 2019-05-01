using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAxes : MonoBehaviour
{
    public Vector3 v1;
    public Vector3 v2;
    public Color line = Color.yellow;
    public Color X = Color.red;
    public Color Y = Color.green;
    public Color Z = Color.blue;


    // Start is called before the first frame update
    void Start()
    {
        setLine(new Vector3(200f, 100f, 50f), transform.position, line);
        displayAxes();
        transform.rotation = XLookRotation(v1 - v2, Vector3.up);
    }

    private void displayAxes()
    {
        setLine(Vector3.up*100, transform.position, Y);
        setLine(Vector3.forward * 100, transform.position, Z);
        setLine(Vector3.right * 100, transform.position, X);
    }

    public static Quaternion XLookRotation(Vector3 right, Vector3 up)
    {

        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }

    public static Quaternion YLookRotation(Vector3 up, Vector3 upwards)
    {

        Quaternion UpToForward = Quaternion.Euler(90f, 0f, 0f); // Correct, it's 90 degrees to make the positive y, into positive forward.
        Quaternion forwardToTarget = Quaternion.LookRotation(up, upwards);

        return forwardToTarget * UpToForward;
    }

    private void setLine(Vector3 _v1, Vector3 _v2, Color color)
    {
        GameObject go = new GameObject(v1.ToString());
        go.transform.SetParent(transform);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = 2;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
        lineRenderer.SetPositions(new Vector3[] { _v1, _v2 });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
