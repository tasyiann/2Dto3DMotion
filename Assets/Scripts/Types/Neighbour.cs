using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Neighbour {

    public BvhProjection projection;
    public float distance2D;
    public float distance3D;
    public Quaternion[] rotations;
    public Quaternion hipRotation;
    public List<BvhProjection> windowIn3Dpoints;    // The window used in the Algorithm.

    public Neighbour(BvhProjection projection, float distance2D)
    {
        this.projection = projection;
        this.distance2D = distance2D;
        this.distance3D = 0.0f;
        windowIn3Dpoints = new List<BvhProjection>();
    }

    public void set3DDistance(float distance3D)
    {
        this.distance3D = distance3D;
    }
        
    public void setRotations(Quaternion[] rots, Quaternion hipRot)
    {
        rotations = rots;
        hipRotation = hipRot;
    }
}
