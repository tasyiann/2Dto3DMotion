using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Winterdust;

/* OPFigure */
public class VMEstimation : DataInFrame
{
    
    private static Scenario sc = Base.sc;   // Data. Please revise this.
    public GameObject VideoplayerGO;        // Set video player.
    public bool automatic;                  // Automatic play of video.
    public bool showInfo;                   // Show text info.
    public Button buttonAutomatic;          // Click this button to play or pause video.
    public Text textInfo;                   // Text info component.
    public int currentFrame;                // Current frame of video.


    private UnityEngine.Video.VideoPlayer videoPlayer;          // Set Video player.
    private List<OPFrame> frames;                               // Frames of the video.
    private int framesLength;                                   // Amount of frames in video.               
    private GLDraw gL;                                          // GL to draw lines.
   


    private void Start()
    {
        buttonAutomatic.onClick.AddListener(setAutomaticVisualisation);
        frames = sc.frames;
        
        setVideoPlayer();               // Set the videoplayer.
        framesLength = DataParsing.estimation.Length;

        
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

        currentFrame = (int)videoPlayer.frame; // <<
    }



    private void updateText()
    {
        string s = "";

        {
            
            if (selectedPoseToDebug==null)
            {
                s += "* * Pose estimation not found in this frame. * *";
                textInfo.text = s;
                return;
            }
           
            Neighbour chosen = selectedPoseToDebug.selectedN;
            if (chosen == null)
                return;

            s += currentFrame + @"\" + framesLength + "\n";
            s += "Cluster: " + chosen.projection.clusterID + "\n";
            // s += "Angle: " + chosen.projection.angle + "\n";
            s += "2D-Distance: " + chosen.distance2D + "\n";
            s += "3D-Rot-Distance: " + chosen.distance3D + "\n";

            textInfo.text = s;
        }

    }





    private void selectFigureToDebug()
    {

        if (currentFrame < 0 || currentFrame >= frames.Count)
            selectedPoseToDebug = null;
        else
        {
            // Check that currentFrame, personIndex are valid.
            if (frames[currentFrame].figures != null && frames[currentFrame].figures.Count > 0 && personIndex >= 0 && personIndex < frames[currentFrame].figures.Count && frames[currentFrame].figures[personIndex].joints.Length != 0)
            {
                selectedPoseToDebug = frames[currentFrame].figures[personIndex];
            }
            else
                selectedPoseToDebug = null;
        }
        
    }

    // Update is called once per frame
    void Update()
    {


        if (automatic)
        {
            if (videoPlayer.isPaused)
                videoPlayer.Play();

            currentFrame = (int)videoPlayer.frame;
            
            if (currentFrame > framesLength - 1 || currentFrame < 0)
            {
                currentFrame = 0;
            }

        }else
        {
            Vector3 pos = transform.position;
            if (Input.GetKeyDown("w"))
            {
                currentFrame++;
                if (currentFrame >= framesLength)
                    currentFrame = 0;
            }
            if (Input.GetKeyDown("s"))
            {
                currentFrame--;
                if (currentFrame < 0)
                    currentFrame = framesLength - 1;
            }

            /* Show video on Current frame. */
            if (!videoPlayer.isPaused)
                videoPlayer.Pause();
            videoPlayer.frame = currentFrame;
        }

        // Export Data.
        selectFigureToDebug(); // <<
        if (frames[currentFrame] != null)
        {
            allPoses = frames[currentFrame].figures;
        }

        if (showInfo)
        {
            updateText();
        }
        else
            textInfo.text = "";


    }



}
