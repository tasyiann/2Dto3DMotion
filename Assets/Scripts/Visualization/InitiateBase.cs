using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DataBaseParametersReader),typeof(AlgorithmsParametersReader))]
public class InitiateBase : MonoBehaviour {

    Thread thread;

    void Start () {         // Start function. NOT awake. Awake is used in the DataBaseParametersReader class.
        InitializeBase();   // Then initialise database.
    }


    private void Update()
    {
        if (Base.areThreadsDone())
        {
            Debug.Log("Initialization is done!");
            // Remove this later
            // Base.writeAnimation3DPointsInCSVFile();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void InitializeBase()
    {
        Base.Threads_StartInit();
    }


}
