using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {
	
	// Handles the boosting functionality of BehaviourPuppet
	public partial class BehaviourPuppet : BehaviourBase {
		
		/// <summary>
		/// Boosts both immunity and impulseMlp for the entire puppet.
		/// </summary>
		public void Boost(float immunity, float impulseMlp) {
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				Boost(i, immunity, impulseMlp);
			}
		}
		
		/// <summary>
		/// Boosts both immunity and impulseMlp for the specified muscle.
		/// </summary>
		public void Boost(int muscleIndex, float immunity, float impulseMlp) {
			BoostImmunity(muscleIndex, immunity);
			BoostImpulseMlp(muscleIndex, impulseMlp);
		}
		
		/// <summary>
		/// Boosts both immunity and impulseMlp for the specified muscle and other muscles according to the boostParents and boostChildren falloffs.
		/// </summary>
		public void Boost(int muscleIndex, float immunity, float impulseMlp, float boostParents, float boostChildren) {
			if (boostParents <= 0f && boostChildren <= 0f) {
				Boost (muscleIndex, immunity, impulseMlp);
				return;
			}

			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
				
				Boost(i, immunity * falloff, impulseMlp * falloff);
			}
		}

		/// <summary>
		/// Sets the immunity of all the muscles to the specified value. Immunity reduces damage from collisions and hits. Immunity will be lerped back to normal automatically (boostFalloff).
		/// </summary>
		public void BoostImmunity(float immunity) {
			if (immunity < 0f) return;
			
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				BoostImmunity(i, immunity);
			}
		}
		
		/// <summary>
		/// Sets the immunity of the muscle at the muscleIndex to the specified value. Immunity reduces damage from collisions and hits. Immunity will be lerped back to normal automatically (boostFalloff).
		/// </summary>
		public void BoostImmunity(int muscleIndex, float immunity) {
			puppetMaster.muscles[muscleIndex].state.immunity = Mathf.Clamp(immunity, puppetMaster.muscles[muscleIndex].state.immunity, 1f);
		}
		
		/// <summary>
		/// Sets the immunity of the muscle at the muscleIndex (and other muscles according to boostParents and boostChildren falloffs) to the specified value. Immunity reduces damage from collisions and hits. Immunity will be lerped back to normal automatically (boostFalloff).
		/// </summary>
		public void BoostImmunity(int muscleIndex, float immunity, float boostParents, float boostChildren) {
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
				
				BoostImmunity(i, immunity * falloff);
			}
		}
		
		/// <summary>
		/// Sets the impulse multiplier of all the muscles to the specified value. Larger impulse multiplier makes the muscles deal more damage to the muscles of other characters. Muscle impulse multipliers will be lerped back to normal automaticalle (boostFalloff).
		/// </summary>
		public void BoostImpulseMlp(float impulseMlp) {
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				BoostImpulseMlp(i, impulseMlp);
			}
		}
		
		/// <summary>
		/// Sets the impulse multiplier of the muscle at the muscleIndex to the specified value. Larger impulse multiplier makes the muscle deal more damage to the muscles of other characters. Muscle impulse multipliers will be lerped back to normal automaticalle (boostFalloff).
		/// </summary>
		public void BoostImpulseMlp(int muscleIndex, float impulseMlp) {
			puppetMaster.muscles[muscleIndex].state.impulseMlp = Mathf.Max(impulseMlp, puppetMaster.muscles[muscleIndex].state.impulseMlp);
		}
		
		/// <summary>
		/// Sets the impulse multiplier of the muscle at the muscleIndex (and other muscles according to boostParents and boostChildren falloffs) to the specified value. Larger impulse multiplier makes the muscle deal more damage to the muscles of other characters. Muscle impulse multipliers will be lerped back to normal automaticalle (boostFalloff).
		/// </summary>
		public void BoostImpulseMlp(int muscleIndex, float impulseMlp, float boostParents, float boostChildren) {
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				float falloff = GetFalloff(i, muscleIndex, boostParents, boostChildren);
				
				BoostImpulseMlp(i, impulseMlp * falloff);
			}
		}
	}
}