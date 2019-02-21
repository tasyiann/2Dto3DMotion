using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTargetSide : MonoBehaviour {

    public Transform player;       //Public variable to store a reference to the player game object

    private Vector3 offset;         //Private variable to store the offset distance between the player and camera
    public float smoothTime = 0.0001f;
    private Transform _camTransform;
    private Vector3 velocity = Vector3.zero;
    // Use this for initialization
    void Start()
    {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        offset = transform.position - player.transform.position;
        _camTransform = GetComponent<Transform>();
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        transform.position = Vector3.SmoothDamp(_camTransform.position, desiredPosition, ref velocity, smoothTime);
    }
}
