using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Solving IK after the physics simulation to make cosmetic adjustments to the final pose of the character for that frame.
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page7.html")]
	public class IKAfterPhysics : MonoBehaviour {

		public PuppetMaster puppetMaster;
		public IK[] IKComponents;
		
		void Start() {
			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}
		
		void OnPuppetMasterWrite() {
			if (!enabled) return;

			// Solve IK after PuppetMaster writes the solved pose on the animated character.
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
