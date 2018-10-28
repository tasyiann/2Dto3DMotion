using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3Drotations : MonoBehaviour {

	// Use this for initialization
	void Start () {


        float x1 = PrevFrameWindow3D.DistanceRotations( new Vector3(0, 25, 60), new Vector3(25, 45, 180) );
        float x2 = PrevFrameWindow3D.DistanceRotations( new Vector3(60, 30, 120), new Vector3(0,0,0) );
        float x3 = PrevFrameWindow3D.DistanceRotations(new Vector3(0, 150, 280), new Vector3(90,120,280) );
        Debug.Log("Frame1:\n" +
            "character A (joint 1: [ 0 25  60]  joint 2: [60 30 120]   joint 3: [0 150 280])\n" +
            "character B(joint 1: [25 45 180]  joint 2: [0  0   0]   joint 3: [90 120 280]))\n" +
            "Distance of joint 1: "+x1+" "+x2+" "+x3);
        
        float x4 = PrevFrameWindow3D.DistanceRotations(new Vector3(5, 30, 65), new Vector3(45, 45, 45));
        float x5 = PrevFrameWindow3D.DistanceRotations(new Vector3(60, 30, 120), new Vector3(60, 30, 120));
        float x6 = PrevFrameWindow3D.DistanceRotations(new Vector3(0, 150, 280), new Vector3(40, 25, 25));
        Debug.Log("Frame 2:\n" +
            "character A (joint 1: [ 5 30 65]  joint 2: [60 30 120]   joint 3: [0 150 280])\n" +
            "character B(joint 1: [45 45 45]  joint 2: [60 30 120]   joint 3: [40 25 25])\n" +
            "Distances: " + x4 + " " + x5 + " " + x6);

        float x7 = PrevFrameWindow3D.DistanceRotations(new Vector3(0,25,60), new Vector3(0,25,60));
        float x8 = PrevFrameWindow3D.DistanceRotations(new Vector3(60, 30, 120), new Vector3(60, 30, 120));
        float x9 = PrevFrameWindow3D.DistanceRotations(new Vector3(0,150,280), new Vector3(0,150,280));
        Debug.Log("Frame 3:\n"+
            "character A(joint 1: [0 25  60]  joint 2: [60 30 120]   joint 3: [0 150 280])\n"+
            "character B(joint 1: [0 25  60]  joint 2: [60 30 120]   joint 3: [0 150 280])\n"+
            "Distances:"+x7+" "+x8+" "+x9);

    }
	

}
