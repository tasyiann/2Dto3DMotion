using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSystemParameters : MonoBehaviour
{
    public Text text_info;

    void Start()
    {
        if (Base.isBaseInitialized)
            text_info.text = "Database: \n" + DataBaseParametersReader.Instance.ToString() + "\nAlgorithm: \n" + AlgorithmsParametersReader.Instance.ToString();
        else
            text_info.text = "PLEASE LOAD DATABASE TO START 3D ESTIMATION.";
    }


}
