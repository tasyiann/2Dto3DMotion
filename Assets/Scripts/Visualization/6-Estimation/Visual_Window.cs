using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//
//
//      TODO: The order of 3D's is off. Needs to be fixed due to new update on the order.
//            GL lines are ok.
//
//
//
public class Visual_Window : MonoBehaviour
{
    public bool show3D=false;
    public GameObject figurePrefab;
    public GameObject figurePrevPrefab;

    private List<Model3DObject> figures = new List<Model3DObject>();
    private List<Model3DObject> prevFigures = new List<Model3DObject>();

    public Material material;             // The material used in gl lines.
    public ControlFrames_viaVideo dataScript;       // Reference to the script which determines the selected pose to debug.
    public bool showSelectedOne;
    public bool showWindowGl;
    public bool showPrev;

    public Vector3 offset;
    public Vector3 offsetToUpLeftCorner;
    public Vector3 previousSelectedWindowPosition; // <<
    public int maxLines = 3;
    public Vector3 CollumnOffset;

    // Colors
    public Color colorOfWindow;
    public Color colorOfPrevWindow;
    public Color colorOfCorresponding3D;
    public Color colorOfkNearProjection;
    public Color colorOfSelected;

    public bool showkNearProjection = false;

    public OPPose figureToDebug;         // The chosen figure to debug, from showEstimationScript.
    public List<BvhProjection> selectedWin;


    private OPPose previousFigure = null; // Previous figure. Used to show the previous selectedN.
    private GLDraw gL;                    // GL lines.
    private Vector3 center;
    private Vector3 pos;
    private Vector3 columnStartingPoint;
    private int linesCounter = 0;
    private Vector3 pos_prev;

    private int windowSize = Base.m;
    private int k = Base.k;
    private int figuresNum;

    //private int currentCollumn=1;


    private GameObject figuresGameObject;

    void Start()
    {
        // How many figures should we initiate in the scene:
        figuresNum = windowSize * k;

        // These values should be saved, because I dont want to lost them form the inspector.
        gL = new GLDraw(material);
        center = transform.position;
        columnStartingPoint = center + offsetToUpLeftCorner;    // The top left point of the current collumn.
        pos = columnStartingPoint;

        // Initiate 3D figures
        initiateFigures();
        initiateWindowOfPrevFrame();
        
    }


    private void OnPostRender()
    {

        List<OPFrame> frames = dataScript.frames;
        int currentIndex = dataScript.currentFrame;
        int figureIndex = dataScript.personIndex;

        if (showPrev)
        {
            pos_prev = center + previousSelectedWindowPosition;
            for (int i = windowSize; i >= 1; i--)
            {
                int k = currentIndex - i;
                if (k < 0 || frames[k] == null || frames[k].figures == null || frames[k].figures[figureIndex] == null || frames[k].figures[figureIndex].Estimation3D == null || frames[k].figures[figureIndex].Estimation3D.projection == null || frames[k].figures[figureIndex].Estimation3D.projection.joints == null || frames[k].figures[figureIndex].Estimation3D.projection.joints.Length == 0)
                {
                    continue;
                }
                Vector3[] joints = frames[k].figures[figureIndex].Estimation3D.projection.joints;
                gL.drawFigure(true, colorOfPrevWindow, joints, null, pos_prev);
                pos_prev += new Vector3(offset.x, 0f, 0f);
            }
            if (dataScript.selectedPoseToDebug != null && dataScript.selectedPoseToDebug.Estimation3D != null && dataScript.selectedPoseToDebug.Estimation3D.projection != null && dataScript.selectedPoseToDebug.Estimation3D.projection.joints != null)
                gL.drawFigure(true, colorOfkNearProjection, dataScript.selectedPoseToDebug.Estimation3D.projection.joints, null, pos_prev);

        }





        // 2. Debug the window of each of its neighbours.
        foreach (Neighbour n in figureToDebug.neighbours)
        {
            drawWindow(n.windowIn3Dpoints);
            // Then, draw the 2D
            if (showkNearProjection)
                gL.drawFigure(true, colorOfkNearProjection, n.projection.joints, null, pos);


            // <<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>
            // Now, go to the next line.
            pos = new Vector3(columnStartingPoint.x, pos.y - offset.y, pos.z); // X should be at the start of column. Y should be set on the next line.

            linesCounter++;

            // Change to next collumn.
            if (linesCounter % maxLines == 0)
            {
                columnStartingPoint = columnStartingPoint + CollumnOffset;
                pos = new Vector3(columnStartingPoint.x, columnStartingPoint.y, columnStartingPoint.z);
            }
            // <<<<<<<<<<<<<<<<<>>>>>>>>>>>>>
        }



        // Set current figure as previous.
    }

    private void drawWindow(List<BvhProjection> window)
    {

        int size = window.Count;
        int counter = 0;
        Color color;
        // One line of figures.
        for (int i = windowSize - 1; i >= 0; i--)
        {
            if (i >= window.Count || window[i] == null)  // Skip this position, if there isn't any figure in this pos of window.
            {
                pos = new Vector3(pos.x + offset.x, pos.y, pos.z);
                continue;
            }


            // Color the middle one differently.

            if (showSelectedOne && figureToDebug.Estimation3D.windowIn3Dpoints == window)
            {
                color = colorOfSelected;
                selectedWin = window;
            }
            else
                color = colorOfWindow;


            if (showWindowGl)
                gL.drawFigure(true, color, window[i].joints, null, pos);

            pos = new Vector3(pos.x + offset.x, pos.y, pos.z);             // Move some offset on the same line.
            counter++;
        }


    }









    // TODO :)


































    private void initiateFigures()
    {
        figures = new List<Model3DObject>();
        figuresGameObject = new GameObject("Figures");
        linesCounter = 1;

        // fix position
        pos = new Vector3(pos.x, pos.y-2f, pos.z);

        for (int i = 0; i < figuresNum; i++)
        {
            // Change Line.
            //Debug.Log(" i:" + i + "%" + windowSize + " = " + i % (windowSize + 1));
            if (i != 0 && i % (windowSize) == 0)
            {
                //Debug.Log("ChangeLine");
                pos = new Vector3(columnStartingPoint.x, pos.y - offset.y, pos.z);
                linesCounter++;
            }


            // Change Collumn.
            if (linesCounter % (maxLines + 1) == 0)
            {
                //Debug.Log("ChangeCollumn");
                linesCounter = 1;
                columnStartingPoint = columnStartingPoint + CollumnOffset;
                pos = new Vector3(columnStartingPoint.x, columnStartingPoint.y-2f, columnStartingPoint.z);
            }

            // Place 3D figure
            GameObject go = Instantiate(figurePrefab);
            go.name = "figure" + i;
            go.transform.position = pos;
            go.transform.parent = figuresGameObject.transform;
            Model3DObject model = new Model3DObject(go);
            model.setVisible(true);
            figures.Add(model);

            pos = new Vector3(pos.x + offset.x, pos.y, pos.z);
        }

    }


    void Update()
    {
        figureToDebug = dataScript.selectedPoseToDebug;
        previousFigure = dataScript.previousPose;
        columnStartingPoint = center + offsetToUpLeftCorner;   // The top left point of the current collumn.
        pos = columnStartingPoint;                             // Go to the starting point of the (first) collumn.
        linesCounter = 0;                                      // Reset the line counter.
        pos_prev = previousSelectedWindowPosition;

        if (show3D)
        {
            update3DModels();
            updatePrevFigures();
        }
        
    }










    private void update3DModels()
    {
        int index = 0;

        if (figureToDebug == null || figureToDebug.neighbours == null)
        {
            return;
        }
            

        foreach (Neighbour n in figureToDebug.neighbours)
        {
            // Whole window does not exist.
            if (n.windowIn3Dpoints == null || n.windowIn3Dpoints.Count==0)
            {
               // Debug.Log("Window does not exist!");
                // skip whole window
                for (int i = windowSize-1; i>=0; i--)
                {
                    figures[index + i].setVisible(false);
                }
                index += windowSize;
                continue;
            }


            //foreach (BvhProjection f in n.windowIn3Dpoints)
            for (int i = windowSize-1; i>=0; i--)
            {
                if (i >= n.windowIn3Dpoints.Count || n.windowIn3Dpoints[i] == null)  // Skip this position, if there isn't any figure in this pos of window.
                {
                    figures[index].setVisible(false);
                    index++;
                    continue;
                }
                BvhProjection f = n.windowIn3Dpoints[i];

                // Just a figure does not exist.
                if (f == null || f.joints==null || f.joints.Length==0)
                {
                    figures[index].setVisible(false);
                    index++;
                    continue;
                }
                figures[index].setVisible(true);
                figures[index].setJoints(f.joints);
                index++;
            }
        }

    }


    private void updatePrevFigures()
    {
        List<OPFrame> frames = dataScript.frames;
        int currentIndex = dataScript.currentFrame;
        int figureIndex = dataScript.personIndex;

        for (int i=windowSize; i>0; i--)
        {
            int k = currentIndex - i;
            if ( k < 0 || frames[k]==null || frames[k].figures==null || frames[k].figures[figureIndex]==null || frames[k].figures[figureIndex].Estimation3D==null || frames[k].figures[figureIndex].Estimation3D.projection==null || frames[k].figures[figureIndex].Estimation3D.projection.joints==null || frames[k].figures[figureIndex].Estimation3D.projection.joints.Length==0)
            {
                continue;
            }
            Vector3[] joints = frames[k].figures[figureIndex].Estimation3D.projection.joints;
            //Debug.Log("i-1:" +(i-1) + " prevFigures length:"+prevFigures.Count);
            prevFigures[windowSize-i].setJoints(joints);
        }
    }


   


    private void destroyFigures()
    {
        Destroy(figuresGameObject);
        figures = null;
    }


    private void initiateWindowOfPrevFrame()
    {
        pos_prev = center + previousSelectedWindowPosition;

        // One line of figures.
        for (int i=0; i<windowSize; i++)
        {

            // Place 3D figure
            GameObject go = Instantiate(figurePrevPrefab);
            go.name = "prevfigure" + i;
            go.transform.position = pos_prev;
            go.transform.parent = figuresGameObject.transform;
            Model3DObject model = new Model3DObject(go);
            model.setVisible(true);
            prevFigures.Add(model);
            pos_prev = new Vector3(pos_prev.x + offset.x, pos_prev.y, pos_prev.z);             // Move some offset on the same line.
   
        }
       
    }

}
