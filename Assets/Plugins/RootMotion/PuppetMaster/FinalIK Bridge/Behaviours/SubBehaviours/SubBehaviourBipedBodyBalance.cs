using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBipedBodyBalance : SubBehaviourBase {

		public float maxOffset = 1f;
		public float offsetMagMlp = 15f;
		public float offsetSpeed;
		public Weight shoulderWeight;
		public Weight bodyWeight;
		public Weight muscleWeight = new Weight(1f);

		private IKSolverFullBodyBiped solver;
		private SubBehaviourCOM centerOfMass;
		private Vector3 offset;

		public void Initiate(BehaviourBase behaviour, SubBehaviourCOM centerOfMass, IKSolverFullBodyBiped solver) {
			this.behaviour = behaviour;
			this.centerOfMass = centerOfMass;
			this.solver = solver;

			behaviour.OnPreActivate += OnPreActivate;
			behaviour.OnPostRead += OnPostRead;
		}
		
		#region Behaviour Delegates

		private void OnPreActivate() {
			offset = Vector3.zero;
		}

		[Space(10)]
		public float maxAngle = 25f;
		public float thighWeight = 10f;
		public float calfWeight = 1f;
		public float spineWeight = 2f;
		public float headWeight = 1f;

		private void OnPostRead() {
			// Offset
			Vector3 shoulderCenter = Vector3.Lerp(solver.leftShoulderEffector.bone.position, solver.rightShoulderEffector.bone.position, 0.5f);
			Vector3 toShoulderCenter = shoulderCenter - behaviour.puppetMaster.muscles[0].target.position;
			
			Vector3 newShoulderCenter = behaviour.puppetMaster.muscles[0].target.position + centerOfMass.inverseRotation * toShoulderCenter;
			Vector3 offsetTarget = (newShoulderCenter - shoulderCenter);
			
			offsetTarget = Vector3.ClampMagnitude(offsetTarget * offsetMagMlp, maxOffset);
			offset = offsetSpeed <= 0f? offsetTarget: Vector3.Lerp(offset, offsetTarget, Time.deltaTime * offsetSpeed);

			// Muscle weights
			float w = muscleWeight.GetValue(centerOfMass.angle);
			
			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Spine) m.state.muscleWeightMlp = w;
			}

			// Body
			Vector3 bodyOffset = offset * bodyWeight.GetValue(centerOfMass.angle);
			
			solver.bodyEffector.positionOffset += bodyOffset;
			
			if (!solver.bodyEffector.effectChildNodes) {
				solver.leftThighEffector.positionOffset += bodyOffset;
				solver.rightThighEffector.positionOffset += bodyOffset;
			}

			solver.leftHandEffector.maintainRelativePositionWeight = 0f;
			solver.rightHandEffector.maintainRelativePositionWeight = 0f;

			//solver.leftShoulderEffector.positionOffset += bodyOffset;
			//solver.rightShoulderEffector.positionOffset += bodyOffset;
			//solver.leftHandEffector.positionOffset += bodyOffset;
			//solver.rightHandEffector.positionOffset += bodyOffset;

			// Shoulders
			Vector3 shoulderOffset = offset * shoulderWeight.GetValue(centerOfMass.angle);
			
			solver.leftShoulderEffector.positionOffset += shoulderOffset;
			solver.rightShoulderEffector.positionOffset += shoulderOffset;

			/*
			// Non-FBBIK version
			if (!centerOfMass.isGrounded) return;
			var biped = solver.GetRoot().GetComponent<FullBodyBipedIK>().references;

			Quaternion leftFootRotation = biped.leftFoot.rotation;
			Quaternion rightFootRotation = biped.rightFoot.rotation;

			float angle = 0f;
			Vector3 axis = Vector3.zero;
			centerOfMass.inverseRotation.ToAngleAxis(out angle, out axis);

			// Thighs
			float thighAngle = Mathf.Clamp(angle * thighWeight, -maxAngle, maxAngle);
			Quaternion thighRotation = Quaternion.AngleAxis(thighAngle, axis);

			biped.leftThigh.rotation = thighRotation * biped.leftThigh.rotation;
			biped.rightThigh.rotation = thighRotation * biped.rightThigh.rotation;

			// Calves
			float calfAngle = Mathf.Clamp(angle * calfWeight, -maxAngle, maxAngle);

			Vector3 legAxisLeft = Vector3.Cross(biped.leftCalf.position - biped.leftThigh.position, biped.leftFoot.position - biped.leftThigh.position);
			Vector3 axisLeft = Vector3.Project(axis, legAxisLeft);

			Vector3 legAxisRight = Vector3.Cross(biped.rightCalf.position - biped.rightThigh.position, biped.rightFoot.position - biped.rightThigh.position);
			Vector3 axisright = Vector3.Project(axis, legAxisRight);

			Quaternion calfRotationLeft = Quaternion.AngleAxis(-calfAngle, axisLeft);
			Quaternion calfRotationRight = Quaternion.AngleAxis(-calfAngle, axisright);

			biped.leftCalf.rotation = calfRotationLeft * biped.leftCalf.rotation;
			biped.rightCalf.rotation = calfRotationRight * biped.rightCalf.rotation;
	*/

			// Non-FBBIK version
			if (!centerOfMass.isGrounded) return;
			var biped = solver.GetRoot().GetComponent<FullBodyBipedIK>().references;
			
			float angle = 0f;
			Vector3 axis = Vector3.zero;
			centerOfMass.inverseRotation.ToAngleAxis(out angle, out axis);

			// Spine
			/*
			float spineAngle = Mathf.Clamp((angle * spineWeight) / biped.spine.Length, -maxAngle, maxAngle);
			Quaternion spinerotation = Quaternion.AngleAxis(spineAngle, axis);

			foreach (Transform s in biped.spine) {
				s.rotation = spinerotation * s.rotation;
			}
			*/

			// Head
			float headAngle = Mathf.Clamp(angle * headWeight, -maxAngle, maxAngle);
			Quaternion headRotation = Quaternion.AngleAxis(headAngle, axis);

			biped.head.rotation = headRotation * biped.head.rotation;
		}

		private void RotateMuscleTarget(Transform target, Rigidbody rigidbody, Rigidbody foot, float weight) {
			/*
			Vector3 dir = rigidbody.position - behaviour.puppetMaster.muscles[0].rigidbody.position;
			//Vector3 dir = rigidbody.position - centerOfMass.position;

			Quaternion r = Quaternion.LookRotation(dir);
			
			Quaternion rotation = Quaternion.Euler(weight * centerOfMass.inverseRotation.eulerAngles) * r;
			//Quaternion rotation = r * Quaternion.Euler(weight * centerOfMass.inverseRotation.eulerAngles);
			*/

			//Quaternion rotation = Quaternion.Euler(weight * centerOfMass.inverseRotation.eulerAngles);
			//Quaternion rotation = Quaternion.FromToRotation(centerOfMass.position - rigidbody.position, Vector3.up);
			//Quaternion rotation = Quaternion.FromToRotation(centerOfMass.direction, rigidbody.position - behaviour.puppetMaster.muscles[0].rigidbody.position);

			/*
			float angle = 0f;
			Vector3 axis = Vector3.zero;
			centerOfMass.inverseRotation.ToAngleAxis(out angle, out axis);

			angle = Mathf.Clamp(angle * weight, -maxAngle, maxAngle);
			Quaternion rotation = Quaternion.AngleAxis(angle, axis);
			*/

			/*
			Vector3 offset = rigidbody.position - centerOfMass.centerOfPressure;

			Quaternion rotation = Quaternion.FromToRotation(
				rigidbody.position - centerOfMass.centerOfPressure, 
				(rigidbody.position + centerOfMass.offset) - centerOfMass.centerOfPressure);
				*/

			/*
			Vector3 toFoot = foot.position - rigidbody.position;
			Quaternion fromTo = Quaternion.FromToRotation(toFoot, Vector3.down);

			float angle = 0f;
			Vector3 axis = Vector3.zero;
			fromTo.ToAngleAxis(out angle, out axis);

			float a = centerOfMass.angle * weight;
			if (angle < 0f) a = -a;
			a = Mathf.Clamp(a, -maxAngle, maxAngle);

			Quaternion rotation = Quaternion.AngleAxis(a, axis);

			target.rotation = rotation * target.rotation;
			*/

			Vector3 toThigh = rigidbody.position - centerOfMass.centerOfPressure;
			Quaternion fromTo = Quaternion.FromToRotation(toThigh, centerOfMass.direction);

			float angle = 0f;
			Vector3 axis = Vector3.zero;
			fromTo.ToAngleAxis(out angle, out axis);
			
			//float a = centerOfMass.angle * weight;
			//if (angle < 0f) a = -a;
			//a = Mathf.Clamp(a, -maxAngle, maxAngle);


			Quaternion rotation = Quaternion.AngleAxis(angle * thighWeight, axis);
			
			target.rotation = rotation * target.rotation;

		}

		#endregion Behaviour Delegates
	}
}
