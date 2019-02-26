using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster 
{
    public List<BvhProjection> projections;     // Each Cluster has its own projections.
    public BvhProjection representative;        // Cluster's representative. Which is a bvhProjection.
    public int getNumberOfProjections { get { return projections.Count; } }


    public Cluster(List<BvhProjection> projections_filled_with_data)
    {
        projections = projections_filled_with_data;
    }

    
    
}
