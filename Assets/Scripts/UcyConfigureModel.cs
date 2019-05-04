using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UcyModeling
{

    [CustomEditor(typeof(UcyConfigureModel))]
    public class UcyConfModelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UcyConfigureModel myscript = target as UcyConfigureModel;
            if (GUILayout.Button("Fix names"))
            {
                myscript.fixNames();
            }
        }
    }





    public class UcyConfigureModel : MonoBehaviour
    {
        public string prefixName;

        public void fixNames()
        {
            Model3D.correctlyNameJoints(transform, prefixName);
        }
    }



}


