using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiateBase : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Scaling.setNewScalingStandard();
        Base.initialize();
        // ConfigureLIMBS.runTests();
    }

}
