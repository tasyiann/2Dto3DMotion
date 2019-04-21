using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;

// https://forums.cgsociety.org/t/converting-rotation-values-for-bvh/1166576/12
public class ProjectMotion : MonoBehaviour
{
    public string @path;
    public Vector3 offset = new Vector3(10f,10f,0f);
    public Material mat;
    public Color color = Color.white;
    public int maxPerLine = 30;
    public int currentFrame;

    private BVH bvh;
    private GameObject skeleton;
    private GLDraw gl;
    private List<BvhProjection> motion;
    private Vector3 speed = new Vector3(0f, 10f, 0f);

    void Start()
    {
        // Initialize
        bvh = new BVH(path);
        gl = new GLDraw(mat);
        motion = InitializeSequence(bvh);
    }

    List<Camera> cams;

    private void OnPostRender()
    {


        
        drawSequence(motion);
    }

    private void Update()
    {
        if (Input.GetKey("s"))
        {
            transform.position -= speed;
        }
        if (Input.GetKey("w"))
        {
            transform.position += speed;
        }
    }

    private void drawSequence(List<BvhProjection> motion)
    {
        Vector3 position = Vector3.zero;
        int counter = 0;
        foreach (BvhProjection frame in motion )
        {
            counter++;
            gl.drawFigure(true, color, frame.joints, null, position);
            if (counter % maxPerLine == 0)
                position = new Vector3(0f, position.y + offset.y, 0f);
            else
                position += new Vector3(offset.x, 0f, 0f);
        }
    }

    private List<BvhProjection> InitializeSequence(BVH bvh)
    {
        List<BvhProjection> list = new List<BvhProjection>();
        int[] order = ProjectionManager.getOrderOfJoints(bvh);
        float scaleFactor = ProjectionManager.getScaleFactorBVH(bvh, order);
        bvh.scale(scaleFactor);
        for (int i=0; i<bvh.frameCount; i++)
        {

            Matrix4x4 matrix;
            Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];
            foreach (var val in Enum.GetValues(typeof(EnumJoint)))
            {
                int index = (int)val;
                matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, i);
                joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            }

            BvhProjection newProjection = new BvhProjection(0, i, 0, joints);
            newProjection.convertPositionsToRoot();
            list.Add(newProjection);
        }
        return list;
    }

}
 