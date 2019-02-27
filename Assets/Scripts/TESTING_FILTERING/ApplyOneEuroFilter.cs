using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyOneEuroFilter : MonoBehaviour
{
    OneEuroFilter<Quaternion> rotationFilter;
    OneEuroFilter<Vector3> positionFilter;
    public bool filterOn = true;
    public bool keepUpdatingParams = false;
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;
    public float noiseAmount = 1.0f;
    float timer = 0.0f;

    void Start()
    {
        rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
        positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        if (filterOn)
        {
            if(keepUpdatingParams)
                rotationFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);

            transform.rotation = rotationFilter.Filter(transform.rotation);
            transform.position = positionFilter.Filter(transform.position);
        }
    }
}
