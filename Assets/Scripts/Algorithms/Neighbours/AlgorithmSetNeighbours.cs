using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[System.Serializable()]
public abstract class AlgorithmSetNeighbours {


    public abstract void SetNeighbours(OPPose opPose, int k, List<List<BvhProjection>> clusters, List<BvhProjection> representatives, List<BvhProjection> mainRepresentatives = null, List<List<BvhProjection>> mainClusters=null);


    public static void sortNeighbours(List<Neighbour> neighbours)
    {
        neighbours.Sort(delegate (Neighbour a, Neighbour b)
        {
            return a.distance2D.CompareTo(b.distance2D);
        });
    }

    // TODO:: RETURNS AN ARRAY OF N NEAREST CLUSTERS - SHOULD I USE THE OTHER ONE?
    public static IList<int> nearestClustersToSearch(OPPose opPose, List<BvhProjection> representatives, int amount)
    {
        SortedList<float, int> sortedList = new SortedList<float, int>();
        int nearestClustersCounter = 0;
        for (int clusterID=0; clusterID<representatives.Count; clusterID++)
        {
            // 1. * * Find Distance * *
            // * * * * * * * * * * * * *
            float distance = representatives[clusterID].Distance2D(opPose);

            // 2. * * Update list that represents the nearest clusters * *
            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            // If there is space for some more clusters, add the cluster no matter what.
            if (nearestClustersCounter < amount)
            {
                // SortedList throws an exception if there is a duplicated key.
                if (sortedList.ContainsKey(distance) == true)
                    continue;
                sortedList.Add(distance, clusterID);
                nearestClustersCounter++;
            }
            else
            {
                // Otherwise, add cluster to the list, only if its distance is lower than the max (in the list).
                if (distance < sortedList.Keys[amount - 1])
                {
                    // SortedList throws an exception if there is a duplicated key.
                    if (sortedList.ContainsKey(distance) == true)
                        continue;
                    sortedList.RemoveAt(amount - 1);     // Remove the last (in the list) cluster (max value).
                    sortedList.Add(distance, clusterID); // Add (sorted) the cluster.
                }
            }
        }
        return sortedList.Values;
    }

    private static void debugNearestClusters(SortedList<float, int> sortedList, SortedList<float, int> debugList)
    {
        string s = "";
        s += "Out of:\n";
        IList<float> keys = debugList.Keys;
        IList<int> values = debugList.Values;
        for(int i=0; i<keys.Count; i++)
        {
            s += values[i]+" : " +keys[i]+"\n";
        }
        s += "The nearest clusters are:\n";
        IList<float>  keys2 = sortedList.Keys;
        IList<int> values2 = sortedList.Values;
        for (int i = 0; i < keys2.Count; i++)
        {
            s += values2[i] + " : " + keys2[i] + "\n";
        }
        Debug.Log(s);
    }

}
