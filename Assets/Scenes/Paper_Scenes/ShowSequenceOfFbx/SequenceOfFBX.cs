using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class SequenceOfFBX : MonoBehaviour
{

    public Vector3 offsetBetweenFigures = new Vector3(4f,0,0);
    public float rate = 0.005f;
    public Vector3 startingPoint;
    public GameObject prefab;

    private Animator animator;
    private AnimationClip animationclip;
    //private float currFrame = 0f;
    private Vector3 position;
    public GameObject container;

    List<GameObject> gameObjs = new List<GameObject>();
    void Start()
    {
        container = new GameObject(Path.GetRandomFileName());
        float currentFrame = 0;
        while (currentFrame <= 1)
        {
            createFigure(currentFrame);
            currentFrame += rate;
        }
    }

    bool first = false;
    bool second = false;
    private void Update()
    {

        if (first & !second)
        {
            foreach (GameObject go in gameObjs)
            {
                animator = go.GetComponent<Animator>();
                animationclip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                animator.enabled = false;
                second = true;
            }
        }


        if (!first) // One time iteration
        {
            float currentFrame = 0;
            foreach (GameObject go in gameObjs)
            {
                animator = go.GetComponent<Animator>();
                animationclip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                animator.Play(animationclip.name, 0, currentFrame);
                currentFrame += rate;
                first = true;
            }
        }


       



    }


    private void createFigure(float currentFrame)
    {
        GameObject go = Instantiate(prefab);
        go.transform.SetParent(container.transform);
        go.name = "figure"+ currentFrame;
        go.transform.position = position;
        position += offsetBetweenFigures;
        gameObjs.Add(go);
    }


}
