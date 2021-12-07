using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugAllAxes : MonoBehaviour
{
    public Transform[] gameObjects;
    public bool AutomaticSelection;


    void Start()
    {
        if (AutomaticSelection)
        {
            Model3D m3d = new Model3D(transform);
            gameObjects = m3d.getJointsAsTransforms().ToArray();
        }
        /*
        foreach (Transform g in gameObjects)
        {
            DisplayAxes c = g.GetComponent<DisplayAxes>();
            if (c != null)
                Destroy(c);

        }
        */
        
        //foreach (Transform g in gameObjects)
        //{
        //    if (g.GetComponent<DisplayAxes>() == null)
        //        g.gameObject.AddComponent<DisplayAxes>();
        //}
        

    }

}
