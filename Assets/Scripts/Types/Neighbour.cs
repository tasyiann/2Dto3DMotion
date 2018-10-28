using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Neighbour {

    public BvhProjection projection;
    public float distance;

    public Neighbour(BvhProjection projection, float distance)
    {
        this.projection = projection;
        this.distance = distance;
        
    }


}
