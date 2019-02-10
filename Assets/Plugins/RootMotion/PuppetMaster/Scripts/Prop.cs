using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Automated prop picking up/dropping for the PuppetMaster.
	/// </summary>
	public abstract class Prop : MonoBehaviour {

		#region User Interface

		[Tooltip("This has no other purpose but helping you distinguish props by PropRoot.currentProp.propType.")]
		/// <summary>
		/// This has no other purpose but helping you distinguish PropRoot.currentProp by type.
		/// </summary>
		public int propType;

		[Tooltip("The Muscle of this prop.")]
		/// <summary>
		/// The Muscle of this prop.
		/// </summary>
		public ConfigurableJoint muscle;

		[Tooltip("Optinal additional pin, useful for long melee weapons that would otherwise require a lot of muscle force and solver iterations to be swinged quickly. Should normally be without any colliders attached.")]
		/// <summary>
		/// Optinal additional pin, useful for long melee weapons that would otherwise require a lot of muscle force and solver iterations to be swinged quickly. Should normally be without any colliders attached.
		/// </summary>
		public ConfigurableJoint additionalPin;

		[Tooltip("Target Transform for the additional pin.")]
		/// <summary>
		/// Target Transform for the additional pin.
		/// </summary>
		public Transform additionalPinTarget;

		[Tooltip("The muscle properties that will be applied to the Muscle on pickup.")]
		/// <summary>
		/// The muscle properties that will be applied to the Muscle.
		/// </summary>
		public Muscle.Props muscleProps = new Muscle.Props();

		[Tooltip("The muscle properties that will be applied to the additional pin Muscle on pickup.")]
		/// <summary>
		/// The muscle properties that will be applied to the additional pin Muscle.
		/// </summary>
		public Muscle.Props additionalPinProps = new Muscle.Props();

		/// <summary>
		/// Is this prop picked up and connected to a PropRoot?
		/// </summary>
		public bool isPickedUp { get { return propRoot != null; }}

		/// <summary>
		/// Returns the PropRoot that this prop is connected to if it is picked up. If this returns null, the prop is not picked up.
		/// </summary>
		public PropRoot propRoot { get; private set; }

		#endregion User Interface

		// Picking up/dropping props is done by simply changing PropRoot.currentProp
		public void PickUp(PropRoot propRoot) {
			muscle.xMotion = xMotion;
			muscle.yMotion = yMotion;
			muscle.zMotion = zMotion;
			muscle.angularXMotion = angularXMotion;
			muscle.angularYMotion = angularYMotion;
			muscle.angularZMotion = angularZMotion;
			
			this.propRoot = propRoot;

			muscle.gameObject.layer = propRoot.puppetMaster.gameObject.layer;
			foreach (Collider c in colliders) {
				if (!c.isTrigger) c.gameObject.layer = muscle.gameObject.layer;
			}

			OnPickUp(propRoot);
		}

		// Picking up/dropping props is done by simply changing PropRoot.currentProp
		public void Drop() {
			this.propRoot = null;

			OnDrop();
		}

		public void StartPickedUp(PropRoot propRoot) {
			this.propRoot = propRoot;
		}

		protected virtual void OnPickUp(PropRoot propRoot) {}
		protected virtual void OnDrop() {}
		protected virtual void OnStart() {}
	
		private ConfigurableJointMotion xMotion, yMotion, zMotion, angularXMotion, angularYMotion, angularZMotion;
		private Collider[] colliders = new Collider[0];

		protected virtual void Awake() {
			if (transform.position != muscle.transform.position) {
				Debug.LogError("Prop target position must match exactly with it's muscle's position!", transform);
			}

			xMotion = muscle.xMotion;
			yMotion = muscle.yMotion;
			zMotion = muscle.zMotion;
			angularXMotion = muscle.angularXMotion;
			angularYMotion = muscle.angularYMotion;
			angularZMotion = muscle.angularZMotion;

			colliders = muscle.GetComponentsInChildren<Collider>();
		}

		void Start() {
			if (!isPickedUp) ReleaseJoint();

			OnStart();
		}

		private void ReleaseJoint() {
			muscle.connectedBody = null;
			muscle.targetRotation = Quaternion.identity;
			
			JointDrive j = new JointDrive();
			j.mode = JointDriveMode.None;
			muscle.slerpDrive = j;
			
			muscle.xMotion = ConfigurableJointMotion.Free;
			muscle.yMotion = ConfigurableJointMotion.Free;
			muscle.zMotion = ConfigurableJointMotion.Free;
			muscle.angularXMotion = ConfigurableJointMotion.Free;
			muscle.angularYMotion = ConfigurableJointMotion.Free;
			muscle.angularZMotion = ConfigurableJointMotion.Free;
		}

		// Just making sure this GameObject's position and rotation matches with the muscle's in the Editor.
		void OnDrawGizmosSelected() {
			if (muscle == null) return;
			if (Application.isPlaying) return;

			transform.position = muscle.transform.position;
			transform.rotation = muscle.transform.rotation;

			muscleProps.group = Muscle.Group.Prop;
			additionalPinProps.group = Muscle.Group.Prop;
		}
	}
}
