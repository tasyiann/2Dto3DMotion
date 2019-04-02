using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    override public string ToString()
    {
        string s = "";
        foreach (var field in Parameters.GetType().GetFields())
        {
            s += field.Name + "= " + field.GetValue(parameters) + "\n";
        }
        return s;
    }
}
