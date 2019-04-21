using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VNectHacker
{
    public class VNectFrame
    {
        public List<Vector3> SkeletonJoints { get; set; }
        public int FrameNumber { get; set; }

        public VNectFrame(List<Vector3> skeleton, int frameNumber)
        {
            SkeletonJoints = skeleton;
            FrameNumber = frameNumber;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            int counter=0;
            s.AppendFormat("Frame:{0}\n",FrameNumber);
            foreach (Vector3 joint in SkeletonJoints)
            {
                s.AppendFormat("{0,4}: {1,10}, {2,10}, {3,10}\n",counter,joint.x,joint.y,joint.z);
                counter++;
            }
            return s.ToString();
        }
    }
}