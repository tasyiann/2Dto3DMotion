using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// This behaviour handles pinning and unpinning puppets when they collide with objects or are hit via code, also automates getting up from an unbalanced state.
	/// </summary>
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page10.html")]
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourPuppet")]
	public partial class BehaviourPuppet : BehaviourBase {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		void OpenUserManual() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page10.html");
		}

		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		void OpenScriptReference() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_behaviour_puppet.html");
		}

		#region Main Properties

		/// <summary>
		/// Puppet means the character is in normal state and pinned. Unpinned means the character has lost balance and is animated physically in muscle space only. GetUp is a transition state from Unpinned to Puppet.
		/// </summary>
		[System.Serializable]
		public enum State {
			Puppet,
			Unpinned,
			GetUp
		}

		/// <summary>
		/// Defines the properties of muscle behaviour.
		/// </summary>
		[System.Serializable]
		public struct MuscleProps {
			[TooltipAttribute("How much will collisions with muscles of this group unpin parent muscles?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin parent muscles?
			/// </summary>
			[Range(0f, 1f)] public float unpinParents;

			[TooltipAttribute("How much will collisions with muscles of this group unpin child muscles?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin child muscles?
			/// </summary>
			[Range(0f, 1f)] public float unpinChildren;

			[TooltipAttribute("How much will collisions with muscles of this group unpin muscles of the same group?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin muscles of the same group?
			/// </summary>
			[Range(0f, 1f)] public float unpinGroup;

			[TooltipAttribute("If 1, muscles of this group will always be mapped to the ragdoll.")]
			/// <summary>
			/// If 1, muscles of this group will always be mapped to the ragdoll.
			/// </summary>
			[Range(0f, 1f)] public float minMappingWeight;

			[TooltipAttribute("If 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.")]
			/// <summary>
			/// If 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.
			/// </summary>
			[Range(0f, 1f)] public float maxMappingWeight;

			[TooltipAttribute("If true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).")]
			/// <summary>
			/// If true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).
			/// </summary>
			public bool disableColliders;

			[TooltipAttribute("How fast will muscles of this group regain their pin weight (multiplier)?")]
			/// <summary>
			/// How fast will muscles of this group regain their pin weight (multiplier)?
			/// </summary>
			public float regainPinSpeed;

			[TooltipAttribute("Smaller value means more unpinning from collisions (multiplier).")]
			/// <summary>
			/// Smaller value means more unpinning from collisions (multiplier).
			/// </summary>
			public float collisionResistance;

			[TooltipAttribute("If the distance from the muscle to it's target is larger than this value, the character will be knocked out.")]
			/// <summary>
			/// If the distance from the muscle to it's target is larger than this value, the character will be knocked out.
			/// </summary>
			public float knockOutDistance;

			[Tooltip("The PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.")]
			/// <summary>
			/// The PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.
			/// </summary>
			public PhysicMaterial puppetMaterial;
			
			[Tooltip("The PhysicsMaterial applied to the muscles while the character is in Unpinned state.")]
			/// <summary>
			/// The PhysicsMaterial applied to the muscles while the character is in Unpinned state.
			/// </summary>
			public PhysicMaterial unpinnedMaterial;
		}

		/// <summary>
		/// Defines the properties of muscle behaviour for certain muscle group(s).
		/// </summary>
		[System.Serializable]
		public struct MusclePropsGroup {
			[TooltipAttribute("Muscle groups to which those properties apply.")]
			/// <summary>
			/// Muscle groups to which those properties apply.
			/// </summary>
			public Muscle.Group[] groups;

			[TooltipAttribute("The muscle properties for those muscle groups.")]
			/// <summary>
			/// The muscle properties for those muscle groups.
			/// </summary>
			public MuscleProps props;
		}

		/// <summary>
		/// Multiplies collision resistance for the specified layers.
		/// </summary>
		[System.Serializable]
		public struct CollisionResistanceMultiplier {
			public LayerMask layers;
			public float multiplier;
		}

		[Header("Master Properties")]

		[Tooltip("Will ground the target to those layers when getting up.")]
		/// <summary>
		/// Will ground the target to those layers when getting up.
		/// </summary>
		public LayerMask groundLayers;

		[Tooltip("Will unpin the muscles that collide with those layers.")]
		/// <summary>
		/// Will unpin the muscles that collide with those layers.
		/// </summary>
		public LayerMask collisionLayers;
		
		[Tooltip("The collision impulse sqrMagnitude threshold under which collisions will be ignored.")]
		/// <summary>
		/// The collision impulse sqrMagnitude threshold under which collisions will be ignored.
		/// </summary>
		public float collisionThreshold;

		/// <summary>
		/// Smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.
		/// </summary>
		public Weight collisionResistance = new Weight(3f, "Smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.");

		[Tooltip("Multiplies collision resistance for the specified layers.")]
		/// <summary>
		/// Multiplies collision resistance for the specified layers.
		/// </summary>
		public CollisionResistanceMultiplier[] collisionResistanceMultipliers;

		[Tooltip("If the distance from the muscle to it's target is larger than this value, the character will be knocked out.")]
		/// <summary>
		/// If the distance from the muscle to it's target is larger than this value, the character will be knocked out.
		/// </summary>
		[Range(0.001f, 10f)] public float knockOutDistance = 1f;

		[Tooltip("How fast will the muscles of this group regain their pin weight?")]
		/// <summary>
		/// How fast will the muscles of this group regain their pin weight?
		/// </summary>
		[Range(0.001f, 10f)] public float regainPinSpeed = 1f;

		[Tooltip("Smaller value makes the muscles weaker when the puppet is knocked out.")]
		/// <summary>
		/// Smaller value makes the muscles weaker when the puppet is knocked out.
		/// </summary>
		[Range(0f, 1f)] public float unpinnedMuscleWeightMlp = 0.3f;

		[Tooltip("Muscle weight multiplier relative to pin weight. It can be used to make muscles weaker/stronger when they are more/less unpinned while in the normal Puppet state.")]
		/// <summary>
		/// Muscle weight multiplier relative to pin weight. It can be used to make muscles weaker/stronger when they are more/less unpinned while in the normal Puppet state.
		/// </summary>
		public AnimationCurve muscleRelativeToPinWeight;

		[Tooltip("'Boosting' is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.")]
		/// <summary>
		/// 'Boosting' is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.
		/// </summary>
		public float boostFalloff = 1f;

		[Header("Muscle Group Properties")]

		[Tooltip("The default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.")]
		/// <summary>
		/// The default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.
		/// </summary>
		public MuscleProps defaults;

		[Tooltip("Overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).")]
		/// <summary>
		/// Overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).
		/// </summary>
		public MusclePropsGroup[] groupOverrides;

		[Header("Losing Balance and Getting Up")]

		[Tooltip("If true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.")]
		/// <summary>
		/// If true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.
		/// </summary>
		public bool canGetUp = true;

		[Tooltip("Minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.")]
		/// <summary>
		/// Minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.
		/// </summary>
		public float getUpDelay = 5f;

		[Tooltip("The duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.")]
		/// <summary>
		/// The duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.
		/// </summary>
		public float blendToAnimationTime = 0.2f;

		[Tooltip("Will not get up before the velocity of the hip muscle has come down to this value.")]
		/// <summary>
		/// Will not get up before the velocity of the hip muscle has come down to this value.
		/// </summary>
		public float maxGetUpVelocity = 0.3f;

		[Tooltip("Will not get up before this amount of time has passed since loosing balance.")]
		/// <summary>
		/// Will not get up before this amount of time has passed since loosing balance.
		/// </summary>
		public float minGetUpDuration = 1f;

		[Tooltip("Collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpCollisionResistanceMlp = 2f;

		[Tooltip("Regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpRegainPinSpeedMlp = 2f;

		[Tooltip("Knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpKnockOutDistanceMlp = 10f;

		[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.")]
		/// <summary>
		/// Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.
		/// </summary>
		public Vector3 getUpOffsetProne;

		[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.")]
		/// <summary>
		/// Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.
		/// </summary>
		public Vector3 getUpOffsetSupine;

		[Tooltip("If true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.")]
		/// <summary>
		/// If true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.
		/// </summary>
		public bool dropProps;

		[Header("Activation")]

		[Tooltip("If true, will set the PuppetMaster.mode to Active when any of the muscles collides with something (static colliders are ignored if 'Activate On Static Collisions' is false) or is hit.")]
		/// <summary>
		/// If true, will set the PuppetMaster.mode to Active when any of the muscles collides with something (static colliders are ignored if 'Activate On Static Collisions' is false) or is hit.
		/// </summary>
		public bool canActivate = true;

		[Tooltip("If false, static colliders will not activate the puppet when they collide with the muscles. Note that the static colliders need to have a kinematic Rigidbody attached for this to work.")]
		/// <summary>
		/// If false, static colliders will not activate the puppet when they collide with the muscles. Note that the static colliders need to have a kinematic Rigidbody attached for this to work.
		/// </summary>
		public bool activateOnStaticCollisions = false;

		[Tooltip("Minimum collision impulse for activating the puppet.")]
		/// <summary>
		/// Minimum collision impulse for activating the puppet.
		/// </summary>
		public float activateOnImpulse = 0f;

		[Tooltip("If true, the PuppetMaster.mode will be set to Kinematic when all muscles are pinned and the puppet is in normal Puppet state. This will increase performance and enable you to have full animation accuracy when PuppetMaster is not needed. Note that collision events are only broadcasted if one of the Rigidbodies is not kinematic, so if you need 2 characters to wake up each other, one has to be active. 'Can Activate' should be true if you wish to reactivate automatically when the puppet collides with somethinf or is hit.")]
		/// <summary>
		/// If true, the PuppetMaster.mode will be set to Kinematic when all muscles are pinned and the puppet is in normal Puppet state. This will increase performance and enable you to have full animation accuracy when PuppetMaster is not needed. Note that collision events are only broadcasted if one of the Rigidbodies is not kinematic, so if you need 2 characters to wake up each other, one has to be active. 'Can Activate' should be true if you wish to reactivate automatically when the puppet collides with somethinf or is hit.
		/// </summary>
		public bool deactivateAutomatically = true;

		[Tooltip("If true, mapping is blended in only if the puppet becomes in contact with something and blended out again to maintain 100% animation accuracy for the rest of the time.")]
		/// <summary>
		/// If true, mapping is blended in only if the puppet becomes in contact with something and blended out again to maintain 100% animation accuracy for the rest of the time.
		/// </summary>
		public bool mapOnlyOnContact = true;

		[Tooltip("The speed of blending in mapping in case of contact.")]
		/// <summary>
		/// "The speed of blending in mapping in case of contact."
		/// </summary>
		public float mappingBlendSpeed = 5f;

		[Header("Events")]

		[Tooltip("Called when the character starts getting up from a prone pose (facing down).")]
		/// <summary>
		/// Called when the character starts getting up from a prone pose (facing down).
		/// </summary>
		public PuppetEvent onGetUpProne;

		[Tooltip("Called when the character starts getting up from a supine pose (facing up).")]
		/// <summary>
		/// Called when the character starts getting up from a supine pose (facing up).
		/// </summary>
		public PuppetEvent onGetUpSupine;

		[Tooltip("Called when the character is knocked out (loses balance). Doesn't matter from which state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance). Doesn't matter from which state.
		/// </summary>
		public PuppetEvent onLoseBalance;

		[Tooltip("Called when the character is knocked out (loses balance) only from the normal Puppet state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance) only from the normal Puppet state.
		/// </summary>
		public PuppetEvent onLoseBalanceFromPuppet;

		[Tooltip("Called when the character is knocked out (loses balance) only from the GetUp state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance) only from the GetUp state.
		/// </summary>
		public PuppetEvent onLoseBalanceFromGetUp;

		[Tooltip("Called when the character has fully recovered and switched to the Puppet state.")]
		/// <summary>
		/// Called when the character has fully recovered and switched to the Puppet state.
		/// </summary>
		public PuppetEvent onRegainBalance;

		public delegate void CollisionImpulseDelegate(MuscleCollision m, float impulse);

		/// <summary>
		/// Called when any of the puppet's muscles has had a collision.
		/// </summary>
		public CollisionImpulseDelegate OnCollisionImpulse;

		/// <summary>
		/// Gets the current state of the puppet (Puppet/Unpinned/GetUp).
		/// </summary>
		public State state { get; private set; }

		/// <summary>
		/// Returns true if any of the puppet's muscles have their mapping weight multiplier > 0.
		/// </summary>
		public bool isMapped { get; private set; }

		/// <summary>
		/// Resets this puppet to the specified position and rotation and normal Puppet state. Use this for respawning existing puppets.
		/// </summary>
		public void Reset(Vector3 position, Quaternion rotation) {
			if (!Application.isPlaying) return;

			foreach (Muscle m in puppetMaster.muscles) {
				m.DisableColliders();
			}
			
			puppetMaster.targetRoot.position = position;
			puppetMaster.targetRoot.rotation = rotation;
			
			foreach (Muscle m in puppetMaster.muscles) {
				m.transform.position = m.target.position;
				m.transform.rotation = m.target.rotation * m.targetRotationRelative;
			}
			
			puppetMaster.StoreTargetMappedState();
			puppetMaster.SampleTargetMappedState();
			
			foreach (Muscle m in puppetMaster.muscles) {
				m.Read();
			}
			
			StartCoroutine(EnableColliders());
			
			SetState(State.Puppet);
		}

		#endregion Main Properties

		private float unpinnedTimer;
		private float getUpTimer;
		private Vector3 hipsForward;
		private Vector3 hipsUp;
		private float getupAnimationBlendWeight, getupAnimationBlendWeightV;

		protected override void OnInitiate() {
			foreach (CollisionResistanceMultiplier crm in collisionResistanceMultipliers) {
				if (crm.layers == 0) {
					Debug.LogWarning("BehaviourPuppet has a Collision Resistance Multiplier that's layers is set to Nothing. Please add some layers.", transform);
				}
			}

			hipsForward = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.forward;
			hipsUp = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.up;
			
			state = State.Unpinned;
			SetState(State.Puppet);

			Activate();
		}

		protected override void OnActivate() {
			puppetMaster.broadcastGroundCollisions = false;
		}
		
		protected override void OnDeactivate() {
			puppetMaster.broadcastCollisions = true;
		}

		void OnDisable() {
			state = State.Unpinned;
		}

		protected override void OnFixedUpdate() {
			// If the PuppetMaster is not active, make sure the puppet is in the Puppet state and return.
			if (!puppetMaster.isActive) {

				SetState(State.Puppet);
				return;
			}

			// Boosting falloff
			foreach (Muscle m in puppetMaster.muscles) {
				m.state.immunity = Mathf.MoveTowards(m.state.immunity, 0f, Time.deltaTime * boostFalloff);
				m.state.impulseMlp = Mathf.Lerp(m.state.impulseMlp, 1f, Time.deltaTime * boostFalloff);
			}

			// Getting up and making sure the puppet stays unpinned and mapped
			if (state == State.Unpinned) {
				unpinnedTimer += Time.deltaTime;

				if (unpinnedTimer >= getUpDelay && canGetUp && puppetMaster.muscles[0].rigidbody.velocity.magnitude < maxGetUpVelocity) {
					SetState(State.GetUp);
					return;
				}

				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp = 0f;
					m.state.mappingWeightMlp = Mathf.MoveTowards(m.state.mappingWeightMlp, 1f, Time.deltaTime * 5f);
				}
			}

			// In PUPPET and GETUP states...
			if (state != State.Unpinned) {
				foreach (Muscle m in puppetMaster.muscles) {
					var props = GetProps(m.props.group);

					float inversePinWeightMlp = 1f - m.state.pinWeightMlp;
					inversePinWeightMlp *= inversePinWeightMlp;

					float speedF = 1f;
					if (state == State.GetUp) speedF = Mathf.Lerp(getUpRegainPinSpeedMlp, 1f, m.state.pinWeightMlp);

					m.state.pinWeightMlp += Time.deltaTime * props.regainPinSpeed * regainPinSpeed * speedF;

					if (m.props.pinWeight > 0f) {
						float stateMlp = 1f;
						if (state == State.GetUp && getUpTimer < 0.5f) {
							stateMlp = Mathf.Lerp(getUpKnockOutDistanceMlp, stateMlp, m.state.pinWeightMlp);
						}

						float velF = Mathf.Clamp(m.rigidbody.velocity.magnitude, 0.5f, 2f);

						if (m.state.pinWeightMlp < 0.5f && m.positionOffset.magnitude * m.props.pinWeight > props.knockOutDistance * knockOutDistance * stateMlp * velF) {
							SetState(State.Unpinned);
							return;
						}
					}

					m.state.muscleWeightMlp = muscleRelativeToPinWeight.Evaluate(m.state.pinWeightMlp);

					if (state == State.GetUp) m.state.muscleDamperAdd = 0f;
				}

				// Max pin weight from the legs and feet
				float maxPinWeight = 1f;
				foreach (Muscle m in puppetMaster.muscles) {
					if (m.props.group == Muscle.Group.Leg || m.props.group == Muscle.Group.Foot) {
						if (m.state.pinWeightMlp < maxPinWeight) maxPinWeight = m.state.pinWeightMlp;
					}
				}
				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp = Mathf.Clamp(m.state.pinWeightMlp, 0, maxPinWeight * 5f);
				}
			}

			// Ending GetUp
			if (state == State.GetUp) {
				getUpTimer += Time.deltaTime;

				if (getUpTimer > minGetUpDuration) {
					getUpTimer = 0f;
					SetState(State.Puppet);
				}
			}
		}

		protected override void OnLateUpdate() {
			if (!puppetMaster.isActive) return;

			if (state == State.Unpinned) {
				puppetMaster.targetRoot.position += puppetMaster.muscles[0].transform.position - puppetMaster.muscles[0].target.position;
				GroundTarget();
			}

			// Dynamic mapping
			BlendMappingRecursive(puppetMaster.muscles[0], false);

			isMapped = IsMapped();
			
			if (!isMapped && deactivateAutomatically) puppetMaster.mode = PuppetMaster.Mode.Kinematic;
		}

		// Called when the PuppetMaster reads
		protected override void OnReadBehaviour() {
			if (!enabled) return;


			getupAnimationBlendWeight = Mathf.SmoothDamp(getupAnimationBlendWeight, 0f, ref getupAnimationBlendWeightV, blendToAnimationTime);
			if (getupAnimationBlendWeight < 0.01f) getupAnimationBlendWeight = 0f;
			
			// Lerps the target pose to last sampled mapped pose. Starting off from the ragdoll pose
			puppetMaster.FixTargetToSampledState(getupAnimationBlendWeight);
		}

		private void BlendMappingRecursive(Muscle muscle, bool to) {
			BlendMapping(muscle, to);
			
			for (int i = 0; i < muscle.childIndexes.Length; i++) {
				BlendMapping(puppetMaster.muscles[muscle.childIndexes[i]], to);
			}
		}

		private void BlendMapping(Muscle muscle, bool to) {
			if (!mapOnlyOnContact) to = true;
			if (muscle.state.pinWeightMlp < 1) to = true;
			var props = GetProps(muscle.props.group);
			
			float speed = to? mappingBlendSpeed * 3f: mappingBlendSpeed;
			float target = to? (state == State.Puppet? props.maxMappingWeight: 1f): props.minMappingWeight;
			
			muscle.state.mappingWeightMlp = Mathf.MoveTowards(muscle.state.mappingWeightMlp, target, Time.deltaTime * speed);
		}
		
		private bool Activate(Collision collision, float impulse) {
			if (!canActivate) return false;
			if (impulse < activateOnImpulse) return false;
			
			if (collision.gameObject.isStatic) {
				return activateOnStaticCollisions;
			}
			
			return true;
		}

		private IEnumerator EnableColliders() {
			yield return null;
			
			foreach (Muscle m in puppetMaster.muscles) {
				m.EnableColliders();
			}
		}
		
		private void GroundTarget() {
			Ray ray = new Ray(puppetMaster.targetRoot.position + Vector3.up * 2f, Vector3.down);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 2f, groundLayers)) {
				puppetMaster.targetRoot.position = hit.point;
			}
		}
	}
}
