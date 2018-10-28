using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {
	
	// Handles switching BehaviourPuppet states.
	public partial class BehaviourPuppet : BehaviourBase {

		// Force the puppet to another state
		public void SetState(State newState) {
			// If already in this state, do nothing
			if (state == newState) return;

			// Start/stop listening to collisions (optimization)
			puppetMaster.broadcastCollisions = newState == State.Puppet || newState == State.GetUp;

			switch(newState) {
				
			// Switching to the PUPPET state
			case State.Puppet:
				puppetMaster.SampleTargetMappedState();

				// If switching from the unpinned state...
				if (state == State.Unpinned) {
					// Repin the puppet
					foreach (Muscle m in puppetMaster.muscles) {
						m.state.pinWeightMlp = 1f;
						m.state.muscleWeightMlp = 1f;
						m.state.muscleDamperAdd = 0f;
						
						var props = GetProps(m.props.group);

						// Change physyc materials
						foreach (Collider c in m.colliders) {
							c.material = props.puppetMaterial != null? props.puppetMaterial: defaults.puppetMaterial;
							if (props.disableColliders) c.enabled = false;
						}
					}
				}

				// Call events
				state = State.Puppet;
				onRegainBalance.Trigger(puppetMaster);
				if (onRegainBalance.switchBehaviour) return;
				
				break;
				
			// Switching to the UNPINNED state
			case State.Unpinned:
				unpinnedTimer = 0f;

				foreach (Muscle m in puppetMaster.muscles) {
					m.state.immunity = 0f;
					var props = GetProps(m.props.group);

					// Change physic materials
					foreach (Collider c in m.colliders) {
						c.material = props.unpinnedMaterial != null? props.unpinnedMaterial: defaults.unpinnedMaterial;

						// Enable colliders
						c.enabled = true;
					}
				}

				// Drop all the props
				if (dropProps) {
					RemoveMusclesOfGroup(Muscle.Group.Prop);
				}

				// Trigger events
				onLoseBalance.Trigger(puppetMaster);
				if (onLoseBalance.switchBehaviour) {
					state = State.Unpinned;
					return;
				}

				// Trigger some more events
				if (state == State.Puppet) {
					onLoseBalanceFromPuppet.Trigger(puppetMaster);
					if (onLoseBalanceFromPuppet.switchBehaviour) {
						state = State.Unpinned;
						return;
					}
				} else {
					onLoseBalanceFromGetUp.Trigger(puppetMaster);
					if (onLoseBalanceFromGetUp.switchBehaviour) {
						state = State.Unpinned;
						return;
					}
				}

				// Unpin the muscles. This is done after the events in case behaviours are switched and the next behaviour might need the weights as they were
				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp = 0f;
					m.state.muscleWeightMlp = unpinnedMuscleWeightMlp;
				}
				
				break;
				
			// Switching to the GETUP state
			case State.GetUp:
				getUpTimer = 0f;

				// Is the ragdoll facing up or down?
				bool isProne = IsProne();
				state = State.GetUp;

				// Trigger events
				if (isProne) {
					onGetUpProne.Trigger(puppetMaster);
					if (onGetUpProne.switchBehaviour) return;
				} else {
					onGetUpSupine.Trigger(puppetMaster);
					if (onGetUpSupine.switchBehaviour) return;
				}
				
				// Unpin the puppet just in case
				foreach (Muscle m in puppetMaster.muscles) {
					m.state.muscleWeightMlp = 0f;
					m.state.pinWeightMlp = 0f;
					m.state.muscleDamperAdd = 0f;
					
					var props = GetProps(m.props.group);

					// Change physic materials
					foreach (Collider c in m.colliders) {
						c.material = props.puppetMaterial != null? props.puppetMaterial: defaults.puppetMaterial;

						// Disable colliders
						if (props.disableColliders) c.enabled = false;
					}
				}
				
				// Set the target's rotation
				Vector3 spineDirection = puppetMaster.muscles[0].rigidbody.rotation * hipsUp;
				Vector3 normal = puppetMaster.targetRoot.up;
				Vector3.OrthoNormalize(ref normal, ref spineDirection);
				puppetMaster.targetRoot.rotation = Quaternion.LookRotation((isProne? spineDirection: -spineDirection), puppetMaster.targetRoot.up);

				// Set the target's position
				puppetMaster.SampleTargetMappedState();
				Vector3 getUpOffset = isProne? getUpOffsetProne: getUpOffsetSupine;
				puppetMaster.targetRoot.position = puppetMaster.muscles[0].rigidbody.position + puppetMaster.targetRoot.rotation * getUpOffset;
				GroundTarget();
				
				getupAnimationBlendWeight = 1f;

				break;
			}

			// Apply the new puppet state
			state = newState;
		}
	}
}
