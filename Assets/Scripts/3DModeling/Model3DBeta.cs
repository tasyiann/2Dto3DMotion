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
                Debug.Log("Joint on model, not found! : " + val.ToString());

            dictionary.Add(val.ToString(),result);
        }
        return dictionary;
    }





    /* Try modifying the rotations, using LookRotation function.
     * https://gamedev.stackexchange.com/questions/136712/locking-a-rotation-on-an-axisyaw-pitch-roll-based-on-a-parental-transform/136743#136743
       https://gamedev.stackexchange.com/questions/139515/lookrotation-make-x-axis-face-the-target-instead-of-z
     */
    public void moveSkeleton(Vector3[] rotations, int choice)
    {
        setRotation(Joints["Hips"], rotations[0], choice);


        // Mporei na ginei me for loop,
        // iparxoun sto Base.base_order
       
        setRotation(Joints["RightArm"], rotations[8],choice);
        setRotation(Joints["RightForeArm"],rotations[9],choice);
        setRotation(Joints["RightHand"], rotations[10],choice);

        setRotation(Joints["LeftArm"], rotations[5], choice);
        setRotation(Joints["LeftForeArm"], rotations[6], choice);
        setRotation(Joints["LeftHand"], rotations[7], choice);

        setRotation(Joints["RightUpLeg"],rotations[14], choice);
        setRotation(Joints["RightLeg"], rotations[15], choice);
        setRotation(Joints["RightFoot"], rotations[16], choice);

        setRotation(Joints["LeftUpLeg"], rotations[11], choice);
        setRotation(Joints["LeftLeg"], rotations[12], choice);
        setRotation(Joints["LeftFoot"], rotations[13], choice);
        

    }
    // https://forums.adobe.com/thread/1475746
    // https://gamedev.stackexchange.com/questions/140579/euler-right-handed-to-quaternion-left-handed-conversion-in-unity
    // http://forums.cgsociety.org/t/converting-rotation-values-for-bvh/1166576/11
    // The x, y, and z angles represent a rotation z 
    // degrees around the z axis, x degrees around the x axis,
    // and y degrees around the y axis (in that order).
    void setRotation(Transform t, Vector3 rotation, int choice)
    {
        Debug.Log("Choice: " + choice);
        float x = rotation.x;
        float y = rotation.y;
        float z = rotation.z;
        Vector3 zz = Vector3.forward;
        Vector3 xx = Vector3.right;
        Vector3 yy = Vector3.up;

        switch (choice)
        {
            case 0: rot(t, x, xx, y, yy, z, zz);break;
            case 1: rot(t, x, xx, z, zz, y, yy); break;
            case 2: rot(t, z, zz, y, yy, x, xx); break;
            case 3: rot(t, z, zz, x, xx, y, yy); break;
            case 4: rot(t, x, xx, -y, yy, -z, zz); break;
            case 5: rot(t, x, xx, -z, zz, -y, yy); break;
            case 6: rot(t, -z, zz, -y, yy, x, xx); break;
            case 7: rot(t, -z, zz, x, xx, -y, yy); break;
            case 8: rot(t, y, yy, x, xx, z, zz); break;
            case 9: rot(t, y, yy, z, zz, z, xx); break;
            case 10: rot(t, -y, yy, x, xx, -z, zz); break;
            case 11: rot(t, -y, yy, -z, zz, x, xx); break;
            case 12: rot(t, -z, Vector3.forward, -x, Vector3.left, -y, Vector3.up); break;
            case 13: rot(t, -y, Vector3.up, -x, Vector3.left, -z, Vector3.forward); break;
            case 14: rot(t, -y, Vector3.up, x, Vector3.left, -z, Vector3.forward); break;
            case 15: rot(t, z, Vector3.forward, -x, Vector3.right, y, Vector3.up); break;
            case 16: rot(t, y, Vector3.up, -x, Vector3.right, z, Vector3.forward); break;
            case 17: rot(t, x, xx, y, yy, -z, zz); break;
            case 18: rot(t, z, zz, y, yy, x, xx); break;
            case 19: rot(t, x, xx, z, zz, y, yy); break;
        }
    }

    private void rot(Transform t, float a, Vector3 aa, float b, Vector3 bb, float c, Vector3 cc)
    {
        t.localRotation = Quaternion.AngleAxis(a, aa) * Quaternion.AngleAxis(b, bb) * Quaternion.AngleAxis(c, cc);
    }







    

}
