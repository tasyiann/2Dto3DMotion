using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Model3DObject
{

    public GameObject gameObject;
    Model3D m3d;

    public Model3DObject(GameObject go, Vector3[] joints=null)
    {
        gameObject = go;
        m3d = new Model3D(go.transform);
        if(joints!=null)
            m3d.moveSkeleton(joints);
    }

    public void setColor(Color color)
    {
        Renderer[] renderers = gameObject.transform.GetComponentsInChildren<Renderer>();//FindObjectsOfType<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = color;
        }
    }


    public void setVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }


    public void setJoints(Vector3[] joints)
    {
        m3d.moveSkeleton(joints);
    }
}