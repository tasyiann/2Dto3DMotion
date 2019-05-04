using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Text;

/* Extension for transform. */
public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
}




public class Model3D
{


    public class MJoint
    {
        public static Vector3[] GlobalAxesOrder = getGlobalAxesOrder();
        public Transform Transform;
        public EnumJoint Tag;
        public Vector3[] ActualAxesDirection;
        public UcyAxes[] ActualAxesOrder;


        public MJoint(Transform transform, EnumJoint enumJoint, Vector3[] axes = null)
        {
            Transform = transform;
            Tag = enumJoint;
            ActualAxesDirection = findAxesOrder();
        }

        public UcyAxes getActualAxis(UcyAxes abstractAxis)
        {
            return ActualAxesOrder[(int)abstractAxis];
        }

        public Vector3 GetTransformDirection(UcyAxes direction)
        {
            return ActualAxesDirection[(int)ActualAxesOrder[(int)direction]];
                //Transform.TransformDirection(GlobalAxesOrder[(int)ActualAxesOrder[(int)direction]]);
        }

        private Vector3[] findAxesOrder()
        {
            Array AxesTypes = Enum.GetValues(typeof(UcyAxes));
            Vector3[] axesOrder = new Vector3[AxesTypes.Length];
            ActualAxesOrder = new UcyAxes[AxesTypes.Length]; // <<




            foreach (UcyAxes axis in AxesTypes)
            {
                Vector3 direction = Transform.TransformDirection(GlobalAxesOrder[(int)axis]).normalized;
                float minDist = float.MaxValue;
                int minIndex = 0;
                for (int i = 0; i < AxesTypes.Length; i++)
                {
                    Vector3 globalAxis = GlobalAxesOrder[i];
                    float dist = Mathf.Abs(Vector3.Angle(direction, globalAxis));
                    if (dist < minDist)
                    {
                        minIndex = i;
                        minDist = dist;
                    }
                }
                ActualAxesOrder[minIndex] = axis;
                //axesOrder[minIndex] = direction;
            }


            foreach (UcyAxes axis in AxesTypes)
            {
                Vector3 direction = Transform.TransformDirection(GlobalAxesOrder[(int)ActualAxesOrder[(int)axis]]).normalized;
                axesOrder[(int)axis] = direction;
            }

            return axesOrder;
        }

        private static Vector3[] getGlobalAxesOrder()
        {
            Array AxesTypes = Enum.GetValues(typeof(UcyAxes));
            Vector3[] globalAxes = new Vector3[AxesTypes.Length];
            // Save global axes, in the order of UcyAxes enumeration.
            foreach (UcyAxes axis in AxesTypes)
            {
                Vector3 direction = Vector3.zero;
                switch (axis)
                {
                    case UcyAxes.UP:
                        direction = Vector3.up;
                        break;
                    case UcyAxes.RIGHT:
                        direction = Vector3.right;
                        break;
                    case UcyAxes.FWD:
                        direction = Vector3.forward;
                        break;
                    case UcyAxes.DOWN:
                        direction = Vector3.down;
                        break;
                    case UcyAxes.LEFT:
                        direction = Vector3.left;
                        break;
                    case UcyAxes.BACK:
                        direction = Vector3.back;
                        break;
                }
                Assert.IsTrue(direction != Vector3.zero);
                globalAxes[(int)axis] = direction;
            }
            return globalAxes;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("{0}:\nUP     -->{1} : {2}\nRIGHT-->{3} : {4}\nFWD   -->{5} : {6}\nDOWN-->{7} : {8}\nLEFT   -->{9} : {10}\nBACK  -->{11} : {12}\n",
                Transform.name,
                ActualAxesOrder[0].ToString(), ActualAxesDirection[(int)ActualAxesOrder[0]],
                ActualAxesOrder[1].ToString(), ActualAxesDirection[(int)ActualAxesOrder[1]],
                ActualAxesOrder[2].ToString(), ActualAxesDirection[(int)ActualAxesOrder[2]],
                ActualAxesOrder[3].ToString(), ActualAxesDirection[(int)ActualAxesOrder[3]],
                ActualAxesOrder[4].ToString(), ActualAxesDirection[(int)ActualAxesOrder[4]],
                ActualAxesOrder[5].ToString(), ActualAxesDirection[(int)ActualAxesOrder[5]]);

            return s.ToString();
        }
    }


    Transform model;
    public List<MJoint> Joints;
    GameObject hips;
    private float angle;
    private float timeCount = 0.0f;

    public enum UcyAxes
    {
        UP, RIGHT, FWD, DOWN, LEFT, BACK
    }
   

    public Model3D(Transform model3d)
    {
        model = model3d;
        Joints = setJoints();
        hips = GameObject.Find(model.name + "/Hips");
    }

    public List<Transform> getJointsAsTransforms()
    {
        List<Transform> list = new List<Transform>();
        foreach (MJoint j in Joints)
        {
            list.Add(j.Transform);
        }
        return list;
    }

    public static void correctlyNameJoints(Transform model, string name = "")
    {
        Transform result;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            if ((int)val == (int)EnumJoint.Spine1)
            {
                result = model.FindDeepChild(name + "Neck");
                result.name = "Neck";
            }
            else
            {
                result = model.FindDeepChild(name + val.ToString());
                result.name = val.ToString();
            }
        }
        result = model.FindDeepChild(name + "Hips");
        result.name = "Hips";
    }

    public List<MJoint> setJoints()
    {
        Debug.Log("Getting transforms from model...");
        List<MJoint> list = new List<MJoint>();
        foreach (EnumJoint val in Enum.GetValues(typeof(EnumJoint)))
        {

            // Find the joint.
            Transform transform;
            if ((int)val == (int)EnumJoint.Spine1)
                transform = model.FindDeepChild("Neck");
            else
                transform = model.FindDeepChild(val.ToString());
            if (!transform)
            {
                Debug.Log("Joint not found :" + val.ToString());
            }
            // Add it to the list.
            list.Add(new MJoint(transform, val));
        }
        // Print out the list
        string s = "";
        foreach (MJoint joint in list)
        {
            s += joint.Transform.name + " ";
        }
        Debug.Log(s);
        return list;
    }

    private Vector3[] ybotDefaultAxes(EnumJoint joint)
    {
        return new Vector3[]
        {
            Vector3.up, Vector3.right, Vector3.forward, Vector3.down, Vector3.left, Vector3.back
        };
    }

    private Vector3[] femaleDefaultAXes(EnumJoint joint)
    {
        Vector3[] axes = null;
        switch (joint)
        {
            case EnumJoint.Head:
                
                break;
            case EnumJoint.LeftArm:
                break;
            case EnumJoint.LeftFoot:
                break;
            case EnumJoint.LeftForeArm:
                break;
            case EnumJoint.LeftHand:
                break;
            case EnumJoint.LeftLeg:
                break;
            case EnumJoint.LeftUpLeg:
                break;
            case EnumJoint.RightArm:
                break;
            case EnumJoint.RightFoot:
                break;
            case EnumJoint.RightForeArm:
                break;
            case EnumJoint.RightHand:
                break;
            case EnumJoint.RightLeg:
                break;
            case EnumJoint.RightUpLeg:
                break;
            case EnumJoint.Spine1:
                break;
        }
        return axes;
    }

    private static Quaternion getRotation(Quaternion qX, Quaternion qY, Quaternion qZ)
    {
        return Quaternion.Euler(new Vector3(qX.eulerAngles.x, qY.eulerAngles.y, qZ.eulerAngles.z));
    }

    /* Try modifying the rotations, using LookRotation function.
     * https://gamedev.stackexchange.com/questions/136712/locking-a-rotation-on-an-axisyaw-pitch-roll-based-on-a-parental-transform/136743#136743
       https://gamedev.stackexchange.com/questions/139515/lookrotation-make-x-axis-face-the-target-instead-of-z
     */
    public void moveSkeleton(Vector3[] newJointsPositions)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);  // Calculate Raw Rotations.
        for (int i = 0; i < rotations.Length; i++)                                // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            Joints[i].Transform.rotation = rotations[i];
        }
    }
    

    public void moveSkeleton_OneEuroFilter(Vector3[] newJointsPositions, OneEuroFilter<Quaternion>[] rotationFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);         // Calculate Raw Rotations.
        hips.transform.rotation = rotationFilterHips.Filter(hips.transform.rotation);               // Set Hips Rotation.

        for (int i = 0; i < rotations.Length; i++)                           // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            Joints[i].Transform.rotation = rotationFiltersJoints[i].Filter(rotations[i]);
        }
    }

    public void moveSkeleton_IK_POSITIONS(Vector3[] newJointsPositions, OneEuroFilter<Vector3>[] positionsFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        hips.transform.rotation = rotationFilterHips.Filter(hips.transform.rotation);    // Set Hips Rotation.
        for (int i = 0; i < Joints.Count; i++)                                               // Set positions in joints.
        {
            Joints[i].Transform.position = positionsFiltersJoints[i].Filter(newJointsPositions[i] + hips.transform.position);
        }
    }

    public void moveSkeletonLERP(Vector3[] newJointsPositions)
    {

        /* Save current rotations. */
        Quaternion curr_hipsRot = hips.transform.rotation;
        List<Quaternion> curr_jointsRot = new List<Quaternion>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            curr_jointsRot.Add(Joints[(int)val].Transform.rotation);
        }

        /* Set rotations with LERP. */
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);              // Calculate Raw Rotations.
        hips.transform.rotation = Quaternion.Lerp(curr_hipsRot, hips.transform.rotation, timeCount);     // Set Hips Rotation with LEPR.
        for (int i = 0; i < rotations.Length; i++)                                                       // Set Rotations in joints with LERP.
        {
            if (rotations[i] == Quaternion.identity)                                                     // Skip, if rotation is identity.
                continue;
            Joints[i].Transform.rotation = Quaternion.Lerp(curr_jointsRot[i], rotations[i], timeCount);
        }

        /* Keep track of time, because of Lerp. */
        timeCount = timeCount + Time.deltaTime;
        timeCount = Mathf.Clamp01(timeCount);
        if (timeCount == 1) timeCount = 0;
    }



    public Quaternion XLookRotation(Vector3 right, Vector3 up)
    {
        // X becomes FWD
        // Z becomes LEFT
        // Y remains UP
        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }

    public Quaternion YLookRotation(Vector3 up, Vector3 upwards)
    {
        // Y becomes FWD
        // Z becomes DOWN
        // X remains RIGHT
        Quaternion UpToForward = Quaternion.Euler(90f, 0f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(up, upwards);

        return forwardToTarget * UpToForward;
    }

    public Quaternion GenericLookRotation(Vector3 targetDirection, Vector3 upwards, MJoint joint, UcyAxes abstractAxis)
    {
        UcyAxes actualAxis = joint.getActualAxis(abstractAxis);
        Vector3 actualAxisDirection = joint.ActualAxesDirection[(int)actualAxis];
        // Find the rotation that turns ActualAxisDirection into fwd.
        Quaternion AbstractAxis_To_Forward = Quaternion.FromToRotation(actualAxisDirection, Vector3.forward);
        Quaternion forwardToTarget = Quaternion.LookRotation(targetDirection, upwards);
        return forwardToTarget * AbstractAxis_To_Forward;
    }

    public Quaternion[] calculateRawRotations(Vector3[] newJoints, Transform hips)
    {
        // The order matters. The rotations should be applied first to the parent and then to the child.
        Vector3 root = (newJoints[8] + newJoints[11]) / 2;
        Quaternion[] rotations = new Quaternion[14];
        hips.rotation = XLookRotation(newJoints[8] - newJoints[11], -(root - newJoints[1]));
        /* HEAD           */
        rotations[0] = Quaternion.identity; // end point.
        /* NECK           */
        rotations[1] = GenericLookRotation(newJoints[0] - newJoints[1], Joints[1].GetTransformDirection(UcyAxes.BACK), Joints[1], UcyAxes.UP);
        /* RIGHT_ARM      */
        rotations[2] = GenericLookRotation(newJoints[3] - newJoints[2], Joints[2].GetTransformDirection(UcyAxes.UP), Joints[2], UcyAxes.RIGHT);
        /* RIGHT_FORE_ARM */
        rotations[3] = GenericLookRotation(newJoints[4] - newJoints[3], Joints[3].GetTransformDirection(UcyAxes.UP), Joints[3], UcyAxes.RIGHT);
        /* RIGHT_HAND     */
        rotations[4] = Quaternion.identity; // end point.
        /* LEFT_ARM       */
        rotations[5] = GenericLookRotation(newJoints[5] - newJoints[6], Joints[5].GetTransformDirection(UcyAxes.UP), Joints[5], UcyAxes.RIGHT);
        /* LEFT_FORE_ARM  */
        rotations[6] = GenericLookRotation(newJoints[6] - newJoints[7], Joints[6].GetTransformDirection(UcyAxes.UP), Joints[6], UcyAxes.RIGHT);
        /* LEFT_HAND      */
        rotations[7] = Quaternion.identity; // end point.
        /* RIGHT_UP_LEG   */
        rotations[8] = GenericLookRotation(newJoints[8] - newJoints[9], Joints[8].GetTransformDirection(UcyAxes.BACK), Joints[8], UcyAxes.UP);
        /* RIGHT_LEG      */
        rotations[9] = GenericLookRotation(newJoints[9] - newJoints[10], Joints[9].GetTransformDirection(UcyAxes.BACK), Joints[9], UcyAxes.UP);
        /* RIGHT_FOOT     */
        rotations[10] = Quaternion.identity; // end point.
        /* LEFT_UPLEG     */
        rotations[11] = GenericLookRotation(newJoints[11] - newJoints[12], Joints[11].GetTransformDirection(UcyAxes.BACK), Joints[11], UcyAxes.UP);
        /* LEFT_LEG       */
        rotations[12] = GenericLookRotation(newJoints[12] - newJoints[13], Joints[12].GetTransformDirection(UcyAxes.BACK), Joints[12], UcyAxes.UP);
        /* LEFT_FOOT      */
        rotations[13] = Quaternion.identity; // end point.

        return rotations;
    }

    public void DebugAxes()
    {
        string s = "";
        foreach (MJoint j in Joints)
        {
            s += j.ToString();
        }
        Debug.Log(s);
    }


}
