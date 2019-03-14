using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record3Ddistance : MonoBehaviour
{
    // animator.stabilizeFeet

    public Transform A;
    public Transform B;
    public string AstringInFrontOfBone="";
    public string BstringInFrontOfBone="";
    public float Height;
    public float scaleFactor = 1.17f;

    public float dist = 0;
    public float iterations = 0;
    public bool record=false;


    private List<Transform> Ajoints;
    private List<Transform> Bjoints;
    
   

    private Animator animator;

    private void Start()
    {



        iterations = 0;
        animator = A.GetComponent<Animator>();
        animationClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        initializeJoints();
        Height = getHeightOfFigure();



        // Initialise
        for (int i = 0; i < Ajoints.Count; i++)
        {
            joints[i] = new List<float>();
        }
    }


    private void Update()
    {
        if(record)
            calculateDist();
    }


    private float getHeightOfFigure()
    {

        Vector3 heighestPoint = Ajoints[(int)EnumJoint.Head].transform.position;
        Vector3 lowestPoint = (Ajoints[(int)EnumJoint.RightFoot].transform.position+Ajoints[(int)EnumJoint.LeftFoot].transform.position)/2f;
        return Vector3.Distance(heighestPoint,lowestPoint);
    }

    public void initializeJoints()
    {
         dist = 0;
         iterations = 0;
         Ajoints = Model3D.setJoints(A, AstringInFrontOfBone);
         Bjoints = Model3D.setJoints(B, BstringInFrontOfBone);
    }

    private void OnApplicationQuit()
    {
        dist = dist / iterations;
        Debug.Log("Height:"+Height);
        Debug.Log("3D Distance is: "+ dist);

        // Debug 1.
        System.IO.File.WriteAllText("AveragedistPerFrame.txt", distAtFrame_Str);

        // Debug 2.
        string str_joint = "";
        foreach (List<float> list in joints)
        {
            foreach (float frameValue in list)
            {
                str_joint += (frameValue+" ");
            }
            str_joint += "\n"; // change line, go to next joint. (total 14 lines).
        }
        System.IO.File.WriteAllText("All values.txt", str_joint);
    }

    private float[] averagePerJoint = new float[14];
    private List<float>[] joints = new List<float>[14];
    private string distAtFrame_Str = "";

    public float calculateDist()
    {
        float distAtFrame = 0;
        for(int i=0; i<Ajoints.Count; i++)
        {
            if (i == (int)EnumJoint.Spine1)
                continue;

            float distAtJoint = Vector3.Distance(Ajoints[i].transform.position * scaleFactor, Bjoints[i].transform.position * scaleFactor);
            distAtFrame += distAtJoint;             // Debug Info
            averagePerJoint[i] += distAtJoint;      // Debug Info
            joints[i].Add(distAtJoint);             // Debug Info
        }

        // Print distAtFrame. 
        distAtFrame_Str += ( (distAtFrame/ (Ajoints.Count-1)) + "\n"); // Save Average dist At Frame. // MINUS ONE because SPINE is out.
        dist += (distAtFrame / (Ajoints.Count-1));  // Average on that frame, to the total.
        iterations++;                           // Frame counter
        
        return dist; // dist til that frame.
    }

    private AnimationClip animationClip;
    public void wakeUpAnimation()
    {
        animator.Play(animationClip.name, 0, 0);
        Debug.Log("Animation" + animationClip.name + " has been started.");
    }

}
