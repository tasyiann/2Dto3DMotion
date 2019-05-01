using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugAllAxes : MonoBehaviour
{
    public Transform[] gameObjects;
    public string prefixName="";
    public bool AutomaticSelection;


    void Start()
    {
        if (AutomaticSelection)
        {
            gameObjects = Model3D.setJoints(transform, prefixName).ToArray();
        }


        foreach (Transform g in gameObjects)
        {
            if(g.GetComponent<DisplayAxes>() == null)
                g.gameObject.AddComponent<DisplayAxes>();
        }
    }

}
