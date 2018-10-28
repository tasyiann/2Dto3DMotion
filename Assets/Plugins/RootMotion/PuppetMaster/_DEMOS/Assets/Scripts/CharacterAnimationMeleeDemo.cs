using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller for PuppetMaster "Melee" demo.
	/// </summary>
	public class CharacterAnimationMeleeDemo: CharacterAnimationThirdPerson {

		CharacterMeleeDemo melee { get { return characterController as CharacterMeleeDemo; }}

		// Update the Animator with the current state of the character controller
		protected override void Update() {
			base.Update();

			animator.SetInteger("ActionIndex", -1);

			if (melee.currentAction != null) {
				animator.SetInteger("ActionIndex", melee.currentActionIndex);

				var anim = melee.currentAction.anim;

				animator.CrossFadeInFixedTime(anim.stateName, anim.transitionDuration, anim.layer, anim.fixedTime);

				if (anim.layer != 0) {
					StopAllCoroutines();
					StartCoroutine(FadeLayerWeight(anim.layer, anim.transitionDuration));
				}

				melee.currentActionIndex = -1;
			}
		}

		private IEnumerator FadeLayerWeight(int layerIndex, float duration) {
			float fadeIn = 0f;

			while (fadeIn < 1f) {
				fadeIn += Time.deltaTime * (1f / duration);
				animator.SetLayerWeight(layerIndex, Mathf.Max(animator.GetLayerWeight(layerIndex), RootMotion.Interp.Float(fadeIn, InterpolationMode.InOutCubic)));
				yield return null;
			}

			while (GetNormalizedTime(layerIndex) < 1f - duration) yield return null;

			float fadeOut = animator.GetLayerWeight(layerIndex);

			while (fadeOut > 0f) {
				fadeOut -= Time.deltaTime * (1f / duration);
				animator.SetLayerWeight(layerIndex, RootMotion.Interp.Float(fadeOut, InterpolationMode.InOutCubic));
				yield return null;
			}

			animator.SetLayerWeight(layerIndex, 0f);
		}

		private float GetNormalizedTime(int layerIndex) {
			return animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime - (int)animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;
		}
	}
}
