using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Create3DFigure 
{
    private static string[] limbsNames = Enum.GetNames(typeof(EnumBONES));
    private static int numLimbs = limbsNames.Length;
    private static int counter=0;

    private GameObject figureGameObject;

    // Material used for the connecting lines
    public Material lineMat;
    public float radius = 0.05f;
    public Mesh cylinderMesh;
    List<GameObject> limbsGameObjects = new List<GameObject>();

    private int[,] defaultJP = new int[13, 2] { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 }, { 1, 5 }, { 5, 6 }, { 6, 7 }, { 1, 8 }, { 8, 9 }, { 9, 10 }, { 1, 11 }, { 11, 12 }, { 12, 13 } };

    public Create3DFigure(Mesh cyclinder_Mesh, Material mat)
    {
        lineMat = mat;
        cylinderMesh = cyclinder_Mesh;
        figureGameObject = new GameObject();
        figureGameObject.name = "Figure"+counter;
        initializeLimbs();
        counter++;
    }



    public void updateLimbs(Vector3 [] joints)
    {
        for(int i=0; i<limbsGameObjects.Count; i++)
        {
            updateLimbPosition(i, joints[defaultJP[i, 0]], joints[defaultJP[i, 1]]);
        }
    }


    private void updateLimbPosition(int index, Vector3 v1, Vector3 v2)
    {
        float cylinderDistance = 0.5f * Vector3.Distance(v1, v2);
        limbsGameObjects[index].transform.localScale = new Vector3(limbsGameObjects[index].transform.localScale.x, cylinderDistance, limbsGameObjects[index].transform.localScale.z);
        limbsGameObjects[index].transform.LookAt(v1, Vector3.up);
        limbsGameObjects[index].transform.rotation *= Quaternion.Euler(90, 0, 0);
    }



    public void initializeLimbs()
    {
        
        for (int i = 0; i < numLimbs; i++)
            createLimb(limbsNames[i]);
    }


    private void createLimb(string limbName)
    {
        GameObject limb = new GameObject();
        limb.name = limbName;
        limb.transform.parent = figureGameObject.transform;


        GameObject cylinder = new GameObject();
        cylinder.transform.parent = limb.transform;
        cylinder.transform.localPosition = new Vector3(0f, 1f, 0f);
        cylinder.transform.localScale = new Vector3(radius, 1f, radius);
        MeshFilter ringMesh = cylinder.AddComponent<MeshFilter>();
        ringMesh.mesh = cylinderMesh;
        MeshRenderer ringRenderer = cylinder.AddComponent<MeshRenderer>();
        ringRenderer.material = lineMat;

        limbsGameObjects.Add(limb);
    }
}
