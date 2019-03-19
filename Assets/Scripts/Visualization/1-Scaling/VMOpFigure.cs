using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**
 * In this Visualisation, we try to show how successfully our algorithm
 * does the scaling on a figure. The figure is made out of 2d points.
 * Each 2d point represents a joint in a 2d World. The input of the program
 * is from the output of OpenPose (JSON files).
*/
public class VMOpFigure : MonoBehaviour
{


    private static Scenario sc = Base.sc;

    public GameObject VideoplayerGO;
    private UnityEngine.Video.VideoPlayer videoPlayer;

    public bool ShowUsedScaling;
    public bool ShowLimbsScaling;
    public bool ShowHeightScaling;
    public bool ShowRawFigure;

    public float Offset = 8.0f;         // The distance between figures.
    public bool showGrid;               // True for showing a grid.
    public int CurrentFrame = 0;        // Current frame to show.

    [Range(1f, 0.01f)]
    public float JSONscale = 0.025f;            // Scaling the raw input.
    private float scalingFactor_ACTUAL_USED;    // That was used in the algorithm.
    private float scalingFactor_LIMBS;         
    private float scalingFactor_HEIGHT;
    private EnumBONES usedLIMB;

    public Material Material;           // The material of GL visuals.
    public Text textInfo;               // The text to show the current frame.
    private GLDraw gL;                  // GL visuals.
    private List<OPFrame> frames;       // The frames of 2d motion. 
    private OPPose figure;

    public Text Raw_text;               
    public Text Scaled_text;            
    public Text Limbs_text;             
    public Text Height_text;            

    /**
     * Initialisation. 
     */
    void Start()
    {
        frames = sc.frames;
        gL = new GLDraw(Material);
        setVideoPlayer();
    }


    /**
     * GL lines are applied on the camera. We use them to represent our figures.
     */
    private void OnPostRender()
    {
        //updateText();

        float newpos = 0;

        if (ShowUsedScaling)
        {
            // NORMALIZED DATA
            scalingFactor_ACTUAL_USED = figure.scaleFactor;
            gL.drawFigure(true, Color.green, figure.joints, figure.available, new Vector3(newpos, 0, 0));
            if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
                drawBoundings(Color.green, figure.joints[(int)EnumJoint.Head], figure.joints[(int)EnumJoint.RightFoot], figure.joints[(int)EnumJoint.LeftFoot], 1000);
            newpos += 10;
        }


        if (ShowLimbsScaling)
        {
            // SCALE_LIMBS
            Scaling.getGlobalScaleFactor_USING_LIMBS(figure.jointsRAW, out usedLIMB, out scalingFactor_LIMBS);
            gL.drawFigure(true, Color.black, figure.jointsRAW, figure.available, new Vector3(newpos, 0, 0), scalingFactor_LIMBS);
            if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
                drawBoundings(Color.black, figure.jointsRAW[(int)EnumJoint.Head], figure.jointsRAW[(int)EnumJoint.RightFoot], figure.jointsRAW[(int)EnumJoint.LeftFoot], 1000, scalingFactor_LIMBS);
            newpos += 10;
        }

        // JSON POSITION
        if (ShowRawFigure)
        {
            gL.drawFigure(true, Color.red, figure.jointsRAW, figure.available, new Vector3(newpos, 0, 0), JSONscale);
            if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
                drawBoundings(Color.red, figure.jointsRAW[(int)EnumJoint.Head], figure.jointsRAW[(int)EnumJoint.RightFoot], figure.jointsRAW[(int)EnumJoint.LeftFoot], 1000, JSONscale);
            newpos += 10;
        }

        if (ShowHeightScaling)
        {
            // SCALE_HEIGHT
            scalingFactor_HEIGHT = Scaling.getGlobalScaleFactor_USING_HEIGHT(figure.jointsRAW);
            gL.drawFigure(true, Color.yellow, figure.jointsRAW, figure.available, new Vector3(newpos, 0, 0), scalingFactor_HEIGHT);
            if (figure.available[(int)EnumJoint.Head] && (figure.available[(int)EnumJoint.RightFoot] || figure.available[(int)EnumJoint.LeftFoot]))
                drawBoundings(Color.yellow, figure.jointsRAW[(int)EnumJoint.Head], figure.jointsRAW[(int)EnumJoint.RightFoot], figure.jointsRAW[(int)EnumJoint.LeftFoot], 1000, scalingFactor_HEIGHT);
            //newpos += 10;
        }


            updateScalingFactors();
        
    }

    private void drawBoundings(Color color, Vector3 head, Vector3 rightFoot, Vector3 leftFoot, float length, float scaling=1f)
    {
        gL.drawHorizontalLine(color, (head*scaling).y, length);
        gL.drawHorizontalLine(color, minNum((rightFoot*scaling).y, (leftFoot * scaling).y), length);
    }

    private float minNum(float a, float b)
    {
       return a < b ? a : b;
    }

    /**
* Updates the text that shows the current frame of the visual. */
    private void updateInfo()
    {
        string s = "";
        if (frames != null)
        {
            s += CurrentFrame + "/" + frames.Count + "\n";
            if (frames[CurrentFrame].figures.Count > 0)
            {
                foreach (var val in Enum.GetValues(typeof(EnumJoint)))
                {
                    s += val.ToString() + ": " + frames[CurrentFrame].figures[0].joints[(int)val] + "\n";
                }
            }
            else
            {
                s += "Figure not found in this frame.";
            }
            textInfo.text = s;
        }
    }


    private void updateScalingFactors()
    {
        Raw_text.text = "Raw Figure scaled by " + JSONscale +" just to fit in frame.";
        Scaled_text.text = "with interpolation : Figure actually scaled by " + scalingFactor_ACTUAL_USED + " using "+ Base.ScaleMethod.ToString();
        Limbs_text.text = "w/out interpolation using LIMBS: " + scalingFactor_LIMBS + " using limb: "+usedLIMB.ToString();
        Height_text.text = "w/out interpolation using HEIGHT: "+ scalingFactor_HEIGHT+ " with fixed height = "+Scaling.scalingHeightFixed;
    }

    /**
     * Move between frames.
     */
    void Update()
    {
        if (Input.GetKey("w"))
        {
            CurrentFrame++;
            if (CurrentFrame >= frames.Count)
                CurrentFrame = 0;
        }
        if (Input.GetKey("s"))
        {
            CurrentFrame--;
            if (CurrentFrame < 0)
                CurrentFrame = frames.Count - 1;
        }
        /* Show video on Current frame. */
        videoPlayer.frame = CurrentFrame; // <<<<<

        if (frames[CurrentFrame].figures.Count > 0)
        {
            figure = frames[CurrentFrame].figures[0];
        }
    }


    /**
 * Updates the text that shows the current frame of the visual. */
    private void updateTextDistancesJoints()
    {
        string s = "";
        if (frames != null)
        {
            s += CurrentFrame + "/" + frames.Count + "\n";
            if (frames[CurrentFrame].figures.Count > 0)
            {
                foreach (var val in Enum.GetValues(typeof(EnumJoint)))
                {
                    s += val.ToString() + ": " + frames[CurrentFrame].figures[0].joints[(int)val] + "\n";
                }
            }
            else
            {
                s += "Figure not found in this frame.";
            }
            textInfo.text = s;
        }
    }

    /**
     * Install the videoPlayer on the scene.
     */
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


}
