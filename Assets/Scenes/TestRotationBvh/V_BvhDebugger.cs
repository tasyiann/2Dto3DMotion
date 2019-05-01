using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tomis.UnityEditor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Winterdust;

// https://forum.unity.com/threads/help-finishing-bvh-export-script-unity-animation-blender.127914/
// https://sites.google.com/a/cgspeed.com/cgspeed/bvhplay/bvhplay-faq
#if UNITY_EDITOR

[CustomEditor(typeof(V_BvhDebugger))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        V_BvhDebugger myScript = (V_BvhDebugger)target;
        if (GUILayout.Button("Export new BVH"))
        {
            Base.InitializeRotations(myScript.rotationsUrl + "/");
            myScript.Estimation = (Neighbour[])OfflineDataProcessing.readBinaryfile(myScript.estimationUrl);
            myScript.ExportBvh();
            Debug.Log("New bvh has been exported!");
        }
    }
}

#endif
public class V_BvhDebugger : MonoBehaviour
{
    private BvhExport BvhExporter;
    public bool DisplayCorrectRoot;
    public Text textInfo;

    [Header("Paths to Files")]
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to bvh", FileExtensions = "bvh", OpenAtPath = @"..\Scenarios")]
    public string bvhUrl;
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to Estimation file", OpenAtPath = @"..\Scenarios")]
    public string estimationUrl;
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to Video", OpenAtPath = @"..\Scenarios")]
    public string videoUrl;
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to bvh Template")]
    public string bvhTemplate;
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to Export Dir", SelectMode = FileSelectionMode.Folder)]
    public string exportDir;
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to Rotations", SelectMode = FileSelectionMode.Folder)]
    public string rotationsUrl;
    private string lastBvhFilePath = "";

    [Header("Cameras")]
    public Camera MotionSequenceCam;
    public Camera BVHDisplayCam;
    public Camera EstimationCam;

    [Header("Motion Sequence Properties")]
    public Vector3 offset = new Vector3(10f, 10f, 0f);
    public Material mat;
    public Color colorDefault;
    public int maxPerLine = 30;
    private BVH bvh;
    private GameObject skeleton;
    private GLDraw gl;
    private List<BvhProjection> motion;
    private Vector3 speed = new Vector3(0f, 10f, 0f);

    [Header("Video Properties")]
    public GameObject VideoplayerGO;      
    public bool automatic = true;                  
    private UnityEngine.Video.VideoPlayer videoPlayer;
    private int framesLength;


    private int _currentFrame;
    public int CurrentFrame { get { return _currentFrame; } set { _currentFrame = value; bvh.moveSkeleton(skeleton, CurrentFrame); updateInfo(); } }

    public Neighbour[] Estimation { get => estimation; set => estimation = value; }
    private Neighbour[] estimation;


    private void Awake()
    {
        MotionSequenceCam.gameObject.AddComponent<OnCameraPostRenderEventRaiser>()
            .OnPostRenderEvent += EventRaiserForCam_Motion_OnPostRenderEvent;
        EstimationCam.gameObject.AddComponent<OnCameraPostRenderEventRaiser>()
            .OnPostRenderEvent += EventRaiserForCam_Estimation_OnPostRenderEvent;
        BVHDisplayCam.gameObject.AddComponent<OnCameraPostRenderEventRaiser>()
            .OnPostRenderEvent += EventRaiserForCam_BVH_OnPostRenderEvent;
    }

    void Start()
    {
        // Load estimation
        Base.InitializeRotations(rotationsUrl + "/");
        estimation = (Neighbour[])OfflineDataProcessing.readBinaryfile(estimationUrl);

        // Export new bvh
        ExportBvh();

        // Motion Sequence
        bvh = new BVH(bvhUrl);
        gl = new GLDraw(mat);
        motion = InitializeSequence(bvh);

        // Debug BVH
        skeleton = bvh.makeDebugSkeleton(false, "ffffff", 0.2f);

        // Video
        setVideoPlayer();
        framesLength = (int)bvh.frameCount-1;
    }

    private void Update()
    {
        updateCamera_Motion_position();
        updateFrame();
    }


    private void EventRaiserForCam_BVH_OnPostRenderEvent(object sender, CameraEventArgs e)
    {
        List<List<Vector3>> trianglesSource, trianglesTarget;
        if(BvhExporter!=null && (trianglesSource = BvhExporter.trianglesSource).Count!=0  && (trianglesTarget = BvhExporter.trianglesTarget).Count != 0)
        {
            GL.Begin(GL.LINE_STRIP);
            mat.SetPass(0);
            // Source
            GL.Color(Color.yellow);
            GL.Vertex(trianglesSource[CurrentFrame][0]);
            GL.Color(Color.red);
            GL.Vertex(trianglesSource[CurrentFrame][1]);
            GL.Color(Color.yellow);
            GL.Vertex(trianglesSource[CurrentFrame][2]);
            GL.Color(Color.green);
            GL.Vertex(trianglesSource[CurrentFrame][0]);
            GL.End();

            // Target
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.yellow);
            GL.Vertex(trianglesTarget[CurrentFrame][0]);
            GL.Color(Color.red);
            GL.Vertex(trianglesTarget[CurrentFrame][1]);
            GL.Color(Color.yellow);
            GL.Vertex(trianglesTarget[CurrentFrame][2]);
            GL.Color(Color.green);
            GL.Vertex(trianglesTarget[CurrentFrame][0]);
            GL.End();

        }
    }


    private void EventRaiserForCam_Motion_OnPostRenderEvent(object sender, CameraEventArgs e)
    {
        drawSequence(motion);
    }


    private void EventRaiserForCam_Estimation_OnPostRenderEvent(object sender, CameraEventArgs e)
    {
        Vector3 center = e.cam.transform.position;
        gl.drawAxes(Color.white, center);
        /* Draw results. */

        if (CurrentFrame>=0 && CurrentFrame<framesLength && estimation!=null && estimation[CurrentFrame]!=null && estimation[CurrentFrame].projection!=null && estimation[CurrentFrame].projection.joints!=null && estimation[CurrentFrame].projection.joints.Length != 0)
        {
            gl.drawFigure(true, Color.green, estimation[CurrentFrame].projection.joints, null, center);
        }
    }


    private void updateCamera_Motion_position()
    {
        if (Input.GetKey("s"))
        {
            MotionSequenceCam.transform.position -= speed;
        }
        if (Input.GetKey("w"))
        {
            MotionSequenceCam.transform.position += speed;
        }
    }

    private void drawSequence(List<BvhProjection> motion)
    {
        Color color;
        Vector3 position = Vector3.zero;
        int counter = 0;
        foreach (BvhProjection frame in motion)
        {
            if (counter == CurrentFrame)
                color = Color.red;
            else
                color = colorDefault;

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
        for (int i = 0; i < bvh.frameCount; i++)
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
            if(DisplayCorrectRoot)
                newProjection.rotatePositions(estimation[CurrentFrame].projection.angle);
            list.Add(newProjection);
        }
        return list;
    }


    private void setVideoPlayer()
    {
        videoPlayer = VideoplayerGO.GetComponent(typeof(UnityEngine.Video.VideoPlayer)) as UnityEngine.Video.VideoPlayer;
        videoPlayer.url = videoUrl;
    }

    private void updateFrame()
    {
        if (automatic)
        {
            if (videoPlayer.isPaused)
                videoPlayer.Play();

            CurrentFrame = (int)videoPlayer.frame;

            if (CurrentFrame >= framesLength || CurrentFrame < 0)
            {
                CurrentFrame = 0;
            }

        }
        else
        {
            Vector3 pos = transform.position;
            if (Input.GetKeyDown("d"))
            {
                CurrentFrame++;
                if (CurrentFrame >= framesLength)
                    CurrentFrame = 0;
            }
            if (Input.GetKeyDown("a"))
            {
                CurrentFrame--;
                if (CurrentFrame < 0)
                    CurrentFrame = framesLength - 1;
            }

            /* Show video on Current frame. */
            if (!videoPlayer.isPaused)
                videoPlayer.Pause();
            videoPlayer.frame = CurrentFrame;
        }
    }

    public void ExportBvh()
    {
        BvhExporter = new BvhExport(Path.GetFullPath(bvhTemplate), estimation);
        lastBvhFilePath = exportDir + "/test";
        BvhExporter.CreateBvhFile(lastBvhFilePath);
        bvhUrl = lastBvhFilePath+".bvh";
    }


    private void updateInfo()
    {
        string s = "";
        if(CurrentFrame>=0 && CurrentFrame<framesLength && estimation !=null && estimation[CurrentFrame].projection!=null & estimation[CurrentFrame].projection.joints != null)
        {
            Neighbour selected = estimation[CurrentFrame];
            s += "Frame: " + CurrentFrame;
            s += "\nAngle: " + selected.projection.angle;
            Vector3 rootRot = Base.base_rotationFiles[selected.projection.rotationFileID][selected.projection.frameNum].getAllRotations()[0];
            s += "\nInitial Rot:" + rootRot;
        }
        textInfo.text = s;
    }


}
