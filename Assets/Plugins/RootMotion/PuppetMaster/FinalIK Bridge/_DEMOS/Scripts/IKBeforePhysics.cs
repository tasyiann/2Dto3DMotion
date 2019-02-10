using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	// Solving IK before the PuppetMaster reads the current pose of the character
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page7.html")]
	public class IKBeforePhysics : MonoBehaviour {

		public PuppetMaster puppetMaster;
		public IK[] IKComponents;

		void Start() {
			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}

		void OnPuppetMasterRead() {
			if (!enabled) return;

			// Solve IK before PuppetMaster reads the pose of the character
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}

		void OnPuppetMasterFixTransforms() {
			if (!enabled) return;

			foreach (IK ik in IKComponents) {
				if (ik.fixTransforms) ik.GetIKSolver().FixTransforms();
			}
		}
	}
}
