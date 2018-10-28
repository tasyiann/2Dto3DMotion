using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.IO;


/* Purpose of this class, is to Visualize the rotations (ONLY the rotations), of our Database. */
public class Rotations3D : Base
{
    public Transform model;
    private Model3DBeta m3d;
    public Material material;
    private int RotationIndex = 0;
    public Text textInfo;
    private GLDraw gL;
    private Vector3[] rotations;
    int RotationFileIndex = 0;

    private void updateText()
    {
        string s = "";
        s += "Rotations File: " +RotationFileIndex+ "/" + (base_rotationFiles.Count-1)+"\n";
        s += "Rotation (frame): " + RotationIndex + "/" + (base_rotationFiles[RotationFileIndex].Count-1)+ "\n";
        textInfo.text = s;
    }


    private void Start()
    {
        gL = new GLDraw(material);
        m3d = new Model3DBeta(model);
        rotations = new Vector3[14]; // Someday, fix the magic numbers pls
    }

    void Update()
    {

        if (Input.GetKey("w"))
        {
            RotationIndex ++;
            updateText();
            updateRotationsToShow();
            m3d.moveSkeleton(rotations);
        }

    }



    private void updateRotationsToShow()
    {
        if (RotationIndex >= base_rotationFiles[RotationFileIndex].Count)
        {
            if (base_rotationFiles.Count - 1 > RotationFileIndex)
            {
                RotationIndex -= base_rotationFiles[RotationFileIndex].Count;
                RotationFileIndex++;
            }
            else
            {
                RotationIndex = 0;
                RotationFileIndex = 0;
                return;
            }
        }
        rotations = base_rotationFiles[RotationFileIndex][RotationIndex].getComparableRotations().ToArray();
    }


}
