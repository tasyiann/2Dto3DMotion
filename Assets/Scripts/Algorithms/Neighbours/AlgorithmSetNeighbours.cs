using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[System.Serializable()]
public abstract class AlgorithmSetNeighbours {


    public abstract void SetNeighbours(OPPose opPose, int k, List<Cluster> clusters);


    public static void sortNeighbours(List<Neighbour> neighbours)
    {
        neighbours.Sort(delegate (Neighbour a, Neighbour b)
        {
            return a.distance2D.CompareTo(b.distance2D);
        });
    }


    public static IList<Cluster> nearestClustersToSearch(OPPose opPose, List<Cluster> clusters, int amount)
    {
        SortedList<float, Cluster> sortedList = new SortedList<float, Cluster>();   // Sorted list of nearest clusters.
        int nearestClustersCounter = 0;                                           

        foreach (Cluster cluster in clusters)
        {
            // Get Distance Representative - Figure.
            float distance = cluster.representative.Distance2D(opPose);             
            if (nearestClustersCounter < amount)
            {
                // SortedList throws an exception if there is a duplicated key.
                if (sortedList.ContainsKey(distance) == true)
                    continue;
                sortedList.Add(distance, cluster);
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
                    sortedList.RemoveAt(amount - 1);        // Remove the last (in the list) cluster (max value).
                    sortedList.Add(distance, cluster);      // Add (sorted) the cluster.
                }
            }
        } // end of loop
        return sortedList.Values;
    }


}
