using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBaseParametersReader : MonoBehaviour
{

    private static DataBaseParametersReader instance;
    public static DataBaseParametersReader Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;
    }

    [SerializeField]
    private DataBaseParameters parameters;
    public DataBaseParameters Parameters { get { return parameters; } }


}
