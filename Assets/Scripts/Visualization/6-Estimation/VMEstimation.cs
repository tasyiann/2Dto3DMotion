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


    // 1 Euro filter - JOINTS
    OneEuroFilter<Quaternion>[] rotationFiltersJoints = new OneEuroFilter<Quaternion>[14];
    public bool updateParams = false;
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;
    public float noiseAmount = 1.0f;

    // 1 Euro filter - HIPS
    OneEuroFilter<Quaternion> rotationFilterHips;
    public float filterFrequency_hips = 120.0f;
    public float filterMinCutoff_hips = 1.0f;
    public float filterBeta_hips = 0.0f;
    public float filterDcutoff_hips = 1.0f;
    public float noiseAmount_hips = 1.0f;

    // Set Video
    public GameObject VideoplayerGO;
    private UnityEngine.Video.VideoPlayer videoPlayer;
    // Set 3d model w/ filters
    public Transform modelFiltered;
    private Model3D m3dFiltered;
    // Set 3d model w/out filters
    public Transform modelRaw;
    private Model3D m3dRaw;


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
        m3dFiltered = new Model3D(modelFiltered);       // Set the 3D Model
        m3dRaw = new Model3D(modelRaw);
        setVideoPlayer();               // Set the videoplayer.
        framesLength = DataParsing.estimation.Length;


        for(int i=0; i<rotationFiltersJoints.Length; i++)
        {
            rotationFiltersJoints[i] = new OneEuroFilter<Quaternion>(filterFrequency);
        }
        rotationFilterHips = new OneEuroFilter<Quaternion>(filterFrequency);

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

        ChooseProjection = (int)videoPlayer.frame; // <<
        //videoPlayer.Pause();
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
            s += "Cluster: " + chosen.projection.clusterID + "\n";
            // s += "Angle: " + chosen.projection.angle + "\n";
            s += "2D-Distance: " + chosen.distance2D + "\n";
            s += "3D-Rot-Distance: " + chosen.distance3D + "\n";

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
            if (videoPlayer.isPaused)
                videoPlayer.Play();
            ChooseProjection = (int)videoPlayer.frame;
            if (ChooseProjection > framesLength - 1 || ChooseProjection < 0)
            {
                ChooseProjection = 0;
                Timer = 0;
            }

        }else
        {
            Vector3 pos = transform.position;
            if (Input.GetKeyDown("w"))
            {
                ChooseProjection++;
                if (ChooseProjection >= framesLength)
                    ChooseProjection = 0;
            }
            if (Input.GetKeyDown("s"))
            {
                ChooseProjection--;
                if (ChooseProjection < 0)
                    ChooseProjection = framesLength - 1;
            }
            /* Show video on Current frame. */
            if (!videoPlayer.isPaused)
                videoPlayer.Pause();
            videoPlayer.frame = ChooseProjection;
        }












        /* Do the 3d Animation. */
        if (estimation != null && estimation[ChooseProjection] != null)
        {
            // Set filtered 3d model
            switch (AnimationSmoothness)
            {
                case smoothMovement.LERP: { m3dFiltered.moveSkeletonLERP(estimation[ChooseProjection].projection.joints); break; }
                case smoothMovement.NONE: { m3dFiltered.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
                case smoothMovement.ONE_EURO:
                    {
                        if (updateParams)
                        {
                            updateParametersRotationFilters();
                        }
                        m3dFiltered.moveSkeleton_OneEuroFilter(estimation[ChooseProjection].projection.joints, rotationFiltersJoints, rotationFilterHips);
                        break;
                    }
                default: { m3dFiltered.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
            }


            // Set Raw 3d model
            m3dRaw.moveSkeleton(estimation[ChooseProjection].projection.joints);
        }





    }

    private void updateParametersRotationFilters()
    {
        rotationFilterHips.UpdateParams(filterFrequency_hips, filterMinCutoff_hips, filterBeta_hips, filterDcutoff_hips);
        foreach (OneEuroFilter<Quaternion> rotfilter in rotationFiltersJoints)
        {
            rotfilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        }
    }

}
