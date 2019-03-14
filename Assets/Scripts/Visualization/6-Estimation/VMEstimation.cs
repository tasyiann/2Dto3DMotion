using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
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
                                            // Current frame of video.
    [HideInInspector]
    public List<OPFrame> frames;            // Frames of the video.


    private UnityEngine.Video.VideoPlayer videoPlayer;          // Set Video player.

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

        string[] videoFiles = Directory.GetFiles(url,"*.mp4");
        if (videoFiles == null || videoFiles.Length==0)
        {
            videoFiles = Directory.GetFiles(url, "*.avi");
            if (videoFiles == null || videoFiles.Length == 0)
                return;
        }
        Debug.Log("Video file name: "+videoFiles[0]);
        videoPlayer.url = videoFiles[0];
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
           
            Neighbour chosen = selectedPoseToDebug.Estimation3D;
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
                if (currentFrame - 1 >= 0 && frames[currentFrame - 1].figures.Count > 0 && personIndex >= 0 && personIndex < frames[currentFrame - 1].figures.Count && frames[currentFrame - 1].figures[personIndex].joints.Length != 0)
                    previousPose = frames[currentFrame - 1].figures[personIndex];
            }
            else
                selectedPoseToDebug = null;
        }
        
    }




    void Update()
    {
        //// <>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        //// -- Initialise recorder --
        //if (!RecordingDistance && startRecordingDistance)
        //{
        //    // Initialize [Set video frame]
        //    videoPlayer.Pause();
        //    videoPlayer.frame = 0;
        //    videoPlayer.Play();
        //    recordingDistScript.initializeJoints();
        //    Debug.Log("Wake up animation.");
        //    recordingDistScript.wakeUpAnimation();
        //    RecordingDistance = true;
        //}

        //// -- Record distance --
        //if (RecordingDistance && startRecordingDistance)
        //{
        //    currentFrame = (int)videoPlayer.frame;
        //    until_now_distance3D = recordingDistScript.calculateDist();
        //}

        //if (startRecordingDistance == false)
        //{
        //    RecordingDistance = false;
        //}
        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>






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
