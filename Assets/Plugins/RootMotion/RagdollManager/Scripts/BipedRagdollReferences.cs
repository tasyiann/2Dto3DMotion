using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Holds references to all Transforms required for a biped ragdoll.
	/// </summary>
	[System.Serializable]
	public struct BipedRagdollReferences {

		/// <summary>
		/// The root transform is the parent of all the biped's bones and should be located at ground level.
		/// </summary>
		public Transform root;
		/// <summary>
		/// The pelvis bone.
		/// </summary>
		public Transform hips;
		/// <summary>
		/// The middle spine bone.
		/// </summary>
		public Transform spine;
		/// <summary>
		/// The last spine bone.
		/// </summary>
		public Transform chest;
		/// <summary>
		/// The head.
		/// </summary>
		public Transform head;
		/// <summary>
		/// The first bone of the left leg.
		/// </summary>
		public Transform leftUpperLeg;
		/// <summary>
		/// The second bone of the left leg.
		/// </summary>
		public Transform leftLowerLeg;
		/// <summary>
		/// The third bone of the left leg.
		/// </summary>
		public Transform leftFoot;
		/// <summary>
		/// The first bone of the right leg.
		/// </summary>
		public Transform rightUpperLeg;
		/// <summary>
		/// The second bone of the right leg.
		/// </summary>
		public Transform rightLowerLeg;
		/// <summary>
		/// The third bone of the right leg.
		/// </summary>
		public Transform rightFoot;
		/// <summary>
		/// The first bone of the left arm.
		/// </summary>
		public Transform leftUpperArm;
		/// <summary>
		/// The second bone of the left arm.
		/// </summary>
		public Transform leftLowerArm;
		/// <summary>
		/// The third bone of the left arm.
		/// </summary>
		public Transform leftHand;
		/// <summary>
		/// The first bone of the right arm.
		/// </summary>
		public Transform rightUpperArm;
		/// <summary>
		/// The second bone of the right arm.
		/// </summary>
		public Transform rightLowerArm;
		/// <summary>
		/// The third bone of the right arm.
		/// </summary>
		public Transform rightHand;

		public bool IsValid() {
			if (root == null ||
				hips == null ||
			    head == null ||
			    leftUpperArm == null ||
			    leftLowerArm == null ||
			    leftHand == null ||

			    rightUpperArm == null ||
			    rightLowerArm == null ||
			    rightHand == null ||

			    leftUpperLeg == null ||
			    leftLowerLeg == null ||
			    leftFoot == null ||

			    rightUpperLeg == null ||
			    rightLowerLeg == null ||
			    rightFoot == null
			    ) return false;

			return true;
		}

		public bool IsEmpty(bool considerRoot) {
			if (considerRoot && root != null) return false;

			if (hips != null ||
			    head != null ||
			    spine != null ||
			    chest != null ||
			    leftUpperArm != null ||
			    leftLowerArm != null ||
			    leftHand != null ||
			    rightUpperArm != null ||
			    rightLowerArm != null ||
			    rightHand != null ||
			    
			    leftUpperLeg != null ||
			    leftLowerLeg != null ||
			    leftFoot != null ||
			    
			    rightUpperLeg != null ||
			    rightLowerLeg != null ||
			    rightFoot != null
			    ) return false;

			return true;
		}

		public Transform[] GetRagdollTransforms() {
			return new Transform[16] {
				hips,
				spine,
				chest,
				head,
				leftUpperArm,
				leftLowerArm,
				leftHand,
				rightUpperArm,
				rightLowerArm,
				rightHand,
				leftUpperLeg,
				leftLowerLeg,
				leftFoot,
				rightUpperLeg,
				rightLowerLeg,
				rightFoot
			};
		}

		public static BipedRagdollReferences FromAvatar(Animator animator) {
			BipedRagdollReferences r = new BipedRagdollReferences();

			if (!animator.isHuman) return r;

			r.root = animator.transform;

			r.hips = animator.GetBoneTransform(HumanBodyBones.Hips);
			r.spine = animator.GetBoneTransform(HumanBodyBones.Spine);
			r.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
			r.head = animator.GetBoneTransform(HumanBodyBones.Head);
			
			r.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			r.leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			r.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			
			r.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			r.rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			r.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			
			r.leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			r.leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			r.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			
			r.rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			r.rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			r.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

			return r;
		}

		public static BipedRagdollReferences FromBipedReferences(BipedReferences biped) {
			BipedRagdollReferences r = new BipedRagdollReferences();

			r.root = biped.root;

			r.hips = biped.pelvis;

			if (biped.spine != null && biped.spine.Length > 0) {
				r.spine = biped.spine[0];
				if (biped.spine.Length > 1) r.chest = biped.spine[biped.spine.Length - 1];
			}

			r.head = biped.head;
			
			r.leftUpperArm = biped.leftUpperArm;
			r.leftLowerArm = biped.leftForearm;
			r.leftHand = biped.leftHand;
			
			r.rightUpperArm = biped.rightUpperArm;
			r.rightLowerArm = biped.rightForearm;
			r.rightHand = biped.rightHand;
			
			r.leftUpperLeg = biped.leftThigh;
			r.leftLowerLeg = biped.leftCalf;
			r.leftFoot = biped.leftFoot;
			
			r.rightUpperLeg = biped.rightThigh;
			r.rightLowerLeg = biped.rightCalf;
			r.rightFoot = biped.rightFoot;
			
			return r;
		}
	}
}
