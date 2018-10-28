using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBipedHeadCatchFall : SubBehaviourBase {

		public Weight lookForLandingWeight;
		public Weight muscleWeight = new Weight(1f);

		private SubBehaviourCOM centerOfMass;

		public void Initiate(BehaviourBase behaviour, SubBehaviourCOM centerOfMass) {
			this.behaviour = behaviour;
			this.centerOfMass = centerOfMass;
			
			behaviour.OnPostRead += OnPostRead;
		}
		
		#region Behaviour Delegates

		private void OnPostRead() {
			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				if (m.props.group == Muscle.Group.Head) m.state.muscleWeightMlp = muscleWeight.GetValue(centerOfMass.angle);
			}
		}
		
		#endregion Behaviour Delegates
	}
}
