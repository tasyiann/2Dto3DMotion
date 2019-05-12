using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Globalization;

public class Visual_ApplyOn3DModel : MonoBehaviour
{
    public enum smoothMovement
    {
        LERP, ONE_EURO, SGOLAY, NONE
    }

    public bool multiFigures; // Please tick this if you're going to use Multi figures script.
    public smoothMovement AnimationSmoothness;
    public DataInFrame script;
    public OPPose selectedPoseToDebug;
    #region OneEuroFilterParams
    // 1 Euro filter - JOINTS
    static int JointsLength = Enum.GetValues(typeof(EnumModel3DJoints)).Length;
    OneEuroFilter<Quaternion>[] rotationFiltersJoints = new OneEuroFilter<Quaternion>[JointsLength];
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;
    public float noiseAmount = 1.0f;

    // 1 Euro filter - HIPS
    /*
    OneEuroFilter<Quaternion> rotationFilterHips;
    public float filterFrequency_hips = 120.0f;
    public float filterMinCutoff_hips = 1.0f;
    public float filterBeta_hips = 0.0f;
    public float filterDcutoff_hips = 1.0f;
    public float noiseAmount_hips = 1.0f;
    */
    #endregion OneEuroFilterParams

    public bool keepUpdatingParameters = false;
    public Transform model;
    private Model3D m3d;

    private Vector3[] jointsPositions;

    private List<List<Vector3>> estimation_Sgolay;
    
    private void readEstimationWithSgolayFromFiles(string path)
    {
        estimation_Sgolay = new List<List<Vector3>>();
        
        for(int i=0; i<Base.numberOfJoints; i++)
        {
            string textFile = path+@"\"+i + "_joint_SGolayed.3D";
            using (StreamReader file = new StreamReader(textFile))
            {
                string ln;
                List<Vector3> joint = new List<Vector3>();
                while ((ln = file.ReadLine()) != null)
                {
                    string[] array = ln.Split(' ');

                    joint.Add(new Vector3(float.Parse(array[0], CultureInfo.InvariantCulture),
                        float.Parse(array[1], CultureInfo.InvariantCulture),
                        float.Parse(array[2], CultureInfo.InvariantCulture)));
                }
                file.Close();
                estimation_Sgolay.Add(joint);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if((int)AnimationSmoothness == (int)smoothMovement.SGOLAY)
        {
           readEstimationWithSgolayFromFiles(@"3DEstimations\SgolayApplied");
        }

        m3d = new Model3D(model);
        initializeOneEuroFilter();
        
        if(!multiFigures)
            selectedPoseToDebug = script.selectedPoseToDebug;
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedPoseToDebug == null)
            return;

        if (!multiFigures)
            selectedPoseToDebug = script.selectedPoseToDebug;

        if(selectedPoseToDebug!= null && selectedPoseToDebug.Estimation3D!=null && selectedPoseToDebug.Estimation3D.projection!=null)
            jointsPositions = selectedPoseToDebug.Estimation3D.projection.joints;

        if (keepUpdatingParameters)
            updateParametersRotationFilters();

        if (jointsPositions != null && jointsPositions.Length!=0)
        {
            // Set filtered 3d model
            switch (AnimationSmoothness)
            {
                case smoothMovement.LERP: { m3d.moveSkeletonLERP(jointsPositions);                                                           break; }
                case smoothMovement.ONE_EURO: { m3d.moveSkeleton_OneEuroFilter(jointsPositions, rotationFiltersJoints);                      break; }
                case smoothMovement.SGOLAY: { m3d.moveSkeletonLERP(getSgolayVectorOfJoints());                                               break; }
                default: { m3d.moveSkeleton(jointsPositions);                                                                                break; }
            }
        }
    }

    private Vector3[] getSgolayVectorOfJoints()
    {
        Vector3[] joints = new Vector3[Base.numberOfJoints];
        for(int i=0; i<estimation_Sgolay.Count; i++)
        {
            joints[i] = estimation_Sgolay[i][script.currentFrame];
        }
        return joints;
    }


    private void updateParametersRotationFilters()
    {
        
        foreach (OneEuroFilter<Quaternion> rotfilter in rotationFiltersJoints)
        {
            rotfilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        }
    }

    private void initializeOneEuroFilter()
    {
        for (int i = 0; i < rotationFiltersJoints.Length; i++)
        {
            rotationFiltersJoints[i] = new OneEuroFilter<Quaternion>(filterFrequency);
        }

        updateParametersRotationFilters();
    }


}
