using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Winterdust;

/* OPFigure */
public class VMEstimation : MonoBehaviour
{
    private static List<List<BvhProjection>> base_clusters = Base.base_clusters;
    private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;
    private static Scenario sc = Base.sc;

    public GameObject VideoplayerGO;
    private UnityEngine.Video.VideoPlayer videoPlayer;
    public Transform model;
    private Model3D m3d;
    private float frame;

    public float Speed=5;
    public bool automatic;
    public enum smoothMovement
    {
        ROTATIONS, LERP, NONE
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


    Neighbour[] estimation = DataParsing.estimation;
    List<OPFrame> frames = sc.frames;
  
    public Text textInfo;
    bool rendered = false;

    private int currentFrame;

    private GLDraw gL;

    private void Start()
    {
        gL = new GLDraw(whiteMaterial); // Set the Material
        m3d = new Model3D(model);       // Set the 3D Model
        setTitles();                    // Set the titles of algorithms.
        setVideoPlayer();               // Set the videoplayer.
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

            s += ChooseProjection + "/" + estimation.Length + "\n";
            s += "ClusterFile: " + chosen.projection.clusterID + "\n";
            s += "Angle: " + chosen.projection.angle + "\n";
            s += "Distance: " + chosen.distance + "\n";

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
            if (ChooseProjection > DataParsing.estimation.Length - 1)
            {
                ChooseProjection = 0;
                Timer = 0;
            }
                
        }




        Vector3 pos = transform.position;
        if (Input.GetKey("w"))
        {
            ChooseProjection ++;
            if (ChooseProjection >= DataParsing.estimation.Length)
                ChooseProjection = 0;
        }
        if (Input.GetKey("s"))
        {
            ChooseProjection --;
            if (ChooseProjection < 0)
                ChooseProjection = DataParsing.estimation.Length - 1;
        }







        /* Do the 3d Animation. */
        if (estimation != null && estimation[ChooseProjection] != null)
        {
            switch (AnimationSmoothness)
            {
                //case smoothMovement.ROTATIONS: { m3d.moveWithRot(estimation[ChooseProjection]); break; }
                case smoothMovement.LERP: { m3d.moveSkeletonLERP(estimation[ChooseProjection].projection.joints); break; }
                case smoothMovement.NONE: { m3d.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
                default: { m3d.moveSkeleton(estimation[ChooseProjection].projection.joints); break; }
            }
        }


        /* Show video on Current frame. */
        videoPlayer.frame = ChooseProjection;


        /*
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.z -= scroll * 20 * 100f * Time.deltaTime;
        transform.position = pos;
        */
    }



}
