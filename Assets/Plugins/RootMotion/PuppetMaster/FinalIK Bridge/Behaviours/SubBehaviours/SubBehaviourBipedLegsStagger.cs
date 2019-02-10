using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBipedLegsStagger : SubBehaviourBase {
		
		[Range(0.0001f, 10f)] public float footMassMlp = 2f;
		[Range(1f, 100f)] public float footInertiaTensorMlp = 1f;
		public float stepAngle = 3f;
		public AnimationCurve stepHeight;
		public Weight muscleWeightThigh = new Weight(5f);
		public Weight muscleWeightCalf = new Weight(5f);
		public Weight muscleWeightFoot = new Weight(1f);
		public Weight stepHeightOffset = new Weight(0.05f);
		public Weight stepOffsetMlp = new Weight(3f);
		public Weight stepDelay = new Weight(0.1f);
		public Weight legOffsetMlp = new Weight(0.2f);
		public Weight stepThreshold = new Weight(0.4f);
		public Weight maxStepDistance = new Weight(0.55f);
		public float footPinWeight;
		public float correctionY = 1f;
		public float correctionXZ;
		public float correctionVelocity = 0.01f;

		private IKSolverFullBodyBiped solver;
		private SubBehaviourCOM centerOfMass;
		private Leg[] legs;
		private int stepLegIndex;
		private float groundYOffset;
		private float groundYOffsetTarget;
		private float groundYOffsetVel;
		private Ray ray;
		private LayerMask groundLayers;

		public void Initiate(BehaviourBase behaviour, SubBehaviourCOM centerOfMass, LayerMask groundLayers, IKSolverFullBodyBiped solver) {
			this.behaviour = behaviour;
			this.centerOfMass = centerOfMass;
			this.groundLayers = groundLayers;
			this.solver = solver;

			behaviour.OnPreActivate += OnPreActivate;
			behaviour.OnPostRead += OnPostRead;
			behaviour.OnPreDisable += OnPreDisable;
			behaviour.OnPostDrawGizmos += OnPostDrawGizmos;

			legs = new Leg[2] {
				new Leg(solver, behaviour.puppetMaster, FullBodyBipedEffector.LeftFoot),
				new Leg(solver, behaviour.puppetMaster, FullBodyBipedEffector.RightFoot)
			};
		}
		
		#region Behaviour Delegates

		private void OnPreActivate() {
			foreach (Leg leg in legs) leg.OnActivate(footMassMlp, footInertiaTensorMlp);
		}
		
		private void OnPostRead() {
			Stepping();
		}

		private void OnPreDisable() {
			groundYOffset = 0f;
			groundYOffsetTarget = 0f;
			
			foreach (Leg leg in legs) leg.OnDisable();
		}

		private void OnPostDrawGizmos() {
			foreach (Leg leg in legs) {
				Gizmos.DrawSphere(leg.position, 0.05f);
			}
		}
		
		#endregion Behaviour Delegates

		#region Leg

		public class Leg {
			public Vector3 stepTo { get; private set; }
			public Vector3 position { get; private set; }
			public Vector3 offset { get; private set; }
			public FBIKChain chain { get; private set; }
			public Muscle muscle { get; private set; }
			
			private float stepDelay;
			private Vector3 stepFrom;
			private IKSolverFullBodyBiped solver;
			private IKEffector effector;
			private float stepTimer;
			private float stepLength;
			private Muscle thighMuscle;

			private float mass;
			private Vector3 inertiaTensor;
			
			public bool isStepping {
				get {
					return stepTimer < stepLength + stepDelay;
				}
			}
			
			public Leg(IKSolverFullBodyBiped solver, PuppetMaster puppetMaster, FullBodyBipedEffector effectorType) {
				this.solver = solver;
				effector = solver.GetEffector(effectorType);
				chain = solver.GetChain(effectorType);
				
				stepFrom = effector.bone.position;
				stepTo = effector.bone.position;
				position = effector.bone.position;
				
				offset = effectorType == FullBodyBipedEffector.LeftFoot? Vector3.left: Vector3.right;
				
				stepTimer = 0f;
				stepLength = 0f;
				
				muscle = puppetMaster.GetMuscle(effector.bone);
				thighMuscle = puppetMaster.GetMuscle(chain.nodes[0].transform);
				
				mass = muscle.rigidbody.mass;
				inertiaTensor = muscle.rigidbody.inertiaTensor;
			}
			
			public void OnActivate(float footMassMlp, float footInertiaTensorMlp) {
				stepTimer = 0f;
				stepLength = 0f;
				position = effector.bone.position;
				
				muscle.rigidbody.mass = mass * footMassMlp;
				muscle.rigidbody.inertiaTensor = inertiaTensor * footInertiaTensorMlp;
			}
			
			public void Step(Vector3 stepTo, float stepLength, float legOffsetMlp, float stepDelay) {
				this.stepFrom = position;
				this.stepLength = stepLength;
				this.stepDelay = stepDelay * Mathf.Lerp(UnityEngine.Random.value, 1, 0.5f);
				stepTimer = 0f;
				
				this.stepTo = stepTo;
			}
			
			public void Update(AnimationCurve stepHeight, float heightOffset, float pinWeight, float correctionY, float correctionXZ, float correctionVelocity) {
				stepTimer += Time.deltaTime;
				
				bool step = stepLength > 0 && stepTimer < stepLength;
				
				if (!step) {
					effector.positionOffset += position - effector.bone.position;
					muscle.state.pinWeightMlp = 0f;
					return;
				}
				
				//float legLength = solver.GetChain(effectorType).nodes[0].length + solver.GetChain(effectorType).nodes[1].length;
				
				if (Vector3.Distance(position, thighMuscle.rigidbody.position) >= 0.5f) {
					muscle.state.pinWeightMlp = 0f;
				}
				
				position = Vector3.Lerp(stepFrom, stepTo, stepTimer / stepLength);
				position += solver.GetRoot().up * (stepHeight.Evaluate(stepTimer) + heightOffset);
				
				if (correctionY != 0f) {
					float realYOffset = position.y - muscle.rigidbody.position.y - muscle.rigidbody.velocity.y * correctionVelocity;
					if (realYOffset > 0f) position += Vector3.up * realYOffset * correctionY;
				}
				
				if (correctionXZ != 0f) {
					Vector3 realXZOffset = position - muscle.rigidbody.position - muscle.rigidbody.velocity * correctionVelocity;
					realXZOffset.y = 0f;
					position += realXZOffset * correctionXZ;
				}
				
				effector.positionOffset += position - effector.bone.position;
				
				muscle.state.pinWeightMlp = pinWeight;
			}
			
			public void OnDisable() {
				muscle.state.pinWeightMlp = 0f;
				
				muscle.rigidbody.mass = mass;
				muscle.rigidbody.inertiaTensor = inertiaTensor;
			}
		}

		#endregion Leg

		private bool CanStep() {
			// @todo check if leg is grounded
			
			if (stepHeight.length < 1) return false;
			if (centerOfMass.angle < stepAngle) return false;
			
			foreach (Leg leg in legs) {
				if (leg.isStepping) return false;
			}
			return true;
		}
		
		private int GetStepLegIndex() {
			if (!CanStep()) return -1;
			
			float maxMag = 0f;
			int index = -1;
			
			for (int i = 0; i < legs.Length; i++) {
				Vector3 toCOM = centerOfMass.position - legs[i].position;
				toCOM.y = 0f;
				
				float mag = toCOM.sqrMagnitude;
				if (mag > maxMag) {
					index = i;
					maxMag = mag;
				}
			}
			
			return index;
		}
		
		private int OtherLegIndex(int legIndex) {
			if (legIndex == 0) return 1;
			return 0;
		}
		
		void Stepping() {
			stepLegIndex = GetStepLegIndex();
			
			if (stepLegIndex != -1) {;
				RaycastHit hit;
				
				Vector3 center = behaviour.puppetMaster.muscles[5].rigidbody.position;
				
				Vector3 velocity = behaviour.puppetMaster.muscles[5].rigidbody.velocity;
				velocity.y = 0f;
				Vector3 position = center + velocity * stepOffsetMlp.GetValue(centerOfMass.angle);
				
				// Add Offset
				Vector3 stepDirection = position - legs[stepLegIndex].position;
				stepDirection.y = 0f;
				position += Quaternion.LookRotation(stepDirection) * legs[stepLegIndex].offset;
				
				// Avoid the other leg
				int otherLegIndex = OtherLegIndex(stepLegIndex);
				Vector3 otherPosition = legs[otherLegIndex].position;
				
				float y = position.y;
				Vector3 toPosition = Flatten(position - legs[stepLegIndex].muscle.rigidbody.position).normalized;
				Vector3 toOtherLeg = Flatten(otherPosition - legs[stepLegIndex].muscle.rigidbody.position).normalized;
				float dot = Mathf.Max(Vector3.Dot(toPosition, toOtherLeg), 0f);
				
				float angle = dot * 20f;
				if (stepLegIndex == 1) angle = -angle;
				
				Quaternion rotation = Quaternion.AngleAxis(angle, solver.GetRoot().up);
				position = SetY(legs[stepLegIndex].muscle.rigidbody.position + rotation * toPosition, y);
				
				// Clamp distance
				Vector3 pelvisToPosition = Flatten(position - behaviour.puppetMaster.muscles[0].rigidbody.position);
				pelvisToPosition = Vector3.ClampMagnitude(pelvisToPosition, maxStepDistance.GetValue(centerOfMass.angle));
				position = behaviour.puppetMaster.muscles[0].rigidbody.position + pelvisToPosition;
				
				ray = new Ray(position, Vector3.down);
				
				if (Physics.Raycast(ray, out hit, 2f, groundLayers)) {
					Vector3 stepTo = hit.point + Vector3.up * stepHeightOffset.GetValue(centerOfMass.angle);
					
					groundYOffsetTarget = Mathf.Min(hit.point.y - behaviour.puppetMaster.targetRoot.position.y, groundYOffsetTarget);
					//groundYOffsetTarget = hit.point.y - solver.GetRoot().position.y;
					
					if (Vector3.Distance(stepTo, legs[stepLegIndex].position) > stepThreshold.GetValue(centerOfMass.angle)) {
						float stepLength = stepHeight[stepHeight.length - 1].time;
						
						legs[stepLegIndex].Step(stepTo, stepLength, legOffsetMlp.GetValue(centerOfMass.angle), stepDelay.GetValue(centerOfMass.angle));
					}
				}
			}
			
			groundYOffset = Mathf.SmoothDamp(groundYOffset, groundYOffsetTarget, ref groundYOffsetVel, 0.2f);
			behaviour.puppetMaster.muscles[0].target.position += solver.GetRoot().up * groundYOffset;
			
			Debug.DrawRay(ray.origin, ray.direction);
			
			foreach (Leg leg in legs) {
				
				leg.Update(stepHeight, stepHeightOffset.GetValue(centerOfMass.angle), footPinWeight, correctionY, correctionXZ, correctionVelocity);
				
				behaviour.puppetMaster.GetMuscle(leg.chain.nodes[0].transform).state.muscleWeightMlp = muscleWeightThigh.GetValue(centerOfMass.angle);
				behaviour.puppetMaster.GetMuscle(leg.chain.nodes[1].transform).state.muscleWeightMlp = muscleWeightCalf.GetValue(centerOfMass.angle);
				behaviour.puppetMaster.GetMuscle(leg.chain.nodes[2].transform).state.muscleWeightMlp = muscleWeightFoot.GetValue(centerOfMass.angle);
			}
		}
	}
}
