using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBipedLegsBalance : SubBehaviourBase {
		
		public Weight feetWeight;
		[Range(0f, 90f)] public float maxFootAngle = 20f;
		[Range(0f, 60f)] public float footSpeed;
		[Range(0.0001f, 10f)] public float footMassMlp = 2f;
		[Range(1f, 100f)] public float footInertiaTensorMlp = 1f;
		public Weight legMuscleWeight = new Weight(1f);
		public Weight footMuscleWeight = new Weight(1f);

		private IKSolverFullBodyBiped solver;
		private SubBehaviourCOM centerOfMass;
		private Quaternion leftFootRotationOffset = Quaternion.identity;
		private Quaternion rightFootRotationOffset = Quaternion.identity;
		private float leftFootMass, rightFootMass;
		private Vector3 leftFootInertiaTensor, rightFootInertiaTensor;
		private Rigidbody leftFootRigidbody, rightFootRigidbody;

		public void Initiate(BehaviourBase behaviour, SubBehaviourCOM centerOfMass, IKSolverFullBodyBiped solver) {
			this.behaviour = behaviour;
			this.centerOfMass = centerOfMass;
			this.solver = solver;

			behaviour.OnPreActivate += OnPreActivate;
			behaviour.OnPostRead += OnPostRead;
			behaviour.OnPreDisable += OnPreDisable;

			leftFootRigidbody = behaviour.puppetMaster.GetMuscle(solver.leftFootEffector.bone).rigidbody;
			rightFootRigidbody = behaviour.puppetMaster.GetMuscle(solver.rightFootEffector.bone).rigidbody;
			
			leftFootMass = leftFootRigidbody.mass;
			rightFootMass = rightFootRigidbody.mass;
			
			leftFootInertiaTensor = leftFootRigidbody.inertiaTensor;
			rightFootInertiaTensor = rightFootRigidbody.inertiaTensor;
		}
		
		#region Behaviour Delegates
		
		private void OnPreActivate() {
			leftFootRigidbody.mass = leftFootMass * footMassMlp;
			rightFootRigidbody.mass = rightFootMass * footMassMlp;
			
			leftFootRotationOffset = Quaternion.identity;
			rightFootRotationOffset = Quaternion.identity;
			
			leftFootRigidbody.inertiaTensor = leftFootInertiaTensor * footInertiaTensorMlp;
			rightFootRigidbody.inertiaTensor = rightFootInertiaTensor * footInertiaTensorMlp;
		}
		
		private void OnPostRead() {
			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Leg) m.state.muscleWeightMlp = legMuscleWeight.GetValue(centerOfMass.angle);
				if (m.props.group == Muscle.Group.Foot) m.state.muscleWeightMlp = footMuscleWeight.GetValue(centerOfMass.angle);
			}

			// @todo lift an ungrounded leg

			float angle;
			Vector3 axis;
			centerOfMass.rotation.ToAngleAxis(out angle, out axis);
			
			angle *= feetWeight.GetValue(centerOfMass.angle);
			
			Quaternion feetRotationOffsetTarget = Quaternion.AngleAxis(Mathf.Clamp(angle, -maxFootAngle, maxFootAngle), axis);
			
			leftFootRotationOffset = footSpeed <= 0f? feetRotationOffsetTarget: Quaternion.Lerp(leftFootRotationOffset, feetRotationOffsetTarget, Time.deltaTime * footSpeed);
			rightFootRotationOffset = footSpeed <= 0f? feetRotationOffsetTarget: Quaternion.Lerp(rightFootRotationOffset, feetRotationOffsetTarget, Time.deltaTime * footSpeed);
			
			solver.leftFootEffector.bone.rotation = leftFootRotationOffset * solver.leftFootEffector.bone.rotation;
			solver.rightFootEffector.bone.rotation = rightFootRotationOffset * solver.rightFootEffector.bone.rotation;
		}

		private void OnPreDisable() {
			leftFootRigidbody.mass = leftFootMass;
			rightFootRigidbody.mass = rightFootMass;
			
			leftFootRigidbody.inertiaTensor = leftFootInertiaTensor;
			rightFootRigidbody.inertiaTensor = rightFootInertiaTensor;
		}
		
		#endregion Behaviour Delegates
	}
}
