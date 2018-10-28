using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourBipedBalaceFBBIK")]
	public partial class BehaviourBipedBalanceFBBIK : BehaviourBase {

		public bool debugRestart;
		public PhysicMaterial material;
		public Weight rigidbodyDrag;
		public LayerMask groundLayers;

		[Space(5)] public SubBehaviourCOM centerOfMass;
		[Space(5)] public SubBehaviourBipedBodyBalance body;
		[Space(5)] public SubBehaviourBipedLegsBalance legs;
		[Space(5)] public SubBehaviourBipedArmsWindmill armsWindmill;
		[Space(5)] public SubBehaviourBipedHeadCatchFall head;

		[Header("Events")]
		public float loseBalanceAngle = 60f;
		public float unbalancedMuscleWeight = 0.01f;
		public PuppetEvent onLoseBalance;

		private FullBodyBipedIK ik;

		protected override void OnInitiate() {
			ik = puppetMaster.targetRoot.GetComponentInChildren<FullBodyBipedIK>();
			if (ik == null) {
				Debug.LogWarning("No FullBodyBipedIK component found on the PuppetMater's target root. Required by BehaviourBipedBalanceFBBIK.");
				return;
			}

			ik.enabled = false;

			centerOfMass.Initiate(this as BehaviourBase, groundLayers);
			body.Initiate(this as BehaviourBase, centerOfMass, ik.solver);
			legs.Initiate(this as BehaviourBase, centerOfMass, ik.solver);
			armsWindmill.Initiate(this as BehaviourBase, centerOfMass, ik.solver);
			head.Initiate(this as BehaviourBase, centerOfMass);

			OnPostRead += AfterSubBehaviourRead;

			Activate(); // @todo remove this
		}

		protected override void OnActivate() {
			puppetMaster.broadcastCollisions = true;
			puppetMaster.broadcastGroundCollisions = true;

			foreach (Muscle m in puppetMaster.muscles) {
				m.state.muscleWeightMlp = 1f;
				m.state.pinWeightMlp = 0f;

				foreach (Collider c in m.colliders) c.material = material;
			}
		}

		// @todo decrease drag by collision
		// @todo increase solver iteration count while enabled
		private void LoseBalance() {
			foreach (Muscle m in puppetMaster.muscles) {
				m.state.muscleWeightMlp = unbalancedMuscleWeight;
				m.rigidbody.drag = 0f; // @todo use last known value
			}

			this.enabled = false;

			onLoseBalance.Trigger(puppetMaster);

			if (debugRestart) StartCoroutine(DelayedRestart(2f)); // @todo for testing only
		}

		protected override void OnReadBehaviour() {
			// Rotate the target to hips forward
			RotateTargetToRootMuscle();

			// Drag
			float drag = rigidbodyDrag.GetValue(centerOfMass.angle);
			if (drag > 0f && centerOfMass.isGrounded) {

				foreach (Muscle m in puppetMaster.muscles) {
					Vector3 newV = m.rigidbody.velocity;
					newV -= Vector3.ClampMagnitude(newV * drag * Time.deltaTime, newV.magnitude);
					newV.y = m.rigidbody.velocity.y;

					m.rigidbody.velocity = newV;
				}
			}

			// Loosing balance
			if (centerOfMass.angle > loseBalanceAngle) LoseBalance();
		}

		protected override void OnFixTransformsBehaviour() {
			if (ik.fixTransforms) ik.solver.FixTransforms();
		}

		private void AfterSubBehaviourRead() {
			ik.solver.Update();
		}

		public IEnumerator DelayedRestart(float delay) {
			yield return new WaitForSeconds(delay);

			StartCoroutine(Restart());
		}

		public IEnumerator Restart() {
			foreach (Muscle m in puppetMaster.muscles) {
				m.state.pinWeightMlp = 1f;
				m.state.muscleWeightMlp = 1f;
			}
			
			yield return new WaitForSeconds(1f);
			this.Activate();
		}
	}
}
