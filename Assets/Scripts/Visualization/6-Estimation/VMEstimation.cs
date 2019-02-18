using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Winterdust;

/* OPFigure */
public class VMEstimation : MonoBehaviour
{
    // Data
    private static List<List<BvhProjection>> base_clusters = Base.base_clusters;
    private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;
    private static Scenario sc = Base.sc;


    // 1 Euro filter
    OneEuroFilter<Quaternion>[] rotationFiltersJoints = new OneEuroFilter<Quaternion>[14];
    OneEuroFilter<Quaternion> rotationFilterHips;
    public bool filterOn = true;
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;
    public float noiseAmount = 1.0f;
    float timer = 0.0f;


    // Set Visualistion
    public GameObject VideoplayerGO;
    private UnityEngine.Video.VideoPlayer videoPlayer;
    public Transform model;
    private Model3D m3d;

    // New - Visual just positions with IK
    OneEuroFilter<Vector3>[] positionFiltersJoints = new OneEuroFilter<Vector3>[14];
    OneEuroFilter<Vector3> positionFilterHips;
    public Transform modelPosition;
    private Model3D m3dPosition;



    public float Speed=5;
    public bool automatic;
    public enum smoothMovement
    {
        ROTATIONS, LERP, ONE_EURO, POSITIONS, NONE
    }
    public smoothMovement AnimationSmoothness;
    public bool showGrid;
    public int ChooseProjection = 0;
    public Material greenMaterial;
    public Material whiteMaterial;
    public Material yellowMaterial;
    public Material redMaterial;
    public Text Best3DAlgText;
    public Text AlgText;
    public Button buttonAutomatic;

    Neighbour[] estimation = DataParsing.estimation;
    List<OPFrame> frames;
  
    public Text textInfo;

    private int currentFrame;
    private int framesLength;
    private GLDraw gL;

    private void Start()
    {
        buttonAutomatic.onClick.AddListener(setAutomaticVisualisation);
        frames = sc.frames;
        gL = new GLDraw(whiteMaterial); // Set the Material
        m3d = new Model3D(model);       // Set the 3D Model
        setTitles();                    // Set the titles of algorithms.
        setVideoPlayer();               // Set the videoplayer.
        framesLength = DataParsing.estimation.Length;


        for(int i=0; i<rotationFiltersJoints.Length; i++)
        {
            rotationFiltersJoints[i] = new OneEuroFilter<Quaternion>(filterFrequency);
        }
        for (int i = 0; i < positionFiltersJoints.Length; i++)
        {
            positionFiltersJoints[i] = new OneEuroFilter<Vector3>(filterFrequency);
        }
        rotationFilterHips = new OneEuroFilter<Quaternion>(filterFrequency);
        positionFilterHips = new OneEuroFilter<Vector3>(filterFrequency);

        m3dPosition = new Model3D(modelPosition);       // Set the 3D Model_POSITIONS

    }


    private void setAutomaticVisualisation()
    {
        if (automatic == false)
            automatic = true;
        else
            automatic = false;
    }

    private void setVideoPlayer()
    {
        string url = sc.inputDir;
        videoPlayer = VideoplayerGO.GetComponent(typeof(UnityEngine.Video.VideoPlayer)) as UnityEngine.Video.VideoPlayer;
        if (videoPlayer == null)
        {
            Debug.Log("VideoPlayer is null");
        }

        if (url == null || url == "") Debug.Log("url is null");
        videoPlayer.url = url + "\\video.mp4";
        videoPlayer.Pause();
    }

    private void setTitles()
    {
        /* Set the title of algorithms */
        Best3DAlgText.text = sc.algEstimation.ToString();
        AlgText.text = sc.algNeighbours.ToString();
    }

    private void updateText()
    {
        string s = "";

        {
            
            if (estimation==null)
            {
                s += "* * Pose estimation not found in this frame. * *";
                textInfo.text = s;
                return;
            }
           
            Neighbour chosen = estimation[ChooseProjection];
            if (chosen == null)
                return;

            s += ChooseProjection + "/" + estimation.Length + "\n";
            s += "ClusterFile: " + chosen.projection.clusterID + "\n";
            s += "Angle: " + chosen.projection.angle + "\n";
            s += "2D-Distance: " + chosen.distance2D + "\n";

            textInfo.text = s;
        }

    }





    private void OnPostRender()
    {
        /* Show video on Current frame. */
        //videoPlayer.frame = ChooseProjection;
        updateText();

        /* Make the axes. */
        if (showGrid) { gL.drawAxes(Color.white); }

        /* Draw results. */
        if (estimation!=null && estimation[ChooseProjection] != null)
        {
            gL.drawFigure(true,Color.green, estimation[ChooseProjection].projection.joints,null,Vector3.zero);
            
        }

        /* Draw original input figure. */
        if (frames[ChooseProjection].figures.Count > 0)
            gL.drawFigure(true,Color.yellow, frames[ChooseProjection].figures[0].joints, frames[ChooseProjection].figures[0].available, Vector3.zero);

    }


    float Timer = 0.0f;
    // Update is called once per frame
    void Update()
    {
        // If we want the animation to run by itself
        //frame += Time.deltaTime * speed;
        Timer += Time.deltaTime;

        if (automatic)
        {
            ChooseProjection = (int)(Timer * Speed);
            if (ChooseProjection > framesLength - 1)
            {
                ChooseProjection = 0;
                Timer = 0;
            }
                
        }




        Vector3 pos = transform.position;
        if (Input.GetKeyDown("w"))
        {
            ChooseProjection ++;
            if (ChooseProjection >= framesLength)
                ChooseProjection = 0;
        }
        if (Input.GetKeyDown("s"))
        {
            ChooseProjection --;
            if (ChooseProjection < 0)
                ChooseProjection = framesLength - 1;
        }







        /* Do the 3d Animation. */
        if (estimation != null && estimation[ChooseProjection] != null)
        {
            switch (AnimationSmoothness)
            {
                //case smoothMovement.ROTATIONS: { m3d.moveWithRot(estimation[ChooseProjection]); break; }
                case smoothMovement.LERP: { m3d.moveSkeletonLERP(estimation[ChooseProjection].projection.joints); break; }
                case smoothMovement.NONE: { m3d.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
                case smoothMovement.ONE_EURO:
                    {
                        m3d.moveSkeleton_OneEuroFilter(estimation[ChooseProjection].projection.joints, rotationFiltersJoints, rotationFilterHips);
                        break;
                    }
                default: { m3d.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
            }

            // Anyways...
            updateParametersRotationFilters();
            m3dPosition.moveSkeleton_IK_POSITIONS(estimation[ChooseProjection].projection.joints, positionFiltersJoints, rotationFilterHips);
        }


        /* Show video on Current frame. */
        videoPlayer.frame = ChooseProjection;


    }

    private void updateParametersRotationFilters()
    {
        rotationFilterHips.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        foreach (OneEuroFilter<Quaternion> rotfilter in rotationFiltersJoints)
        {
            rotfilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        }
    }

}
