using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual_MultiFigures : MonoBehaviour
{
    public DataInFrame script;                    // Use this script to get data.
    public List<GameObject> modelsRaw;            // List of the models.
    public List<GameObject> modelsFiltered;       // List of the models.
    public List<OPPose> figures;
    public List<Visual_ApplyOn3DModel> components_toSetData_Raw;
    public List<Visual_ApplyOn3DModel> components_toSetData_Filtered;
    public Camera cameraRaw;
    public Camera cameraFiltered;


    void Start()
    {
        // Make sure, the script component on these models,
        // have the multi figures feature enabled.
        setMultiFiguresEnable(modelsRaw);
        setMultiFiguresEnable(modelsFiltered);
        components_toSetData_Raw = getComponentsToSetData(modelsRaw);
        components_toSetData_Filtered = getComponentsToSetData(modelsFiltered);
    }

    private void setMultiFiguresEnable(List<GameObject> models)
    {
        foreach (GameObject model in models)
        {
            Visual_ApplyOn3DModel component = model.GetComponent<Visual_ApplyOn3DModel>();
            component.multiFigures = true;
        }
    }

    private void setActiveModelsOnScene(List<GameObject> models)
    {
        for (int i = 0; i < modelsRaw.Count; i++)
        {
            if (i < figures.Count)
            {
                if(!models[i].activeSelf)
                    models[i].SetActive(true);
            }
            else
            {
                if (models[i].activeSelf)
                    models[i].SetActive(false);
            }  
        }
    }


    private void updateModelsOnScene(List<Visual_ApplyOn3DModel> components)
    {
        for(int i=0; i<figures.Count; i++)
        {
            if (i >= components.Count)
                break;

            components[i].selectedPoseToDebug = figures[i];
        }
    }

    private List<Visual_ApplyOn3DModel> getComponentsToSetData(List<GameObject> models)
    {
        List<Visual_ApplyOn3DModel> list = new List<Visual_ApplyOn3DModel>();
        foreach(GameObject model in models)
        {
            list.Add(model.GetComponent<Visual_ApplyOn3DModel>());
        }
        return list;
    }

    
    private void repositionCameraOnX(Camera camera, List<GameObject> models)
    {
        Vector3 endingPosition = camera.transform.position;

        if (models == null)
            return;

        if (figures.Count == 1)
        {
            endingPosition = camera.transform.position = new Vector3(models[0].transform.position.x, camera.transform.position.y, camera.transform.position.z);
        }
        else if(figures.Count > 1)
        {
            Vector3 middleOfModels = (models[0].transform.position + models[models.Count - 1].transform.position) / 2;
            endingPosition = new Vector3(middleOfModels.x, camera.transform.position.y, camera.transform.position.z);
        }

        camera.transform.position = endingPosition;
    }


 
    void Update()
    {
        // Update frame.
        figures = script.allPoses;
        setActiveModelsOnScene(modelsRaw);
        setActiveModelsOnScene(modelsFiltered);
        updateModelsOnScene(components_toSetData_Raw);
        updateModelsOnScene(components_toSetData_Filtered);
        repositionCameraOnX(cameraRaw, modelsRaw);
        repositionCameraOnX(cameraFiltered, modelsFiltered);
    }



}
