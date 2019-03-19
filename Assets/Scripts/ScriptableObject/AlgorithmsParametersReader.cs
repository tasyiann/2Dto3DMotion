using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmsParametersReader : MonoBehaviour
{
    private static AlgorithmsParametersReader instance;
    public static AlgorithmsParametersReader Instance { get { return instance; } }

    void Awake()
    {
        instance = this;
    }

    [SerializeField]
    private AlgorithmsParameters parameters;
    public AlgorithmsParameters Parameters { get { return parameters;  } }

}
