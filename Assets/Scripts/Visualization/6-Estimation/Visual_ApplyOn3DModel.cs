using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Visual_ApplyOn3DModel : MonoBehaviour
{
    public enum smoothMovement
    {
        LERP, ONE_EURO, NONE
    }

    public smoothMovement AnimationSmoothness;
    public DataInFrame script;

    #region OneEuroFilterParams
    // 1 Euro filter - JOINTS
    OneEuroFilter<Quaternion>[] rotationFiltersJoints = new OneEuroFilter<Quaternion>[14];
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
    #endregion OneEuroFilterParams

    public bool keepUpdatingParameters = false;
    public Transform model;
    private Model3D m3d;
    private Vector3[] jointsPositions;


    
    // Start is called before the first frame update
    void Start()
    {
        m3d = new Model3D(model);
        initializeOneEuroFilter();
    }

    // Update is called once per frame
    void Update()
    {
        jointsPositions = script.selectedPoseToDebug.selectedN.projection.joints;

        if (keepUpdatingParameters)
            updateParametersRotationFilters();

        if (jointsPositions != null && jointsPositions.Length!=0)
        {
            // Set filtered 3d model
            switch (AnimationSmoothness)
            {
                case smoothMovement.LERP: { m3d.moveSkeletonLERP(jointsPositions);                                                           break; }
                case smoothMovement.ONE_EURO: { m3d.moveSkeleton_OneEuroFilter(jointsPositions, rotationFiltersJoints, rotationFilterHips);  break; }
                default: { m3d.moveSkeleton(jointsPositions);                                                                                break; }
            }
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

    private void initializeOneEuroFilter()
    {
        for (int i = 0; i < rotationFiltersJoints.Length; i++)
        {
            rotationFiltersJoints[i] = new OneEuroFilter<Quaternion>(filterFrequency);
        }
        rotationFilterHips = new OneEuroFilter<Quaternion>(filterFrequency);
        updateParametersRotationFilters();
    }


}
