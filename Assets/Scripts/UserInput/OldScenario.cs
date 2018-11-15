using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class OldScenario : MonoBehaviour {

    public Text textUrl;
    public string url;

    public void OpenExplorer()
    {
        url = EditorUtility.OpenFolderPanel("Choose scenario", "", "");
        textUrl.text = url;
        Base.base_CurrentDir = url;
    }


    public void VIEW()
    {
        string scenarioFileName = findScenarioFile();
        if (scenarioFileName == null) {
            Debug.Log("Scenario File not found!");
            return;
        }
        Scenario scenario = (Scenario)DataParsing.readBinaryfile(scenarioFileName);
        scenario.SetInputDir(url);
        Base.SetCurrentScenario(scenario); // <<<

        if (Base.sc == null)
        {
            Debug.Log("scenario is null!");
        }
        else
        {
            Debug.Log("Hello");
        }

        DataParsing.CalculateEstimation();
        UnityEngine.SceneManagement.SceneManager.LoadScene(7); // Estimation Scene
    }

    private string findScenarioFile()
    {
        List<List<BvhProjection>> listClusters = new List<List<BvhProjection>>();
        string[] fileEntries = Directory.GetFiles(url);

        foreach (string fileName in fileEntries)
        {
            if (fileName.EndsWith(".sc"))
            {
                Debug.Log("Getting scenario from file: " + fileName);
                return fileName;
            }
        }
        return null;
    }
}
