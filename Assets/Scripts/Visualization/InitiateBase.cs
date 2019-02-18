using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiateBase : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Scaling.setNewScalingStandard();
        Base.initialize();
        new BvhExport("TemplateBVH\\template.bvh", "Databases\\Big-Database\\Rotations\\4", "try.bvh");
        // ConfigureLIMBS.runTests();
    }

}
