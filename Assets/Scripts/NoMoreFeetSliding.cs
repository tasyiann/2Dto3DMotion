using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RootMotion.FinalIK
{

    public class NoMoreFeetSliding : MonoBehaviour
    {
        #region Parameters
        public GameObject model;
        public Material SlightTransparentMat;
        public Material InvisibleMat;
        public bool showPrediction = false;
        public bool DebugMode = false;
        //public Renderer[] skins;
        [Range(0.0f,1f)]
        public float secondsPrediction = 0.3f;
        /// <summary>
        /// Minimum distance from the feet joint to the ground.
        /// </summary>
        [Range(0.0f, 1f)]
        public float floorOffset = 0.1f;
        /// <summary>
        /// Maximum distance between future and current joint.
        /// </summary>
        [Range(0.0f, 1f)]
        public float maxDistanceBetweenJoints = 0.1f;
        [Range(0.0f, 1f)]
        public float feetHeight = 0.3f;
        [Range(0.0f, 1f)]
        public float positionWeight = 1f;
        [Range(0.0f, 1f)]
        public float rotationWeight = 0.5f;

        #endregion Parameters

        private FullBodyBipedIK fullBodyBipedIK_Current;
        private FullBodyBipedIK fullBodyBipedIK_Future;

        Transform leftFoot_Future;
        Transform rightFoot_Future;
        Transform leftFoot_Current;
        Transform rightFoot_Current;

        IKEffector leftFoot_Effector;
        IKEffector rightFoot_Effector;
        bool isLeftFootFrozen = false;
        bool isRightFootFrozen = false;

        private GameObject currentModel;
        private GameObject futureModel;
        private Animator modelFutureAnimator;
        private AnimationClip modelFutureAnimationClip;

        private Renderer[] currModelRenderers;
        private Renderer[] futureModelRenderers;


        private bool visible = true;

        private GameObject leftFootTarget;
        private GameObject rightFootTarget;

        private void Start()
        {
            /* Change model's layer to "Ignore Raycast", so in Update function we can raycast to the ground. */
            model.layer = 2; // Here, please place "Ignore Raycast" Layer. Just to be sure.

            /* Current model (a duplicate of model) */
            currentModel = Instantiate(model);                           // Duplicate model.
            currentModel.name = "NoMoreSliding_Humanoid";                // Rename new gameobject.
            currentModel.transform.SetParent(model.transform.parent);    // Place in hierarchy.
            currentModel.transform.position = model.transform.position;
            currModelRenderers = getSkinsRenderers(currentModel);

            fullBodyBipedIK_Current = currentModel.GetComponent<FullBodyBipedIK>();
            leftFoot_Effector = fullBodyBipedIK_Current.solver.leftFootEffector;
            rightFoot_Effector = fullBodyBipedIK_Current.solver.rightFootEffector;


            /* Future prediction model (model) */
            futureModel = model;
            futureModelRenderers = getSkinsRenderers(futureModel);
            futureModel.name = "FutureMovement_Humanoid";                                            // Rename future model.
            setSkinsSlightTransparent(futureModelRenderers);                                         // Make it transparent.
            modelFutureAnimator = futureModel.transform.GetComponent<Animator>();                    // Get the animator.
            modelFutureAnimationClip = modelFutureAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;    // Get Animation clip.
            Debug.Log("Animation: '" + modelFutureAnimationClip.name + "' will be forwarded by some frames");
            float animationDuration = modelFutureAnimationClip.length;                                            // (1 / this) equals the percentage of a second
            modelFutureAnimator.Play(modelFutureAnimationClip.name, 0, (1/animationDuration)* secondsPrediction); // Make future animation ahead.

            fullBodyBipedIK_Future = futureModel.GetComponent<FullBodyBipedIK>();
            leftFoot_Future = fullBodyBipedIK_Future.references.leftFoot.transform;     // Get Left foot of futureModel
            rightFoot_Future = fullBodyBipedIK_Future.references.rightFoot.transform;   // Get Right foot of futureModel



            // Targets
            leftFootTarget = new GameObject("LeftFootTarget");
            rightFootTarget = new GameObject("RightFootTarget");
            leftFootTarget.transform.SetParent(currentModel.transform);
            rightFootTarget.transform.SetParent(currentModel.transform);
            leftFoot_Effector.target = leftFootTarget.transform;
            rightFoot_Effector.target = rightFootTarget.transform;



        }

        // Why does this work?
        private void setSkinsSlightTransparent(Renderer[] skins)
        {
            foreach(Renderer skin in skins)
            {
                skin.material = SlightTransparentMat;
            }
        }
        private void setSkinsInvisible(Renderer[] skins)
        {
            foreach (Renderer skin in skins)
            {
                skin.material = InvisibleMat;
            }
        }
        private void setSkinsColor(Color color, GameObject model)
        {
            Renderer[] tempRenderers = model.GetComponentsInChildren<Renderer>();
            foreach (Renderer skin in tempRenderers)
            {
                skin.material.color = color;
            }
        }
        private Renderer[] getSkinsRenderers(GameObject model)
        {
            return model.GetComponentsInChildren<Renderer>();
        }


        private void Update()
        {
            examineAndFixFoot(ref isLeftFootFrozen, leftFoot_Future, leftFoot_Current, leftFoot_Effector, ref leftFootTarget, "LeftFootTarget");
            examineAndFixFoot(ref isRightFootFrozen, rightFoot_Future, rightFoot_Current, rightFoot_Effector, ref rightFootTarget, "RightFootTarget");
            predictionModelAppear();
        }

        private void predictionModelAppear()
        {
            if (showPrediction && !visible)
            {
                setSkinsSlightTransparent(futureModelRenderers);
                visible = true;
            }
            if(visible && !showPrediction)
            {
                setSkinsInvisible(futureModelRenderers);
                visible = false;
            }
                
        }


        private void examineAndFixFoot(ref bool isFootFrozen, Transform foot_future, Transform foot_current, IKEffector footEffector, ref GameObject footTarget, string name)
        {
            bool intersectionFound;
            RaycastHit intersection = groundIntersection(foot_future.transform, out intersectionFound);
            float distanceFromGround = intersection.distance;
            if (intersectionFound && !isFootFrozen  && distanceFromGround < (floorOffset-feetHeight))
            {
                // Place feet above the ground plane, not in the ground plane.
                Vector3 targetPosition = new Vector3(intersection.point.x, intersection.point.y + feetHeight, intersection.point.z);
                updateTargetPosition(footTarget, targetPosition);
                stickOnGround(footEffector);
                isFootFrozen = true;
                if (DebugMode)
                {
                    setSkinsColor(Color.red, currentModel);
                    Debug.Log(foot_future.name + " - preventing Foot Sliding");
                }

            }
            if (intersectionFound && isFootFrozen && distanceFromGround >= (floorOffset-feetHeight))
            {
                releaseFromGround(footEffector);
                isFootFrozen = false;
                if (DebugMode)
                {
                    setSkinsColor(Color.blue, currentModel);
                    //Debug.Log(foot.name + " is RELEASED.");
                }

            }
        }

        private float getDistanceBetweenJoints(Transform foot_future, Transform foot_current)
        {
            return Vector3.Distance(foot_future.position, foot_current.position);
        }


        private void updateTargetPosition(GameObject footTarget, Vector3 position)
        {
            footTarget.transform.position = position;
            
        }

        private void stickOnGround(IKEffector effector)
        {
            effector.positionWeight = positionWeight;
            effector.rotationWeight = rotationWeight;
        }

        private void releaseFromGround(IKEffector effector)
        {
            effector.positionWeight = 0f;
            effector.rotationWeight = 0f;
        }

        private RaycastHit groundIntersection(Transform foot, out bool intersectionFound)
        {
             
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(foot.position, -Vector3.up, out hit))
            {
                intersectionFound = true;
                return hit;
            }
            intersectionFound = false;
            return new RaycastHit();
        }

    }


}
 
