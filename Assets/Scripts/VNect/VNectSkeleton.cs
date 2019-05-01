using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static AnimationFilesHacker.DataLoader;

namespace AnimationFilesHacker
{
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


    public class VNectSkeleton
    {
        public enum JointsDefinition
        {
            Head, Neck, LeftArm, LeftForeArm, LeftHand, RightArm, RightForeArm, RightHand, LeftUpLeg, LeftLeg, LeftFoot, RightUpLeg, RightLeg, RightFoot, Root
        }

        private readonly int NUM_JOINTS; // Discard LeftFootEnd, RightFootEnd
        private readonly float LINE_WIDTH = 1f;
        private readonly Vector3 SPHERE_SCALE = new Vector3(3f,3f,3f);

        private List<Vector3> joints;
        private bool FixedRootRotation;
        public List<Vector3> Joints { get => joints; set { joints = value; updateSkeleton(); } }
        public GameObject SkeletonGameObject { get; set; }
        public List<Transform> JointsGameObjects;
        public Material material;
        public float ActualSkeletonHeight;
        public float NormalizedSkeletonHeight;
        public float ScalingFactor;
        List<AnimationFrame> Frames;

        public VNectSkeleton(GameObject prefabSkeleton, string name, List<AnimationFrame> frames, bool fixedRootRotation, int numOfJoints = 15, bool normalize = true, DataType dataType = DataType.Ik3D,  float targetHeight = 1f)
        {
            NUM_JOINTS = numOfJoints;
            instantiateSkeleton(prefabSkeleton, name);
            JointsGameObjects = getJointGameObjects();
            resizeSkeleton(SPHERE_SCALE, LINE_WIDTH);
            Frames = frames;
            ActualSkeletonHeight = getHeightOfSkeleton();
            FixedRootRotation = fixedRootRotation;

            if (normalize)
            {
                ScalingFactor = getScalingFactor(ActualSkeletonHeight, targetHeight);
                NormalizeFrames(ScalingFactor, dataType);
                NormalizedSkeletonHeight = getHeightOfSkeleton();
            }


            if(FixedRootRotation)
            {
                ApplyFixedRootRotation();
            }

        }

        public void ApplyFixedRootRotation()
        {
            foreach (AnimationFrame frame in Frames)
            {
                List<Vector3> frameJoints = frame.SkeletonJoints;
                // Set initial positions.
                JointsGameObjects[(int)JointsDefinition.Root].rotation = Quaternion.identity;
                for (int i = 0; i < NUM_JOINTS; i++)
                {
                    Transform joint = JointsGameObjects[i];
                    joint.position = frameJoints[i];
                }

                // Rotate Root.
                Quaternion result = Triangle3DRot(frameJoints);
                JointsGameObjects[(int)JointsDefinition.Root].rotation = result;

                // Save new positions
                frame.SkeletonJoints = getPositionsOfJointGameObjects();
            }
        }


        private List<Transform> getJointGameObjects()
        {
            List<Transform> list = new List<Transform>();
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                list.Add(SkeletonGameObject.transform.FindDeepChild(i.ToString()));
            }
            return list;
        }

        private List<Vector3> getPositionsOfJointGameObjects()
        {
            List<Vector3> positionList = new List<Vector3>();
            foreach (Transform t in JointsGameObjects)
            {
                positionList.Add(t.position);
            }
            return positionList;
        }

        public GameObject instantiateSkeleton(GameObject prefabSkeleton, string name)
        {
            SkeletonGameObject = UnityEngine.Object.Instantiate(prefabSkeleton);
            SkeletonGameObject.name = name;
            return SkeletonGameObject;
        }

        
        private void NormalizeFrames(float scalingFactor, DataType dataType)
        {
            foreach(AnimationFrame frame in Frames)
            {
                NormalizeSkeleton(frame.SkeletonJoints, scalingFactor, dataType);
            }
        }


        
        public float getHeightOfSkeleton()
        {
            float result = 0;
            foreach (AnimationFrame frame in Frames)
            {
                List<Vector3> s = frame.SkeletonJoints;
                float FootLeg = Vector3.Distance(s[(int)JointsDefinition.RightFoot], s[(int)JointsDefinition.RightLeg]);
                float LegHip = Vector3.Distance(s[(int)JointsDefinition.RightLeg], s[(int)JointsDefinition.RightUpLeg]);
                float HipNeck = Vector3.Distance(s[(int)JointsDefinition.Root], s[(int)JointsDefinition.Neck]);
                float neckToHead = Vector3.Distance(s[(int)JointsDefinition.Neck], s[(int)JointsDefinition.Head]);
                float actualHeight = (FootLeg + LegHip) + HipNeck + neckToHead;
                result += actualHeight;
            }
            return result/Frames.Count;
        }

        private float getScalingFactor(float skeletonHeight, float targetHeight)
        {
            return targetHeight / skeletonHeight;
        }


        public void updateSkeleton()
        {
            // Set Spheres
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                JointsGameObjects[i].position = Joints[i];
            }


            // Set Lines
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                Transform joint = JointsGameObjects[i];
                foreach (Transform child in joint.transform)
                {
                    LineRenderer childLineRenderer = child.GetComponent<LineRenderer>();
                    Vector3[] positions = {child.position, child.parent.position };
                    childLineRenderer.SetPositions(positions);
                }
            }

        }

        private Quaternion Triangle3DRot(List<Vector3> joints)
        {
            // https://stackoverflow.com/questions/11217680/how-to-calculate-the-quaternion-that-represents-a-triangles-3d-rotation
            // Define a triangle plane
            Vector3 s1, s2, s3, t1, t2, t3;
            s1 = joints[(int)JointsDefinition.LeftUpLeg];
            s2 = joints[(int)JointsDefinition.RightUpLeg];
            s3 = joints[(int)JointsDefinition.Neck];

            // Yes, they are the same.
            // Debug.Log(s1+","+JointsGameObjects[(int)JointsDefinition.LeftUpLeg].position);

            t1 = new Vector3(-1f, 0f, 0f);
            t2 = new Vector3(1f, 0f, 0f);
            t3 = s3;
            // Calculations
            Vector3 normSource = Vector3.Cross((s1 - s2),(s1 - s3));
            Vector3 normTarget = Vector3.Cross((t1 - t2),(t1 - t3));
            Quaternion quat1 = Quaternion.FromToRotation(normSource,normTarget);
            Quaternion quat2 = Quaternion.FromToRotation(quat1 * (s1-s2),(t1-t2));
            Quaternion QuatFinal = quat2 * quat1;
            return QuatFinal;
        }
        


        public void resizeSkeleton(Vector3 scale, float lineWidth)
        {
            JointsGameObjects[(int)JointsDefinition.Root].localScale = scale;
            foreach (Transform t in JointsGameObjects)
            {
                LineRenderer lr = t.GetComponent<LineRenderer>();
                lr.endWidth = lineWidth;
                lr.startWidth = lineWidth;
            }
        }


        public void NormalizeSkeleton(List<Vector3> JointsToBeNormalized, float scalingFactor, DataType dataType)
        {
            Vector3 rootPosition = JointsToBeNormalized[(int)JointsDefinition.Root];
            rootPosition = new Vector3(rootPosition.x, dataType == DataType.UCY_Ik3D ? rootPosition.y : -rootPosition.y, rootPosition.z);

            for (int i=0; i< NUM_JOINTS; i++)
            {
                JointsToBeNormalized[i] = new Vector3(JointsToBeNormalized[i].x, dataType == DataType.UCY_Ik3D? JointsToBeNormalized[i].y : -JointsToBeNormalized[i].y, JointsToBeNormalized[i].z);
                JointsToBeNormalized[i] = (JointsToBeNormalized[i] - rootPosition) * scalingFactor;
            }
        }


        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("Bones Length:\n");
            foreach(Transform joint in JointsGameObjects)
            {
                foreach (Transform child in joint.transform)
                    s.AppendFormat("{0,-3} , {1,-3} = {2,8}\n",joint.name, child.name, Vector3.Distance(joint.position, child.position));
            }
            s.AppendFormat("Actual Skeleton Height: {0}\nCurrent Skeleton Height: {1}\nScaling Factor: {2}",ActualSkeletonHeight,NormalizedSkeletonHeight,ScalingFactor);
            return s.ToString();
        }

        public string skeletonInfoScale()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("Actual Skeleton Height: {0}\nCurrent Skeleton Height: {1}\nScaling Factor: {2}", ActualSkeletonHeight, NormalizedSkeletonHeight, ScalingFactor);
            return s.ToString();
        }

    }
}