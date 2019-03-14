using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Winterdust;
public class V_ShowSampleSequenceOfMotion : MonoBehaviour
{
    public bool play = false;
    public string fileName;
    public int figuresNum = 20;
    public int currentFrame;
    public Vector3 offsetBetween = new Vector3(10f,0f,0f);
    public int frameRate = 17;
    public Vector3 startingPoint;
    public Color color;
    //public bool animate = true;
    public float jointSize = 0.6f;


    private bool initiated = false;
    private int offsetFrame;
    private List<BvhFigure> figures = new List<BvhFigure>();

    private class BvhFigure
    {
        public BVH bvh;
        public GameObject obj;
        public int frame;
        public Color color;
        public float jointSize;

        public BvhFigure(BVH b, GameObject o, Color c, float js)
        {
            bvh = b;
            obj = o;
            color = c;
            jointSize = js;
        }

        public void setFrame(int newFrame)
        {
            bvh.moveSkeleton(obj, newFrame);
            frame = newFrame;
        }

        public Vector3 getRootPosition()
        {
            return bvh.allBones[0].localFramePositions[frame];
        }

        public void setColor(Color c, float js)
        {
            obj.GetComponent<BVHDebugLines>().color = c;
          
            Renderer[] renderers = obj.transform.GetComponentsInChildren<Renderer>();//FindObjectsOfType<Renderer>();
            foreach (Renderer r in renderers)
            {
                Debug.Log(r.name);
                r.material.color = c;
            }

            //traverseHierarchy(obj.transform, c);
            color = c;
            jointSize = js;

        }


        private void traverseHierarchy(Transform root, Color c)
        {
            foreach(Transform child in obj.transform)
            {
                if (root == child)
                    continue;

                Debug.Log(child.name);
                Renderer r = child.GetComponent<Renderer>();
                if (r != null)
                    r.material.color = c;
                traverseHierarchy(child, c);
            }
        }
    }


    void Start()
    {

    }

    private void initiate()
    {
        initiateFigures();
        repositionFigures();
        offsetFrame = 0;
        initiated = true;
    }



    private void initiateFigures()
    {
        removeFigures(ref figures);
        if (fileName.EndsWith(".bvh"))
        {
            BVH bvh = new BVH(fileName);
            for (int i = 0; i < figuresNum; i++)
            {
                GameObject go = bvh.makeDebugSkeleton(false, "000000", jointSize);
                figures.Add(new BvhFigure(bvh, go,color,jointSize));
            }
        }
    }

    private void removeFigures(ref List<BvhFigure> figures)
    {
        foreach (BvhFigure f in figures)
        {
            Destroy(f.obj);
        }
        figures = new List<BvhFigure>();
    }


    private void Update()
    {

        if (!play)
            return;

        if (!initiated)
            initiate();

        offsetFrame = 0;
        updateFrames();
        repositionFigures();
        updateFigures();
        updateColorSize();
    }

    private void updateFigures()
    {
        if (figuresNum != figures.Count)
            initiateFigures();
    }

    private void updateColorSize()
    {
        if (figures == null || figures.Count == 0)
            return;

        if (color !=figures[0].color || jointSize!=figures[0].jointSize)
            foreach (BvhFigure f in figures)
            {
                f.setColor(color,jointSize);
            }
    }

    private void repositionFigures()
    {
        Vector3 position = startingPoint;
        foreach (BvhFigure figure in figures)
        {
            figure.obj.transform.position = position;
            position += offsetBetween;
        }
    }


    private void updateFrames()
    {
        foreach (BvhFigure figure in figures)
        {
            figure.setFrame(currentFrame + offsetFrame);
            offsetFrame += frameRate;

        }
    }

}
