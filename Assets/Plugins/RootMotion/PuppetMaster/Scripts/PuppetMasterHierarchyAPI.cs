using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Contains high level API calls for changing the PuppetMaster's muscle structure.
	public partial class PuppetMaster: MonoBehaviour {

		/// <summary>
		/// NB! Make sure to call this from FixedUpdate!
		/// Creates a new muscle for the specified "joint" and targets it to the "target". The joint will be connected to the specified "connectTo" Muscle.
		/// Note that the joint will be binded to it's current position and rotation relative to the "connectTo", so make sure the added object is positioned correctly when calling this.
		/// This method allocates memory, avoid using it each frame.
		/// </summary>
		public void AddMuscle(ConfigurableJoint joint, Transform target, Rigidbody connectTo, Transform targetParent, Muscle.Props muscleProps = null) {
			if (!CheckIfInitiated()) return;

			if (!initiated) {
				Debug.LogWarning("PuppetMaster has not been initiated.", transform);
				return;
			}
			
			if (ContainsJoint(joint)) {
				Debug.LogWarning("Joint " + joint.name + " is already used by a Muscle", transform);
				return;
			}
			
			if (target == null) {
				Debug.LogWarning("AddMuscle was called with a null 'target' reference.", transform);
				return;
			}
			
			if (connectTo == joint.GetComponent<Rigidbody>()) {
				Debug.LogWarning("ConnectTo is the joint's own Rigidbody, can not add muscle.", transform);
				return;
			}

			if (!isActive) {
				Debug.LogWarning("Adding muscles to inactive PuppetMasters is not currently supported.", transform);
				return;
			}
			
			if (muscleProps == null) muscleProps = new Muscle.Props();
			
			Muscle muscle = new Muscle();
			muscle.props = muscleProps;
			muscle.joint = joint;
			muscle.target = target;
			muscle.joint.transform.parent = HierarchyIsFlat() || connectTo == null? transform: connectTo.transform;
			joint.gameObject.layer = gameObject.layer; //@todo what if collider is on a child gameobject?
			target.gameObject.layer = targetRoot.gameObject.layer;
			
			if (connectTo != null) {
				muscle.target.parent = targetParent;
				
				Vector3 relativePosition = GetMuscle(connectTo).transform.InverseTransformPoint(muscle.target.position);
				Quaternion relativeRotation = Quaternion.Inverse(GetMuscle(connectTo).transform.rotation) * muscle.target.rotation;
				
				joint.transform.position = connectTo.transform.TransformPoint(relativePosition);
				joint.transform.rotation = connectTo.transform.rotation * relativeRotation;
				
				joint.connectedBody = connectTo;
			}
			
			muscle.Initiate(muscles);
			
			if (connectTo != null) {
				muscle.rigidbody.velocity = connectTo.velocity;
				muscle.rigidbody.angularVelocity = connectTo.angularVelocity;
			}
			
			// Ignore internal collisions
			if (!internalCollisions) {
				for (int i = 0; i < muscles.Length; i++) {
					muscle.IgnoreCollisions(muscles[i], true);
				}
			}
			
			Array.Resize(ref muscles, muscles.Length + 1);
			muscles[muscles.Length - 1] = muscle;
			
			// Update angular limit ignoring
			muscle.IgnoreAngularLimits(!angularLimits);

			if (behaviours.Length > 0) {
				muscle.broadcaster = muscle.joint.gameObject.AddComponent<MuscleCollisionBroadcaster>();
				muscle.broadcaster.puppetMaster = this;
				muscle.broadcaster.muscleIndex = muscles.Length - 1;
			}

			UpdateHierarchies();
		}
		
		/// <summary>
		/// Removes the muscle with the specified joint and all muscles connected to it from PuppetMaster management. This will not destroy the body part/prop, but just release it from following the target.
		/// If you call RemoveMuscleRecursive on an upper arm muscle, the entire arm will be disconnected from the rest of the body.
		/// If attachTarget is true, the target Transform of the first muscle will be parented to the disconnected limb.
		/// This method allocates some memory, avoid using it each frame.
		/// </summary>
		public void RemoveMuscleRecursive(ConfigurableJoint joint, bool attachTarget) {
			if (!CheckIfInitiated()) return;
			
			if (joint == null) {
				Debug.LogWarning("RemoveMuscleRecursive was called with a null 'joint' reference.", transform);
				return;
			}
			
			if (!ContainsJoint(joint)) {
				Debug.LogWarning("No Muscle with the specified joint was found, can not remove muscle.", transform);
				return;
			}

			int index = GetMuscleIndex(joint);
			Muscle[] newMuscles = new Muscle[muscles.Length - (muscles[index].childIndexes.Length + 1)];

			int added = 0;
			for (int i = 0; i < muscles.Length; i++) {
				if (i != index && !muscles[index].childFlags[i]) {
					newMuscles[added] = muscles[i];
					added ++;
				} else {
					if (muscles[i].broadcaster != null) Destroy(muscles[i].broadcaster);
				}
			}
			
			muscles[index].joint.connectedBody = null;

			muscles[index].joint.targetRotation = Quaternion.identity;
			JointDrive j = new JointDrive();
			j.mode = JointDriveMode.None;
			muscles[index].joint.slerpDrive = j;
			muscles[index].joint.xMotion = ConfigurableJointMotion.Free;
			muscles[index].joint.yMotion = ConfigurableJointMotion.Free;
			muscles[index].joint.zMotion = ConfigurableJointMotion.Free;
			muscles[index].joint.angularXMotion = ConfigurableJointMotion.Free;
			muscles[index].joint.angularYMotion = ConfigurableJointMotion.Free;
			muscles[index].joint.angularZMotion = ConfigurableJointMotion.Free;
			
			muscles[index].transform.parent = null;
			
			if (attachTarget) {
				muscles[index].target.parent = muscles[index].transform;
				muscles[index].target.position = muscles[index].transform.position;
				muscles[index].target.rotation = muscles[index].transform.rotation * muscles[index].targetRotationRelative;
			}
			
			muscles = newMuscles;
			
			UpdateHierarchies();
		}
		
		/// <summary>
		/// NB! Make sure to call this from FixedUpdate!
		/// Replaces a muscle with a new one. This can be used to replace props, 
		/// but in most cases it would be faster and more efficient to do it by maintaining the muscle (the Joint and the Rigidbody) and just replacing the colliders and the graphical model.
		/// This method allocates memory, avoid using it each frame.
		/// </summary>
		public void ReplaceMuscle(ConfigurableJoint oldJoint, ConfigurableJoint newJoint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// NB! Make sure to call this from FixedUpdate!
		/// Completely replaces the muscle structure. Make sure the new muscle objects are positioned and rotated correctly relative to their targets.
		/// This method allocates memory, avoid using it each frame.
		/// </summary>
		public void SetMuscles(Muscle[] newMuscles) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// Disables the muscle with the specified joint and all muscles connected to it. This is a faster and more efficient alternative to RemoveMuscleRecursive,
		/// as it will not require reinitiating the muscles.
		/// </summary>
		public void DisableMuscleRecursive(ConfigurableJoint joint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// Re-enables a previously disabled muscle and the muscles connected to it.
		/// </summary>
		public void EnableMuscleRecursive(ConfigurableJoint joint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}

		/// <summary>
		/// Flattens the ragdoll hierarchy so that all muscles are parented to the PuppetMaster.
		/// </summary>
		[ContextMenu("Flatten Muscle Hierarchy")]
		public void FlattenHierarchy() {
			foreach (Muscle m in muscles) {
				if (m.joint != null) m.joint.transform.parent = transform;
			}
		}

		/// <summary>
		/// Builds a hierarchy tree from the muscles.
		/// </summary>
		[ContextMenu("Tree Muscle Hierarchy")]
		public void TreeHierarchy() {
			foreach (Muscle m in muscles) {
				if (m.joint != null) {
					m.joint.transform.parent = m.joint.connectedBody != null? m.joint.connectedBody.transform: transform;
				}
			}
		}

		private void AddIndexesRecursive(int index, ref int[] indexes) {
			int l = indexes.Length;
			Array.Resize(ref indexes, indexes.Length + 1 + muscles[index].childIndexes.Length);
			indexes[l] = index;
			
			if (muscles[index].childIndexes.Length == 0) return;
			
			for (int i = 0; i < muscles[index].childIndexes.Length; i++) {
				AddIndexesRecursive(muscles[index].childIndexes[i], ref indexes);
			}
		}

		// Are all the muscles parented to the PuppetMaster Transform?
		private bool HierarchyIsFlat() {
			foreach (Muscle m in muscles) {
				if (m.joint.transform.parent != transform) return false;
			}
			return true;
		}
	}
}
