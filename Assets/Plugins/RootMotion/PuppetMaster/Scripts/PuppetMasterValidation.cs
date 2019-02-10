using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Code for making sure if the PuppetMaster setup is valid.
	public partial class PuppetMaster: MonoBehaviour {

		/// <summary>
		/// Determines whether this PuppetMaster instance is valid for initiation.
		/// </summary>
		public bool IsValid(bool log) {
			if (muscles == null) {
				if (log) Debug.LogWarning("PuppetMaster Muscles is null.", transform);
				return false;
			}
			
			if (muscles.Length == 0) {
				if (log) Debug.LogWarning("PuppetMaster has no muscles.", transform);
				return false;
			}
			
			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i] == null) {
					if (log) Debug.LogWarning("Muscle is null, PuppetMaster muscle setup is invalid.", transform);
					return false;
				}
				
				if (!muscles[i].IsValid(log)) return false;
			}

			if (targetRoot.root != transform.root) {
				if (log) Debug.LogWarning("Target Root is not parented to the same root Transform as the PuppetMaster.", transform);
				return false;
			}

			if (targetRoot == null) {
				if (log) Debug.LogWarning("Invalid PuppetMaster setup. (targetRoot not found)", transform);
				return false;
			}

			for (int i = 0; i < muscles.Length; i++) {
				for (int c = 0; c < muscles.Length; c++) {
					if (i != c) {
						if (muscles[i] == muscles[c] || muscles[i].joint == muscles[c].joint) {
							if (log) Debug.LogWarning("Joint " + muscles[i].joint.name + " is used by multiple muscles (indexes " + i + " and " + c + "), PuppetMaster muscle setup is invalid.", transform);
							return false;
						}
					}
				}
			}
			
			if (muscles[0].joint.connectedBody != null && muscles.Length > 1) {
				for (int i = 1; i < muscles.Length; i++) {
					if (muscles[i].transform.GetComponent<Rigidbody>() == muscles[0].joint.connectedBody) {
						if (log) Debug.LogWarning("The first muscle needs to be the one that all the others are connected to (the hips).", transform);
						return false;
					}
				}
			}
			
			/*
			for (int i = 0; i < muscles.Length; i++) {
				if (Vector3.SqrMagnitude(muscles[i].joint.transform.position - muscles[i].target.position) > 0.0001f) {
					if (log) Debug.LogWarning("The position of each muscle needs to match with the position of it's target. Muscle '" + muscles[i].joint.name + "' position does not match with it's target.", muscles[i].joint.transform);
					return;
				}
			}
			*/
			
			return true;
		}

		// Log an error if API is called before initiation.
		private bool CheckIfInitiated() {
			if (!initiated) Debug.LogError("PuppetMaster has not been initiated yet.");
			return initiated;
		}
	}
}
