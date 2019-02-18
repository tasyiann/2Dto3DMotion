﻿// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
namespace OpenPose.Example {
    /*
     * User of OPWrapper
     */
    public class OpenPoseUserScript : MonoBehaviour
    {
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
        public bool update1EuroValues;

        // 3D Estimation
        public Transform model;
        private Model3D m3d;
        private static Scenario sc = Base.sc;
        private static List<List<BvhProjection>> base_clusters = Base.base_clusters;            // Clustered projections.
        public static List<BvhProjection> base_main_representatives = Base.base_main_representatives;        // All main representatives.
        public static List<List<BvhProjection>> base_main_clusters = Base.base_main_clusters;                // All main clusters.
        private static List<BvhProjection> base_representatives = Base.base_representatives;    // Representatives.
        private static List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;      // Rotations.
        public static Vector3[] estimation_to_debug = null;                                     // To debug 2D figure
        public static Vector3[] rawInputToDebug = null;
        public static OPPose figureToDebug = null;


        // The 2D human to control
        [SerializeField]
        Transform humanContainer;
        [SerializeField]
        ImageRenderer imageRenderer;
        [SerializeField]
        Text fpsText;
        [SerializeField]
        Text peopleText;

        // Output control
        private OPDatum datum;

        // User settings
        public ProducerType inputType = ProducerType.Webcam;
        public string producerString = "-1";
        public int maxPeople = -1;
        public float renderThreshold = 0.05f;
        public bool
            handEnabled = false,
            faceEnabled = false;
        public Vector2Int
            netResolution = new Vector2Int(-1, 368),
            handResolution = new Vector2Int(368, 368),
            faceResolution = new Vector2Int(368, 368);
        public void SetHandEnabled(bool enabled) { handEnabled = enabled; }
        public void SetFaceEnabled(bool enabled) { faceEnabled = enabled; }
        public void SetRenderThreshold(string s) { float res; if (float.TryParse(s, out res)) { renderThreshold = res; }; }
        public void SetMaxPeople(string s) { int res; if (int.TryParse(s, out res)) { maxPeople = res; }; }
        public void SetPoseResX(string s) { int res; if (int.TryParse(s, out res)) { netResolution.x = res; }; }
        public void SetPoseResY(string s) { int res; if (int.TryParse(s, out res)) { netResolution.y = res; }; }
        public void SetHandResX(string s) { int res; if (int.TryParse(s, out res)) { handResolution.x = res; }; }
        public void SetHandResY(string s) { int res; if (int.TryParse(s, out res)) { handResolution.y = res; }; }
        public void SetFaceResX(string s) { int res; if (int.TryParse(s, out res)) { faceResolution.x = res; }; }
        public void SetFaceResY(string s) { int res; if (int.TryParse(s, out res)) { faceResolution.y = res; }; }

        public void ApplyChanges()
        {
            // Restart OpenPose
            StartCoroutine(UserRebootOpenPoseCoroutine());
        }

        // Bg image
        public bool renderBgImg = false;
        public void ToggleRenderBgImg()
        {
            renderBgImg = !renderBgImg;
            OPWrapper.OPEnableImageOutput(renderBgImg);
            imageRenderer.FadeInOut(renderBgImg);
        }

        // Number of people
        int numberPeople = 0;

        // Frame rate calculation
        [Range(0f, 1f)]
        public float frameRateSmoothRatio = 0.8f;
        private float avgFrameRate = 0f;
        private float avgFrameTime = -1f;
        private float lastFrameTime = -1f;




        private void Start()
        {
            // Initialise 3D model
            m3d = new Model3D(model);

            // Register callbacks
            OPWrapper.OPRegisterCallbacks();
            // Enable OpenPose run multi-thread (default true)
            OPWrapper.OPEnableMultiThread(true);
            // Enable OpenPose log to unity (default true)
            OPWrapper.OPEnableDebug(true);
            // Enable OpenPose output to unity (default true)
            OPWrapper.OPEnableOutput(true);
            // Enable receiving image (default false)
            OPWrapper.OPEnableImageOutput(renderBgImg);

            // Configure OpenPose with default value, or using specific configuration for each
            /* OPWrapper.OPConfigureAllInDefault(); */
            UserConfigureOpenPose();

            // Set 1euro filter
            for (int i = 0; i < rotationFiltersJoints.Length; i++)
                rotationFiltersJoints[i] = new OneEuroFilter<Quaternion>(filterFrequency);
            rotationFilterHips = new OneEuroFilter<Quaternion>(filterFrequency);

            // Start OpenPose
            OPWrapper.OPRun();

            
        }

        // User can change the settings here
        private void UserConfigureOpenPose()
        {
            OPWrapper.OPConfigurePose(
                /* enable */ true, /* netInputSize */ netResolution, /* outputSize */ null,
                /* keypointScaleMode */ ScaleMode.InputResolution,
                /* gpuNumber */ -1, /* gpuNumberStart */ 0, /* scalesNumber */ 1, /* scaleGap */ 0.3f,
                /* renderMode */ RenderMode.Gpu, /* poseModel */ PoseModel.COCO_18, // << CHANGED
                                                                                    /* blendOriginalFrame */ true, /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f,
                /* defaultPartToRender */ 0, /* modelFolder */ null,
                /* heatMapTypes */ HeatMapType.None, /* heatMapScaleMode */ ScaleMode.UnsignedChar,
                /* addPartCandidates */ false, /* renderThreshold */ renderThreshold, /* numberPeopleMax */ maxPeople,
                /* maximizePositives */ false, /* fpsMax fps_max */ -1.0,
                /* protoTxtPath */ "", /* caffeModelPath */ "");

            OPWrapper.OPConfigureHand(
                /* enable */ handEnabled, /* netInputSize */ handResolution,
                /* scalesNumber */ 1, /* scaleRange */ 0.4f, /* tracking */ false,
                /* renderMode */ RenderMode.None,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.2f);

            OPWrapper.OPConfigureFace(
                /* enable */ faceEnabled, /* netInputSize */ faceResolution, /* renderMode */ RenderMode.None,
                /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.4f);

            OPWrapper.OPConfigureExtra(
                /* reconstruct3d */ false, /* minViews3d */ -1, /* identification */ false, /* tracking */ -1,
                /* ikThreads */ 0);

            OPWrapper.OPConfigureInput(
                /* producerType */ inputType, /* producerString */ producerString,
                /* frameFirst */ 0, /* frameStep */ 1, /* frameLast */ ulong.MaxValue,
                /* realTimeProcessing */ false, /* frameFlip */ false,
                /* frameRotate */ 0, /* framesRepeat */ false,
                /* cameraResolution */ null, /* cameraParameterPath */ null,
                /* undistortImage */ false, /* numberViews */ -1);

            OPWrapper.OPConfigureOutput(
                /* verbose */ -1.0, /* writeKeypoint */ "", /* writeKeypointFormat */ DataFormat.Yml,
                /* writeJson */ "", /* writeCocoJson */ "", /* writeCocoFootJson */ "", /* writeCocoJsonVariant */ 1,
                /* writeImages */ "", /* writeImagesFormat */ "png", /* writeVideo */ "", /* writeVideoFps */ 30.0,
                /* writeHeatMaps */ "", /* writeHeatMapsFormat */ "png", /* writeVideo3D */ "",
                /* writeVideoAdam */ "", /* writeBvh */ "", /* udpHost */ "", /* udpPort */ "8051");

            OPWrapper.OPConfigureGui(
                /* displayMode */ DisplayMode.NoDisplay, /* guiVerbose */ false, /* fullScreen */ false);
        }

        private IEnumerator UserRebootOpenPoseCoroutine()
        {
            if (OPWrapper.state == OPState.None) yield break;
            // Shutdown if running
            if (OPWrapper.state == OPState.Running)
            {
                OPWrapper.OPShutdown();
            }
            // Wait until fully stopped
            yield return new WaitUntil(() => { return OPWrapper.state == OPState.Ready; });
            // Configure and start
            UserConfigureOpenPose();
            OPWrapper.OPRun();
        }


        public static List<OPFrame> frames = new List<OPFrame>();
        private static int currentframeIndex = 0;
        // HERE
        // This fuction is made by me
        private Vector3[] setFigure(ref OPDatum datum, int personID, float scoreThres, OPFrame frame)
        {
            if (datum.poseKeypoints == null || personID >= datum.poseKeypoints.GetSize(0))
            {
                return null;
            }

            // Identification TODO::::
            long id = datum.poseIds.Get(personID);
            //Debug.Log("The id is:"+id);


            // Pose
            Vector3[] joints = new Vector3[OPPose.KEYPOINTS_NUMBER];
            bool[] available = new bool[OPPose.KEYPOINTS_NUMBER];
            for (int i = 0; i < joints.Length; i++)
            {
                // Compare score
                if (datum.poseKeypoints.Get(personID, i, 2) <= scoreThres)
                {
                    available[i] = false;
                }
                else
                {
                    // Save new positions
                    available[i] = true; 
                    // people x bodyparts x (x,y,score)
                    joints[i] = new Vector3(datum.poseKeypoints.Get(personID, i, 0), datum.poseKeypoints.Get(personID, i, 1), 0f);
                }
            }
            // SET NEW FIGURE!
            rawInputToDebug = new Vector3[14];
            Array.Copy(joints, rawInputToDebug, joints.Length);
            OPPose poseNEW = new OPPose(joints, available, frames, currentframeIndex);
            frame.figures.Add(poseNEW);
            figureToDebug = poseNEW;
            //Debug.Log("RAW POSITIONS translated to 000 \n"+poseNEW.jointsToString(true));
            return getEstimation(poseNEW);

        }



        OPPose prevFigure=null;
        private Vector3[] getEstimation(OPPose figure)
        {
            // STEP_A: Find k-BM.
            sc.algNeighbours.SetNeighbours(figure, sc.k, base_clusters, base_representatives, base_main_representatives, base_main_clusters);
            // STEP_B: Find Best 3D.
            figure.selectedN = sc.algEstimation.GetEstimation(prevFigure, figure, sc.m, base_rotationFiles);
            //Debug.Log("Estimation of frame "+currentframeIndex+ ":\n"+figure.selectedN.projection.jointsToString());
            // Set the figure as the previous one, and go to the next frame.
            prevFigure = figure;
            return figure.selectedN.projection.joints;
        }


        private void updateParametersRotationFilters()
        {
            rotationFilterHips.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            foreach (OneEuroFilter<Quaternion> rotfilter in rotationFiltersJoints)
            {
                rotfilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            }
        }

        private void Update()
        {

            // New data received
            if (OPWrapper.OPGetOutput(out datum))
            {
                // Add new frame to the List
                OPFrame currframe = new OPFrame();
                frames.Add(currframe);
                currentframeIndex++;

                // Update 1 euro filter values
                if (update1EuroValues)
                {
                    updateParametersRotationFilters();
                }

                // Visualize 3D
                int personID = 0;
                Vector3[] estimation = setFigure(ref datum, personID, renderThreshold, currframe);
                estimation_to_debug = estimation;
                if (estimation != null)
                {
                    m3d.moveSkeleton_OneEuroFilter(estimation,rotationFiltersJoints, rotationFilterHips);
                }


                // Draw human in data
                int i = 0;
                foreach (var human in humanContainer.GetComponentsInChildren<HumanController2D>())
                {
                    human.DrawHuman(ref datum, i++, renderThreshold);
                }

                // Draw image
                imageRenderer.UpdateImage(datum.cvInputData);

                // Number of people
                if (datum.poseKeypoints == null || datum.poseKeypoints.Empty()) numberPeople = 0;
                else numberPeople = datum.poseKeypoints.GetSize(0);
                peopleText.text = "People: " + numberPeople;

                // Calculate framerate
                if (lastFrameTime > 0f)
                {
                    if (avgFrameTime < 0f) avgFrameTime = Time.time - lastFrameTime;
                    else
                    {
                        avgFrameTime = Mathf.Lerp(Time.time - lastFrameTime, avgFrameTime, frameRateSmoothRatio);
                        avgFrameRate = 1f / avgFrameTime;
                    }
                }
                lastFrameTime = Time.time;
                fpsText.text = avgFrameRate.ToString("F1") + " FPS";
            }
        }


    }
}
