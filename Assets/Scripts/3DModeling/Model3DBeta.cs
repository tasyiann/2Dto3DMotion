using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;



public class Model3DBeta
{

    Transform model;
    readonly Dictionary<string,Transform> Joints;

    public Model3DBeta(Transform model3d)
    {
        model = model3d;
        Joints = SetJoints();
    }

    private Dictionary<string, Transform> SetJoints()
    {
        Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
        foreach (var val in Enum.GetValues(typeof(EnumModel3DJoints)))
        {
            Transform result = model.FindDeepChild(val.ToString());
            if (!result)
                Debug.Log("THE JOINT IS NULL >>>>>>>>>>>>>>>>>>>>>>>>>" + val.ToString());
            dictionary.Add(val.ToString(),result);
        }
        return dictionary;
    }




    private Quaternion getRotation(Quaternion qX, Quaternion qY, Quaternion qZ)
    {
        return Quaternion.Euler(new Vector3(qX.eulerAngles.x, qY.eulerAngles.y, qZ.eulerAngles.z));
    }


    Quaternion XLookRotation(Vector3 right, Vector3 up)
    {

        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }




    Quaternion YLookRotation(Vector3 up, Vector3 upwards)
    {

        Quaternion UpToForward = Quaternion.Euler(90f, 0f, 0f); // Correct, it's 90 degrees to make the positive y, into positive forward.
        Quaternion forwardToTarget = Quaternion.LookRotation(up, upwards);

        return forwardToTarget * UpToForward;
    }





    /* Try modifying the rotations, using LookRotation function.
     * https://gamedev.stackexchange.com/questions/136712/locking-a-rotation-on-an-axisyaw-pitch-roll-based-on-a-parental-transform/136743#136743
       https://gamedev.stackexchange.com/questions/139515/lookrotation-make-x-axis-face-the-target-instead-of-z
     */
    public void moveSkeleton(Vector3[] rotations, Vector3[] positions=null)
    {
        if (positions == null)
        {
            // Keep hips at initial rotation and position.
        }
        else
        {
            // Hips: Works fine!
            Vector3 root = (positions[8] + positions[11]) / 2;
            Quaternion hips_x = YLookRotation((positions[1] - root), Vector3.up);
            Quaternion hips_yz = XLookRotation(positions[8] - positions[11], Vector3.up);
            Joints["Hips"].rotation = getRotation(hips_x, hips_yz, hips_yz);
        }
 
        Joints["RightArm"].rotation = XLookRotation(-rotations[2], Vector3.up);
        Joints["RightForeArm"].rotation = XLookRotation(-rotations[3], Vector3.up);
        Joints["RightHand"].rotation = XLookRotation(-rotations[4], Vector3.up);

        Joints["LeftArm"].rotation = XLookRotation(-rotations[5], Vector3.up);
        Joints["LeftForeArm"].rotation = XLookRotation(-rotations[6], Vector3.up);
        Joints["LeftHand"].rotation = XLookRotation(-rotations[7], Vector3.up);

 

    }









    

}
