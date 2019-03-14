using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiateBase : MonoBehaviour {


    public bool Clusters, Projections, Rotations;

	// Use this for initialization
	void Awake () {
        // Scaling.setNewScalingStandard();
        Base.initialize(Clusters,Projections,Rotations);
        //new BvhExport("TemplateBVH\\template.bvh", "Databases\\Big-Database\\Rotations\\4", "try.bvh");
        // ConfigureLIMBS.runTests();
    }

}
