using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyOneEuroFilter : MonoBehaviour
{
    public Transform noisyTransform;
    public Transform filteredTransform;
    Quaternion quat;


    OneEuroFilter<Quaternion> rotationFilter;
    public bool filterOn = true;
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;

    public float noiseAmount = 1.0f;
    float timer = 0.0f;

    void Start()
    {
        rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        if (filterOn)
        {
            rotationFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            filteredTransform.rotation = rotationFilter.Filter(noisyTransform.rotation);
        }
        else
            filteredTransform.rotation = noisyTransform.rotation;
    }
}
