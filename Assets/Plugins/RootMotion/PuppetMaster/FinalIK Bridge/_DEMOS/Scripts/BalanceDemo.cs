using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Resets the Animator and the balance behaviour once BehaviourFall ends.
	public class BalanceDemo : MonoBehaviour {

		public Animator animator;
		public BehaviourBipedBalanceFBBIK balance;

		public void ResetPuppet() {
			balance.StartCoroutine(balance.Restart());

			animator.CrossFadeInFixedTime("Idle", 0.2f);
		}
	}
}
