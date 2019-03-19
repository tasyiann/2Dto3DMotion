using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
public class UserInput : MonoBehaviour {

    public Text textUrl;
    public string url;


    public void OpenExplorer()
    {
        url = EditorUtility.OpenFolderPanel("Choose input", "", "");
        textUrl.text = url;
    }

    public void GO_OFFLINE()
    {
        GenerateNewScenario();
        DataParsing.OFFLINE_Pipeline();
        UnityEngine.SceneManagement.SceneManager.LoadScene(7); // Estimation Scene
    }

    public void GO_REALTIME()
    {
        GenerateNewScenario();
        UnityEngine.SceneManagement.SceneManager.LoadScene(12); // Estimation Scene
    }


    private void GenerateNewScenario()
    {
        Base.SetCurrentScenario(new Scenario(url));
    }

}
