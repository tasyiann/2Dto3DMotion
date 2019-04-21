using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Tomis.UnityEditor.Utilities
{
    [CustomPropertyDrawer(typeof(FileSelectAttribute))]
    class FileSelectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fileSelectAttribute = attribute as FileSelectAttribute;
            if(fileSelectAttribute == null)
                throw new NullReferenceException("Why is fileSelectAttribute null");
            


            var openAtPath = "";

            if (!String.IsNullOrEmpty(fileSelectAttribute.OpenAtPath))
            {
                openAtPath = fileSelectAttribute.AssetRelativePath ?
                    Application.dataPath + @"\" + fileSelectAttribute.OpenAtPath : 
                    fileSelectAttribute.OpenAtPath;
                openAtPath = Path.GetFullPath(openAtPath);
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                var selectedFilePath = property.stringValue;
                     
                if (fileSelectAttribute.DisplayWarningWhenNotSelected && String.IsNullOrEmpty(selectedFilePath))
                {
                    EditorGUILayout.HelpBox("!WARNING: No file selected!", MessageType.Warning);
                }
                
                var buttonName = fileSelectAttribute.ButtonName ?? "Select " +
                                 (fileSelectAttribute.SelectMode == FileSelectionMode.Folder ? "folder" : "file");
                
                if (GUI.Button(new Rect(position.x, position.y, position.width / 2, position.height),
                    new GUIContent
                    {
                        text = buttonName,
                        tooltip = fileSelectAttribute.Tooltip
                    }))
                {
                    if (fileSelectAttribute.SelectMode == FileSelectionMode.Folder)
                    {
                        selectedFilePath = EditorUtility.OpenFolderPanel(buttonName, 
                            openAtPath ?? "", fileSelectAttribute.FileExtensions);
                    }
                    else
                    {
                        selectedFilePath = EditorUtility.OpenFilePanel(buttonName, 
                            openAtPath ?? "", fileSelectAttribute.FileExtensions);
                    }
                    property.stringValue = selectedFilePath;
                }
                Rect newRect = new Rect(position);
                float offset = 0.3f;
                newRect.x = newRect.x + newRect.width / 2;
                newRect.width = newRect.width / 2 + offset;
                
                GUI.Label(newRect, "Selected" + ((fileSelectAttribute.SelectMode == FileSelectionMode.Folder) ? " Folder:" : " File: ")
                 + Path.GetFileName(selectedFilePath) ?? "");
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Property must be string");
            }
        }
    }
}

