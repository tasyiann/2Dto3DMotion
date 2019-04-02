using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSystemParameters : MonoBehaviour
{
    public Text text_info;

    void Start()
    {
        text_info.text = "Database: \n" + DataBaseParametersReader.Instance.ToString() + "\nAlgorithm: \n" + AlgorithmsParametersReader.Instance.ToString();
    }


}
