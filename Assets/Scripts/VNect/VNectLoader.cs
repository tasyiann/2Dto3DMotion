using System.Collections.Generic;
using Tomis.UnityEditor.Utilities;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VNectHacker;
using static VNectHacker.DataLoader;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Linq;
using System;
using static VNectHacker.VNectSkeleton;
using System.Text;

[CustomEditor(typeof(VNectLoader))]
public class ObjectBuilderEditorVNect : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Load, Compare, and Fbx Export Data.");
        
        VNectLoader myScript = (VNectLoader)target;

        if (GUILayout.Button("Update FPS"))
        {
            myScript.automaticIncrementFrameIndex();
        }
        if (GUILayout.Button(myScript.RecordState ? "Stop Manually Recording FBX [ybot_A]" : "Start Manually Recording FBX [ybot_A]"))
        {
            myScript.RecordState = !myScript.RecordState;
        }
        EditorGUI.BeginDisabledGroup(!myScript.playMode || myScript.RecordState);
        if(GUILayout.Button("Automatic Recording [ybot_A]"))
        {
            myScript.AutomaticState = true;
            myScript.RecordState = true;
            
        }

        

        //if (GUILayout.Button("Load Images As Textures"))
        //{
            // myScript.textures = myScript.ConvertImagesAsTextures(myScript.ImagesPath);
        //}
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Compare Positions! [Skeletons]"))
        {
            if (myScript.frames_A.Count != myScript.frames_B.Count)
            {
                Debug.LogError("Cannot compare Skeletons due to different number of frames!" +
                    "A: "+myScript.frames_A.Count + ", B: "+myScript.frames_B.Count);
            }
            else
            {
                myScript.AverageDistance = myScript.CalculateDistance();
            }
        }

        if(GUILayout.Button("Export Positions from [ybot_A]"))
        {
            myScript.exportPositionsFromModel3D();
        }


        myScript.foldOut1 = EditorGUILayout.Foldout(myScript.foldOut1, "Data");
        if (myScript.foldOut1)
        {
            var level = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            DrawDefaultInspector();
            EditorGUI.indentLevel = level;
        }

        myScript.Filter = (VNectLoader.Filters)EditorGUILayout.EnumPopup("Filter",myScript.Filter);

        myScript.foldOut2 = EditorGUILayout.Foldout(myScript.foldOut2, "1EuroFilter");
        if (myScript.foldOut2)
        {
            var level = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            myScript.FilterFrequency = EditorGUILayout.FloatField("Frequency",myScript.FilterFrequency);
            myScript.FilterMinCutoff = EditorGUILayout.FloatField("Min Cutoff", myScript.FilterMinCutoff);
            myScript.FilterBeta = EditorGUILayout.FloatField("Beta", myScript.FilterBeta);
            myScript.FilterDcutoff = EditorGUILayout.FloatField("Dcutoff", myScript.FilterDcutoff);
            myScript.FilterNoiseAmount = EditorGUILayout.FloatField("Noise Amount", myScript.FilterNoiseAmount);

            EditorGUI.indentLevel = level;
        }

    }
}



public class VNectLoader : MonoBehaviour
{
    #region Variables

    public bool foldOut1 { get; set; }
    public bool foldOut2 { get; set; }
 
    [HideInInspector]
    public bool editInspector = true;
    [HideInInspector]
    public bool playMode = false;
    [Header("Set GameObjects")]
    public GameObject prefabSkeleton_A;
    public GameObject prefabSkeleton_B;
    public RawImage Image;
    public Text textInfo;
    public Material material;
    public Transform model3D;
    private Model3D modelController;

    // Video Players
    public GameObject VideoplayerGO_A;
    public GameObject VideoplayerGO_B;
    private UnityEngine.Video.VideoPlayer videoPlayer_A;
    private UnityEngine.Video.VideoPlayer videoPlayer_B;

    public readonly int NumberOfJoints = 15;

    

    [Header("Import Data")]
    [FileSelect(AssetRelativePath = true, ButtonName = "A : Data File", SelectMode = FileSelectionMode.File)]
    public string dataDirPath_A;
    [FileSelect(AssetRelativePath = true, ButtonName = "B : Data File", SelectMode = FileSelectionMode.File, DisplayWarningWhenNotSelected = false)]
    public string dataDirPath_B;
    [FileSelect(AssetRelativePath = true, ButtonName = "Video A", SelectMode = FileSelectionMode.File, DisplayWarningWhenNotSelected = false)]
    public string VideoPath_A;
    [FileSelect(AssetRelativePath = true, ButtonName = "Video B", SelectMode = FileSelectionMode.File, DisplayWarningWhenNotSelected = false)]
    public string VideoPath_B;


    public DataType FileType_A;
    public DataType FileType_B;
    public bool NormalizeSkeleton = true;
    public float SkeletonHeight = 175f;
    public bool FixedRootRotation = false;
    public bool Show3DModel = false;
    public bool showDistances = false;
    [ReadOnly]
    public float AverageDistance = 0;

    [Header("Export to Fbx")]
    [FileSelect(AssetRelativePath = true, ButtonName = "Path to save Fbx", SelectMode = FileSelectionMode.Folder)]
    public string PathToSaveFbx;
    public string Clipfilename;
    public int FramesPerSecond = 24;
    private string Clipfilename_Ready;

    // 1st Data
    private VNectSkeleton skeleton_A;
    public List<VNectFrame> frames_A;
    // 2nd Data
    private VNectSkeleton skeleton_B;
    public List<VNectFrame> frames_B;


    private bool recordState;
    public bool RecordState { get => recordState;
        set
        {
            recordState = value;
            Debug.Log("Inside setter: " + RecordState.ToString());
            if (RecordState)
                startRecording();
            else
                stopRecording();
        }
    }
    private GameObjectRecorder m_Recorder;
    private AnimationClip clip;

    private bool automaticState;
    public bool AutomaticState { get => automaticState;
        set
        {
            automaticState = value;
            if (automaticState)
            {
                FrameIndx = 0;
            }
        }
    }

    [FileSelect(AssetRelativePath = true, ButtonName = "Path to images", SelectMode = FileSelectionMode.Folder)]
    public string ImagesPath;
    public Vector2 TextureSize;
    [HideInInspector]
    public List<Texture2D> textures = null;

    #endregion Variables

    #region FrameIndex
    private int frameIndx;
    public int FrameIndx
    {
        get { return frameIndx; }
        set
        {
            frameIndx = value < 0 ? frames_A.Count - 1 : (value % frames_A.Count);

            if (skeleton_A != null)
                skeleton_A.Joints = frames_A[frameIndx].SkeletonJoints;

            if (skeleton_B != null)
                skeleton_B.Joints = frames_B[frameIndx].SkeletonJoints;

            if (textures != null && textures.Count > FrameIndx)
                Image.texture = textures[FrameIndx];
            if (textInfo != null)
            {
                if (!showDistances)
                    textInfo.text = defaultInfoText();
                else
                    textInfo.text = distancesInfoText();
            }

            if (Show3DModel)
            {
                if (!model3D.gameObject.activeSelf) model3D.gameObject.SetActive(true);
                if (model3D != null && modelController == null)
                {
                    modelController = new Model3D(model3D);
                }
                Move3Dmodel();
            }
            else
            {
                if (model3D.gameObject.activeSelf) model3D.gameObject.SetActive(false);
            }

            updateVideoPlayers();
        }
    }
    #endregion FrameIndex

    #region 1EuroFilter
    public enum Filters { None, NEW, OneEuro, SGolay, Lerp }
    public Filters Filter { get; set; }
    [HideInInspector, SerializeField]
    private float _filterFrequency;
    public float FilterFrequency { get => _filterFrequency; set { _filterFrequency = value; updateParametersRotationFilters(); } }
    [HideInInspector, SerializeField]
    private float _filterMinCutoff;
    public float FilterMinCutoff { get=> _filterMinCutoff; set { _filterMinCutoff = value; updateParametersRotationFilters(); } }
    [HideInInspector, SerializeField]
    private float _filterBeta;
    public float FilterBeta { get => _filterBeta; set { _filterBeta = value; updateParametersRotationFilters(); } }
    [HideInInspector, SerializeField]
    private float _filterDcutoff;
    public float FilterDcutoff { get => _filterDcutoff; set { _filterDcutoff = value; updateParametersRotationFilters(); } }
    [HideInInspector, SerializeField]
    private float _noiseAmount;
    public float FilterNoiseAmount { get => _noiseAmount; set { _noiseAmount = value; updateParametersRotationFilters(); } }
    private List<OneEuroFilter<Quaternion>> rotationFiltersJoints = new List<OneEuroFilter<Quaternion>>();


    private void initializeOneEuroFilter()
    {
        for (int i = 0; i < NumberOfJoints; i++)
        {
            rotationFiltersJoints.Add( new OneEuroFilter<Quaternion>(FilterFrequency));
        }
    }

    private void updateParametersRotationFilters()
    {
        foreach (OneEuroFilter<Quaternion> rotfilter in rotationFiltersJoints)
        {
            rotfilter.UpdateParams(FilterFrequency, FilterMinCutoff, FilterBeta, FilterDcutoff);
        }
    }




    #endregion 1EuroFilter

    void Start()
    {
        Filter = Filters.None;
        playMode = true;

        if (dataDirPath_A != null && dataDirPath_A.Length!=0)
        {
            DataLoader loader = new DataLoader(dataDirPath_A);
            frames_A = loader.getAllFrames(FileType_A);
            skeleton_A = new VNectSkeleton(prefabSkeleton_A, Clipfilename, frames_A, FixedRootRotation, NumberOfJoints, NormalizeSkeleton, FileType_A, SkeletonHeight);
            skeleton_A.material = material;
            automaticIncrementFrameIndex();
        }

        if (dataDirPath_B != null && dataDirPath_B.Length != 0)
        {
            DataLoader loader = new DataLoader(dataDirPath_B);
            frames_B = loader.getAllFrames(FileType_B);
            skeleton_B = new VNectSkeleton(prefabSkeleton_B, Clipfilename+"_B", frames_B, FixedRootRotation, NumberOfJoints, NormalizeSkeleton, FileType_B, SkeletonHeight);
        }

        initializeVideoPlayers();
        initializeOneEuroFilter();
    }


    private string distancesInfoText()
    {
        if (frames_A == null || frames_B == null)
            return "Couldn't compare skeletons.\n";

        StringBuilder s = new StringBuilder();
        string[] JointNames = Enum.GetNames(typeof(JointsDefinition));
        List<Vector3> jointsA = skeleton_A.Joints;
        s.AppendFormat("Frame:{0}\nJoint Distances:\n",FrameIndx);
        for (int i=0; i<NumberOfJoints; i++)
        {
            float dist = Vector3.Distance(skeleton_A.Joints[i], skeleton_B.Joints[i]);
            s.AppendFormat("{0}: {1}\n",JointNames[i],dist);
        }
        s.AppendFormat("Average dist for {0} frames: {1}\n",frames_A.Count, AverageDistance);
        s.AppendFormat("A' skeleton:\n" + skeleton_A.skeletonInfoScale());
        s.AppendFormat("\nB' skeleton:\n" + skeleton_B.skeletonInfoScale());
        return s.ToString();
    }

    private string defaultInfoText()
    {
        return (skeleton_A != null ? "SKELETON A:\n-------------\nFrame: " + frameIndx + "\n" + skeleton_A.ToString() + "\n" : "") +
               (skeleton_B != null ? "SKELETON B:\n-------------\nFrame: " + frameIndx + "\n" + skeleton_B.ToString() : "");
    }

    // Simulate motion and take values meanwhile!
    public void exportPositionsFromModel3D()
    {
        StringBuilder s = new StringBuilder();
        int frameCounter = 0;
        s.AppendFormat("University of Cyprus 3D Estimation :" +
            " With 1 Euro Filtering (freq: {0}, minCutoff: {1}, beta:{2}, dcutoff:{3}, noiseAmount:{4})\n",
            FilterFrequency,FilterMinCutoff,FilterBeta,FilterDcutoff,FilterNoiseAmount);
        for(int i=0; i<frames_A.Count; i++)
        {
            s.AppendFormat("{0}",frameCounter);
            // Update Movement!
            skeleton_A.Joints = frames_A[i].SkeletonJoints;
            Move3Dmodel();
            // Take values!
            for(int j=0; j< modelController.JointsGameObjects.Count; j++)
            {
                Vector3 joint = modelController.JointsGameObjects[j].transform.position;
                s.AppendFormat(", {0}, {1}, {2}", joint.x, joint.y, joint.z);
            }
            s.Append("\n");
            frameCounter++;
        }
        File.WriteAllText("UCY.filtered", s.ToString());
        Debug.Log("File has been exported!");
    }




    private void Move3Dmodel()
    {
        switch (Filter)
        {
            case Filters.None:
                modelController.moveSkeleton(skeleton_A.Joints.ToArray());
                break;
            case Filters.NEW:
                modelController.moveSkeletonNEW(skeleton_A.Joints.ToArray());
                break;
            case Filters.OneEuro:
                modelController.moveSkeleton_OneEuroFilter(skeleton_A.Joints.ToArray(),rotationFiltersJoints.ToArray(), rotationFiltersJoints[(int)JointsDefinition.Root]);
                break;
            case Filters.SGolay:
                break;
            case Filters.Lerp:
                modelController.moveSkeletonLERP(skeleton_A.Joints.ToArray());
                break;
        }
    }

    private void initializeVideoPlayers()
    {

        if (VideoplayerGO_A != null && VideoPath_A != null && VideoPath_A.Length!=0)
        {
            videoPlayer_A = VideoplayerGO_A.GetComponent(typeof(UnityEngine.Video.VideoPlayer)) as UnityEngine.Video.VideoPlayer;
            videoPlayer_A.url = VideoPath_A;
            videoPlayer_A.Pause();
     
        }
        if (VideoplayerGO_B != null && VideoPath_B != null && VideoPath_B.Length!= 0)
        {
            videoPlayer_B = VideoplayerGO_B.GetComponent(typeof(UnityEngine.Video.VideoPlayer)) as UnityEngine.Video.VideoPlayer;
            videoPlayer_B.url = VideoPath_B;
            videoPlayer_B.Pause();
        
        }
    }

    private void updateVideoPlayers()
    {
        if (videoPlayer_A != null)
        {
            videoPlayer_A.frame = FrameIndx;
        }
        if (videoPlayer_B != null)
        {
            videoPlayer_B.frame = FrameIndx;
        }
    }

    public void automaticIncrementFrameIndex()
    {
        CancelInvoke();
        InvokeRepeating("IncrementFrameIndxByFps", 0f, 1f / FramesPerSecond);
    }

    private void Update()
    {
        if (Input.GetKeyDown("w"))
        {
            FrameIndx++;
        }
            
        if (Input.GetKeyDown("s"))
        {
            FrameIndx--;
        }
            
    }

    public void stopRecording()
    {
        Debug.Log("Stop Recording");
        if (m_Recorder.isRecording)
        {
            // Create clip in the specified directory
            clip = new AnimationClip();
            // "record" is off, but we were recording:
            // save to clip and clear recording.
            m_Recorder.SaveToClip(clip);
            m_Recorder.ResetRecording();
            // Save clip.
            AssetDatabase.CreateAsset(clip, Clipfilename_Ready + ".anim");
            Debug.Log("Animation clip has been created.");
        }
    }

    public void startRecording()
    {
        // Create the GameObjectRecorder.
        m_Recorder = new GameObjectRecorder(skeleton_A.SkeletonGameObject);
        // Bind all the Transforms on the GameObject and all its children.
        m_Recorder.BindComponentsOfType<Transform>(skeleton_A.SkeletonGameObject, true);
        Debug.Log("Start Recording");
        if (Clipfilename == null)
            return;
        string PathToSaveFbx_Ready = "Assets" + PathToSaveFbx.Substring(Application.dataPath.Length); // Important!
        Clipfilename_Ready = PathToSaveFbx_Ready + @"/" + Clipfilename;
    }

    void LateUpdate()
    {

        if (recordState)
            m_Recorder.TakeSnapshot(Time.deltaTime);


        if (AutomaticState)
        {
            if(FrameIndx == frames_A.Count - 1)
            {
                stopRecording();
                AutomaticState = false;
                RecordState = false;
            }
        }


    }

    public float CalculateDistance()
    {
        float overallDistance = 0;
        for (int f=0; f<frames_A.Count; f++)
        {
            float distanceInFrame = 0;
            Assert.IsTrue(frames_A[f].SkeletonJoints.Count == frames_B[f].SkeletonJoints.Count);
            for (int i = 0; i < frames_A[f].SkeletonJoints.Count; i++)
            {
                distanceInFrame += Vector3.Distance(frames_A[f].SkeletonJoints[i], frames_B[f].SkeletonJoints[i]);
            }
            overallDistance += distanceInFrame / frames_A[f].SkeletonJoints.Count;
        }
        return overallDistance / frames_A.Count;
    }

    void IncrementFrameIndxByFps()
    {
        FrameIndx++;

    }

    /* Create Textures from Images */
   
    private Texture2D createTexture(string filename)
    {
        Texture2D texture = new Texture2D((int)(TextureSize.x), (int)(TextureSize.y));
        byte[] binaryImageData = File.ReadAllBytes(@filename);
        texture.LoadImage(binaryImageData);
        texture.Compress(false);
        return texture;
    }

    public List<Texture2D> ConvertImagesAsTextures(string path)
    {
        List<Texture2D> textures = new List<Texture2D>();
        var sorted = Directory.GetFiles(path).OrderBy(f => f);
        foreach(var fileName in sorted)
        {
            if(fileName.EndsWith(".jpg"))
                textures.Add(createTexture(fileName));
        }
        //WriteToBinaryFile(path+@"\Textures.mat",textures);
        return textures;
    }

    private void OnPostRender()
    {
        // Define a triangle plane
        Vector3 s1, s2, s3, t1, t2, t3;
        s1 = skeleton_A.JointsGameObjects[(int)JointsDefinition.LeftUpLeg].position;
        s2 = skeleton_A.JointsGameObjects[(int)JointsDefinition.RightUpLeg].position;
        s3 = skeleton_A.JointsGameObjects[(int)JointsDefinition.Neck].position;
        t1 = new Vector3(-1f, 0f, 0f) * s1.magnitude;
        t2 = new Vector3(1f, 0f, 0f) * s2.magnitude;
        t3 = s3;

        // Visualisation
        GL.Begin(GL.LINE_STRIP);
        material.SetPass(0);
        GL.Color(Color.red);
        GL.Vertex(s1);
        GL.Vertex(s2);
        GL.Vertex(s2);
        GL.Vertex(s3);
        GL.Vertex(s1);

        GL.Color(Color.green);
        GL.Vertex(t1);
        GL.Vertex(t2);
        GL.Vertex(t2);
        GL.Vertex(t3);
        GL.Vertex(t1);
        GL.End();
    }

}
