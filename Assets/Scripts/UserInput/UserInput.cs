using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
public class UserInput : MonoBehaviour {

    public Text textUrl;
    public string url;
    public Dropdown kNNAlg_dropdown;
    public Dropdown EstAlg_dropdown;
    public InputField k_input;
    public InputField m_input;
    public enum EnumAlgorithms                  // Select Neighbours Algorithm.
    { JBJEuclidean }
    public enum EnumNN                          // Select Estimation Algorithm.
    { PrevFrame2D, PrevFrameWindow3D, Nearest }

    private void Awake()
    {
        kNNAlg_dropdown.ClearOptions();
        List<string> list = new List<string>();
        foreach(string s in Enum.GetNames(typeof(EnumAlgorithms))){
            list.Add(s);
        }
        kNNAlg_dropdown.AddOptions(list);

        EstAlg_dropdown.ClearOptions();
        list = new List<string>();
        foreach (string s in Enum.GetNames(typeof(EnumNN)))
        {
            list.Add(s);
        }
        EstAlg_dropdown.AddOptions(list);

    }



    public void OpenExplorer()
    {
        url = EditorUtility.OpenFolderPanel("Choose input", "", "");
        textUrl.text = url;
    }

    public void GO()
    {
        SavePreferences();
        DataParsing.Calculate3D();
        UnityEngine.SceneManagement.SceneManager.LoadScene(7); // Estimation Scene
    }

    private void SavePreferences()
    {
        AlgorithmSetNeighbours alg_setNeighbour=null;           
        switch (kNNAlg_dropdown.value)
        {
            case (int)EnumAlgorithms.JBJEuclidean: { alg_setNeighbour = new JBJEuclideanComparison(); break; }
            //case (int)EnumAlgorithms.None: { alg_setNeighbour = null; break; }
        }
        AlgorithmEstimation alg_estimation= new NearestEstimation();
        switch (EstAlg_dropdown.value)
        {
            case (int)EnumNN.PrevFrame2D: { alg_estimation = new PrevFrame2D(); break; }
            case (int)EnumNN.Nearest: { alg_estimation = new NearestEstimation();  break; }
            case (int)EnumNN.PrevFrameWindow3D: { alg_estimation = new PrevFrameWindow3D();  break; }
            //case (int)EnumNN.None: { alg_estimation = null;  break; }
        }
        Base.SetCurrentScenario(new Scenario(url,Int32.Parse(k_input.text), Int32.Parse(m_input.text),alg_estimation,alg_setNeighbour));
    }

}
