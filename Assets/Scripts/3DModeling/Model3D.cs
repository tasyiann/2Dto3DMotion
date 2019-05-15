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
        public EnumModel3DJoints Tag;
        public Vector3[] ActualAxesDirection;
        public UcyAxes[] ActualAxesOrder;


        public MJoint(Transform transform, EnumModel3DJoints enumJoint, Vector3[] axes = null)
        {
            Transform = transform;
            Tag = enumJoint;
            ActualAxesDirection = findAxesOrder();
        }

        public UcyAxes getActualAxis(UcyAxes abstractAxis)
        {
            return ActualAxesOrder[(int)abstractAxis];
        }

        public Vector3 GetDirectionUsingAbstractAxis(UcyAxes direction)
        {
            return ActualAxesDirection[(int)getActualAxis(direction)];
        }

        private UcyAxes getCorrespondingActualAxis(UcyAxes axis)
        {
            Array AxesTypes = Enum.GetValues(typeof(UcyAxes));
            // TransformDirection : From local space to World Space.
            // direction is the direction in world space.
            Vector3 directionWorldSpace = Transform.TransformDirection(GlobalAxesOrder[(int)axis]).normalized;
            float minDist = float.MaxValue;
            UcyAxes minAxis = 0;
            for (int i = 0; i < AxesTypes.Length; i++)
            {
                Vector3 globalAxis = GlobalAxesOrder[i];
                float dist = Mathf.Abs(Vector3.Angle(directionWorldSpace, globalAxis));
                if (dist < minDist)
                {
                    minAxis = (UcyAxes)i;
                    minDist = dist;
                }
            }
            return minAxis;
        }


        private Vector3[] findAxesOrder()
        {
            Array AxesTypes = Enum.GetValues(typeof(UcyAxes));
            Vector3[] axesOrderDirection = new Vector3[AxesTypes.Length];
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
            }


            foreach (UcyAxes axis in AxesTypes)
            {
                UcyAxes actualAxis = ActualAxesOrder[(int)axis];
                Vector3 localDirection = GlobalAxesOrder[(int)actualAxis];
                Vector3 worldDirection = Transform.TransformDirection(localDirection).normalized;
                axesOrderDirection[(int)axis] = worldDirection;
            }

            return axesOrderDirection;
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
                ActualAxesOrder[5].ToString(), ActualAxesDirection[(int)ActualAxesOrder[5]]
                );

            return s.ToString();
        }
    }


    Transform model;
    public List<MJoint> Joints;
    private float angle;
    private float timeCount = 0.0f;
    List<Quaternion> curr_jointsRot = new List<Quaternion>();

    public enum UcyAxes
    {
        UP, RIGHT, FWD, DOWN, LEFT, BACK
    }

    public enum Filtering
    {
        NONE, LERP, ONEEURO,SGOLAY
    }

    public Model3D(Transform model3d)
    {
        model = model3d;
        Joints = setJoints();
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
        foreach (var val in Enum.GetValues(typeof(EnumModel3DJoints)))
        {
            result = model.FindDeepChild(name + val.ToString());
            result.name = val.ToString();
        }
    }

    public List<MJoint> setJoints()
    {
        Debug.Log("Getting transforms from model...");
        List<MJoint> list = new List<MJoint>();
        foreach (EnumModel3DJoints val in Enum.GetValues(typeof(EnumModel3DJoints)))
        {

            // Find the joint.
            Transform transform;
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

    private Vector3[] ybotDefaultAxes(EnumModel3DJoints joint)
    {
        return new Vector3[]
        {
            Vector3.up, Vector3.right, Vector3.forward, Vector3.down, Vector3.left, Vector3.back
        };
    }

    private Vector3[] femaleDefaultAXes(EnumModel3DJoints joint)
    {
        Vector3[] axes = null;
        switch (joint)
        {
            case EnumModel3DJoints.Head:

                break;
            case EnumModel3DJoints.LeftArm:
                break;
            case EnumModel3DJoints.LeftFoot:
                break;
            case EnumModel3DJoints.LeftForeArm:
                break;
            case EnumModel3DJoints.LeftHand:
                break;
            case EnumModel3DJoints.LeftLeg:
                break;
            case EnumModel3DJoints.LeftUpLeg:
                break;
            case EnumModel3DJoints.RightArm:
                break;
            case EnumModel3DJoints.RightFoot:
                break;
            case EnumModel3DJoints.RightForeArm:
                break;
            case EnumModel3DJoints.RightHand:
                break;
            case EnumModel3DJoints.RightLeg:
                break;
            case EnumModel3DJoints.RightUpLeg:
                break;
            case EnumModel3DJoints.Neck:
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
    public void moveSkeleton(Vector3[] newJointsPositions, Vector3? rootTranslation=null)
    {
        applyRotations(newJointsPositions,Filtering.NONE);
        if (rootTranslation != null)
        {
            Joints[(int)EnumModel3DJoints.Hips].Transform.position = rootTranslation.Value;
        }
    }

    public void moveSkeleton_OneEuroFilter(Vector3[] newJointsPositions, OneEuroFilter<Quaternion>[] rotationFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips=null, Vector3? rootTranslation = null)
    {
        applyRotations(newJointsPositions, Filtering.ONEEURO, rotationFiltersJoints);
        if (rootTranslation != null)
        {
            Joints[(int)EnumModel3DJoints.Hips].Transform.position = rootTranslation.Value;
        }
    }

    public void moveSkeletonLERP(Vector3[] newJointsPositions, Vector3? rootTranslation = null)
    {
        applyRotations(newJointsPositions, Filtering.LERP);

        if (rootTranslation != null)
        {
            Joints[(int)EnumModel3DJoints.Hips].Transform.position = rootTranslation.Value;
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

    public Quaternion GenericLookRotation(Vector3 targetDirection, Vector3 upwards, MJoint joint, UcyAxes abstractAxis, Filtering filter, OneEuroFilter<Quaternion>[] rotationFiltersJoints = null)
    {
        UcyAxes actualAxis = joint.getActualAxis(abstractAxis);
        Vector3 actualAxisDirection = joint.ActualAxesDirection[(int)actualAxis];

        // Find the rotation that points the axis we want to rotate to local fwd.
        Quaternion AbstractAxis_To_Forward = Quaternion.FromToRotation(actualAxisDirection, joint.ActualAxesDirection[(int)UcyAxes.FWD]);

        // After rotating the axes, up-axis might change.
        // LookRotation: Creates a rotation with specified forward and upwards directions
        Quaternion forwardToTarget = Quaternion.LookRotation(targetDirection, upwards);
        Quaternion rotation = forwardToTarget * AbstractAxis_To_Forward;
        
        switch (filter)
        {
            case Filtering.LERP:    return Quaternion.Lerp(curr_jointsRot[(int)joint.Tag], rotation, timeCount);
            case Filtering.ONEEURO: return rotationFiltersJoints[(int)joint.Tag].Filter(rotation);
            default: return rotation;
        }
    }

    private Vector3 UpwardsDirection(MJoint curr)
    {
        // Fix Rolling by defining Upwards direction:
        Vector3 fwdDirection;
        UcyAxes hipUPDirection = Joints[14].getActualAxis(UcyAxes.UP);
        UcyAxes otherUPDirection = curr.getActualAxis(UcyAxes.UP);
        if (hipUPDirection != otherUPDirection)
        {
            // Select the one with the smallest angle distance.

            fwdDirection = Joints[14].GetDirectionUsingAbstractAxis(UcyAxes.FWD); //new Vector3(0,0,1);
        }
        else
        {
            fwdDirection = Joints[14].GetDirectionUsingAbstractAxis(UcyAxes.BACK); // new Vector3(0, 0, -1);
        }
        // The result depends on the current hips direction! 
        return Joints[14].Transform.TransformDirection(fwdDirection); // upwards
    }

    public void applyRotations(Vector3[] newJoints, Filtering filter, OneEuroFilter<Quaternion>[] rotationFiltersJoints = null)
    {
        // Get current rotations
        curr_jointsRot = new List<Quaternion>();
        foreach (var val in Enum.GetValues(typeof(EnumModel3DJoints)))
        {
            curr_jointsRot.Add(Joints[(int)val].Transform.rotation);
        }

      

        // The order matters. The rotations should be applied first to the parent and then to the child.
        Vector3 root = (newJoints[8] + newJoints[11]) / 2; 
        // HIPS           
        Joints[14].Transform.rotation = GenericLookRotation(newJoints[8] - newJoints[11], newJoints[1] - root, Joints[14], UcyAxes.RIGHT, filter, rotationFiltersJoints);
        // HEAD
        // ----
        // NECK
        Joints[1].Transform.rotation = GenericLookRotation(newJoints[0] - newJoints[1], UpwardsDirection(Joints[1]), Joints[1], UcyAxes.UP, filter, rotationFiltersJoints);
        // RIGHT_ARM
        Joints[2].Transform.rotation = GenericLookRotation(newJoints[3] - newJoints[2], UpwardsDirection(Joints[2]), Joints[2], UcyAxes.RIGHT, filter, rotationFiltersJoints);
        // RIGHT_FORE_ARM
        Joints[3].Transform.rotation = GenericLookRotation(newJoints[4] - newJoints[3], UpwardsDirection(Joints[3]), Joints[3], UcyAxes.RIGHT, filter, rotationFiltersJoints);
        // RIGHT_HAND
        // ----
        // LEFT_ARM       
        Joints[5].Transform.rotation = GenericLookRotation(newJoints[5] - newJoints[6], UpwardsDirection(Joints[5]), Joints[5], UcyAxes.RIGHT, filter, rotationFiltersJoints);
        // LEFT_FORE_ARM
        Joints[6].Transform.rotation = GenericLookRotation(newJoints[6] - newJoints[7], UpwardsDirection(Joints[6]), Joints[6], UcyAxes.RIGHT, filter, rotationFiltersJoints);
        // LEFT_HAND    
        // ----
        // RIGHT_UP_LEG  
        Joints[8].Transform.rotation = GenericLookRotation(newJoints[8] - newJoints[9], UpwardsDirection(Joints[8]), Joints[8], UcyAxes.UP, filter, rotationFiltersJoints);
        // RIGHT_LEG     
        Joints[9].Transform.rotation = GenericLookRotation(newJoints[9] - newJoints[10], UpwardsDirection(Joints[9]), Joints[9], UcyAxes.UP, filter, rotationFiltersJoints);
        // RIGHT_FOOT    
        // ----
        // LEFT_UPLEG    
        Joints[11].Transform.rotation = GenericLookRotation(newJoints[11] - newJoints[12], UpwardsDirection(Joints[11]), Joints[11], UcyAxes.UP, filter, rotationFiltersJoints);
        // LEFT_LEG      
        Joints[12].Transform.rotation = GenericLookRotation(newJoints[12] - newJoints[13], UpwardsDirection(Joints[12]), Joints[12], UcyAxes.UP, filter, rotationFiltersJoints);
        // LEFT_FOOT     
        // ----
    }

    public void DebugAxes()
    {
        string s = model.name+":\n";
        foreach (MJoint j in Joints)
        {
            s += j.ToString();
        }
        Debug.Log(s);
    }


}
