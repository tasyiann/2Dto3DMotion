using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UcyModeling
{
    public class UcyConfigureModel : MonoBehaviour
    {
        public string prefixName;
        private void Awake()
        {
            // Rename the joints so they are compatible to our methods.
            Model3D.correctlyNameJoints(transform,prefixName);
        }
    }



}


