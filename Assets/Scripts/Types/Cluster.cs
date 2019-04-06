using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Cluster 
{
    public List<BvhProjection> projections;

    // Each Cluster has its own projections.
    private BvhProjection representative;
    public BvhProjection Representative { private set { representative = value; } get { return representative; } } // Cluster's representative. Which is a bvhProjection.

    public void setRepresentative(BvhProjection r)
    {
        representative = r;
    }

    public void SortCluster()
    {
        setDistancesFromReperesentative();
        sortProjections();
    }

    public int getNumberOfProjections { get { return projections.Count; } }


    public Cluster(List<BvhProjection> projections_filled_with_data)
    {
        projections = projections_filled_with_data;
        foreach (BvhProjection p in projections)
        {
            p.cluster = this;
        }
    }



    private void sortProjections()
    {
        projections.Sort((x, y) =>
        x.distanceFromRepresentative.CompareTo(y.distanceFromRepresentative));
    }


    private void setDistancesFromReperesentative()
    {
        foreach (BvhProjection p in projections)
        {
            p.distanceFromRepresentative = p.Distance2D(representative);
        }
    }
}
