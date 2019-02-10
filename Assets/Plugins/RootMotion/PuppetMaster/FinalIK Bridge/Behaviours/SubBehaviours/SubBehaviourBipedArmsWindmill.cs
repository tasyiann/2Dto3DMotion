using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBipedArmsWindmill : SubBehaviourBase {
		
		public Weight windmillWeight;
		public Weight muscleWeight = new Weight(1f);
		public Weight windmillSpeed = new Weight(12f);
		public Weight windmillSpread = new Weight(0.8f);
		public Weight windmillRadius = new Weight(0.5f);
		[Range(0f, 1f)] public float maintainArmRelativePos = 0.5f;
		[Range(0f, 1f)] public float windmillSyncOffset = 0.3f;
		public Vector3 windmillOffset;
			
		private float windmillAngle;
		private IKSolverFullBodyBiped solver;
		private SubBehaviourCOM centerOfMass;

		public void Initiate(BehaviourBase behaviour, SubBehaviourCOM centerOfMass, IKSolverFullBodyBiped solver) {
			this.behaviour = behaviour;
			this.centerOfMass = centerOfMass;
			this.solver = solver;

			behaviour.OnPostRead += OnPostRead;
		}

		#region Behaviour Delegates

		private void OnPostRead() {
			float mW = muscleWeight.GetValue(centerOfMass.angle);

			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Arm || m.props.group == Muscle.Group.Hand) m.state.muscleWeightMlp = mW;
			}
				
			// Chest
			Vector3 shoulderPosLeft = solver.leftShoulderEffector.bone.position + solver.leftShoulderEffector.positionOffset;
			Vector3 shoulderPosRight = solver.rightShoulderEffector.bone.position + solver.rightShoulderEffector.positionOffset;
			Vector3 shoulderDirection = (shoulderPosLeft - shoulderPosRight).normalized;
			Vector3 up = behaviour.puppetMaster.targetRoot.up;
			Vector3 chestForward = Vector3.Cross(up, shoulderDirection);
			float armLengthLeft = solver.leftArmChain.nodes[0].length + solver.leftArmChain.nodes[1].length;
			float armLengthRight = solver.rightArmChain.nodes[0].length + solver.rightArmChain.nodes[1].length;
				
			windmillAngle += Time.deltaTime * windmillSpeed.GetValue(centerOfMass.angle) * Mathf.Rad2Deg;
			if (windmillAngle > 360f) windmillAngle -= 360f;
			if (windmillAngle < -360f) windmillAngle += 360f;
				
			float w = windmillWeight.GetValue(centerOfMass.angle);

			solver.leftHandEffector.positionOffset += (GetArmPositionWindmill(shoulderPosLeft, shoulderDirection, chestForward, armLengthLeft, windmillAngle) - solver.leftHandEffector.bone.position) * w;
			solver.rightHandEffector.positionOffset += (GetArmPositionWindmill(shoulderPosRight, -shoulderDirection, chestForward, armLengthRight, -windmillAngle + windmillSyncOffset * 306f) - solver.rightHandEffector.bone.position) * w;
				
			// Maintain Relative Position Weight
			solver.leftHandEffector.maintainRelativePositionWeight = Mathf.Max(maintainArmRelativePos - w, 0f); // @todo reset maintainRelativePositionWeight in OnDisable
			solver.rightHandEffector.maintainRelativePositionWeight = Mathf.Max(maintainArmRelativePos - w, 0f);
		}

		#endregion Behaviour Delegates
			
		private Vector3 GetArmPositionWindmill(Vector3 shoulderPosition, Vector3 shoulderDirection, Vector3 chestForward, float armLength, float windmillAngle) {
			Quaternion chestRotation = Quaternion.LookRotation(chestForward, behaviour.puppetMaster.targetRoot.up);
				
			Vector3 toSide = shoulderDirection * armLength * windmillSpread.GetValue(centerOfMass.angle);
				
			Quaternion windmillRotation = Quaternion.AngleAxis(windmillAngle, shoulderDirection);
				
			Vector3 toWindmill = windmillRotation * chestForward * armLength * windmillRadius.GetValue(centerOfMass.angle);
			Vector3 windmillPos = shoulderPosition + toSide + toWindmill;
			windmillPos += chestRotation * windmillOffset;
				
			return windmillPos;
		}
	}
}
