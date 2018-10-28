using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	// An example of extending the Prop class to create some additional custom functionalities.
	public class PropMelee: Prop {

		public CapsuleCollider capsuleCollider;
		public BoxCollider boxCollider;
		public float actionColliderRadiusMlp = 1f;
		public float actionMassMlp = 1f;
		public Vector3 COMOffset;

		private float defaultColliderRadius;
		private float defaultMass;
		private Rigidbody r;

		public void StartAction(float duration) {
			StopAllCoroutines();
			StartCoroutine(EnableActionCollider(duration));
		}

		public IEnumerator EnableActionCollider(float duration) {
			capsuleCollider.radius = defaultColliderRadius * actionColliderRadiusMlp;
			r.mass = defaultMass * actionMassMlp;

			yield return new WaitForSeconds(duration);

			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;
		}
		
		protected override void OnStart() {
			// Initiate stuff here.
			defaultColliderRadius = capsuleCollider.radius;

			r = muscle.GetComponent<Rigidbody>();
			r.centerOfMass += COMOffset;
			defaultMass = r.mass;
		}
		
		protected override void OnPickUp(PropRoot propRoot) {
			// Called when the prop has been picked up and connected to a PropRoot.
			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;

			capsuleCollider.enabled = true;
			boxCollider.enabled = false;
		}
		
		protected override void OnDrop() {
			// Called when the prop has been dropped.
			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;

			capsuleCollider.enabled = false;
			boxCollider.enabled = true;
		}
	}
}
