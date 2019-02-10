using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {
	
	// Handles damaging the puppet via collisions and hits.
	public partial class BehaviourPuppet : BehaviourBase {

		private MuscleCollisionBroadcaster broadcaster;

		/// <summary>
		/// Knock out this puppet.
		/// </summary>
		public void Unpin() {
			Debug.Log("BehaviourPuppet.Unpin() has been deprecated. Use SetState(BehaviourPuppet.State) instead.");
			SetState(State.Unpinned);
		}

		// When a muscle is hit (through MuscleCollisionBroadcaster.Hit(...))
		protected override void OnMuscleHitBehaviour(MuscleHit hit) {
			// Should we activate the puppet?
			if (canActivate) puppetMaster.mode = PuppetMaster.Mode.Active;

			// Unpin the muscle (and other muscles) and add force
			UnPin(hit.muscleIndex, hit.unPin);

			// Add force
			puppetMaster.muscles[hit.muscleIndex].rigidbody.isKinematic = false;
			puppetMaster.muscles[hit.muscleIndex].rigidbody.AddForceAtPosition(hit.force, hit.position);
		}

		// When a muscle collides with something (called by the MuscleCollisionBroadcaster component on the muscle).
		protected override void OnMuscleCollisionBehaviour(MuscleCollision m) {
			// All the conditions for ignoring this
			if (!enabled) return;
			if (state == State.Unpinned) return;
			if (!LayerMaskExtensions.Contains(collisionLayers, m.collision.gameObject.layer)) return;
			if (!puppetMaster.isActive && !activateOnStaticCollisions && m.collision.gameObject.isStatic) return;
			if (collisionThreshold > 0f) {
				float mlp = PuppetMasterSettings.instance != null? (1f + PuppetMasterSettings.instance.currentlyActivePuppets * PuppetMasterSettings.instance.activePuppetCollisionThresholdMlp): 1f;
				if (m.collision.impulse.sqrMagnitude < collisionThreshold * mlp) return;
			}

			// Get the collision impulse on the muscle
			float impulse = GetImpulse(m);
			if (impulse <= 0f) return;

			// Try to find out if it collided with another puppet's muscle
			if (m.collision.collider.attachedRigidbody != null) {	
				broadcaster = m.collision.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
				if (broadcaster != null) {
					if (broadcaster.muscleIndex < broadcaster.puppetMaster.muscles.Length) {
						// Multiply impulse (if the other muscle has been boosted)
						impulse *= broadcaster.puppetMaster.muscles[broadcaster.muscleIndex].state.impulseMlp;
					}
				}
			}

			// Should we activate the puppet?
			if (Activate(m.collision, impulse)) puppetMaster.mode = PuppetMaster.Mode.Active;

			// Let other scripts know about the collision
			if (OnCollisionImpulse != null) OnCollisionImpulse(m, impulse);

			// Unpin the muscle (and other muscles)
			UnPin(m.muscleIndex, impulse);
		}

		// Calculating the impulse magnitude from a collision
		private float GetImpulse(MuscleCollision m) {
			#if UNITY_5_1
			// Physically incorrect, but more or less working and very fast hack for pre-Unity5.2 versions
			return (puppetMaster.muscles[m.muscleIndex].rigidbody.velocity - puppetMaster.muscles[m.muscleIndex].state.velocity).magnitude;

			#else
			// Since Unity 5.2 this is so easy
			float i = m.collision.impulse.magnitude;

			foreach (CollisionResistanceMultiplier crm in collisionResistanceMultipliers) {
				if (LayerMaskExtensions.Contains(crm.layers, m.collision.collider.gameObject.layer)) {
					if (crm.multiplier <= 0f) i = Mathf.Infinity;
					else i /= crm.multiplier;
					break;
				}
			}
			
			return i;
			#endif
		}

		// Unpin a muscle and other muscles linked to it
		private void UnPin(int muscleIndex, float unpin) {
			BehaviourPuppet.MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
			
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				UnPinMuscle(i, unpin * GetFalloff(i, muscleIndex, props.unpinParents, props.unpinChildren, props.unpinGroup));
			}
		}

		// Unpin a specific muscle according to it's collision resistance, immunity and other values
		private void UnPinMuscle(int muscleIndex, float unpin) {
			// All the conditions to ignore this
			if (unpin <= 0f) return;
			if (puppetMaster.muscles[muscleIndex].state.immunity >= 1f) return;

			// Find the group properties
			BehaviourPuppet.MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);

			// Making the puppet more resistant to collisions while getting up
			float stateF = 1f;
			if (state == State.GetUp) stateF = Mathf.Lerp(getUpCollisionResistanceMlp, 1f, puppetMaster.muscles[muscleIndex].state.pinWeightMlp);

			// Applying collision resistance
			float cR = collisionResistance.mode == Weight.Mode.Float? collisionResistance.floatValue: collisionResistance.GetValue(puppetMaster.muscles[muscleIndex].targetVelocity.magnitude);
			float damage = unpin / (props.collisionResistance * cR * stateF);
			damage *= 1f - puppetMaster.muscles[muscleIndex].state.immunity;

			// Finally apply the damage
			puppetMaster.muscles[muscleIndex].state.pinWeightMlp -= damage;
		}

	}
}