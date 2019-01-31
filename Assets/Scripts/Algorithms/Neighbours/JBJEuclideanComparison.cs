using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

[System.Serializable()]
public class JBJEuclideanComparison : AlgorithmSetNeighbours{

    private static int amountOfClustersToSeach = 10;

    public override void SetNeighbours(OPPose opPose, int k, List<List<BvhProjection>> clusters, List<BvhProjection> representatives)
    {
        SortedList<float,Neighbour> sortedList = new SortedList<float,Neighbour>();
        // Find the nearest clusters to search.
        IList<int> nearestClusters = nearestClustersToSearch(opPose, representatives, amountOfClustersToSeach);
        int nearestNeighboursCounter = 0;
        // For each near cluster, search the projections!
        foreach (int clusterId in nearestClusters)
        {
            // Update nearest projections LIST while searching the clusters
            List<BvhProjection> cluster = clusters[clusterId];
            foreach (BvhProjection projection in cluster)
            {
                // Get the distance between the projection and the openpose pose, and create Neighbour.
                Neighbour neighbour = new Neighbour(projection, projection.Distance2D(opPose));
                // If there is space for some more neighbours, add neighbour no matter what.
                if (nearestNeighboursCounter < k)
                {
                    // SortedList throws an exception if there is a duplicated key.
                    if (sortedList.ContainsKey(neighbour.distance2D) == true)
                        continue;
                    sortedList.Add(neighbour.distance2D, neighbour);
                    nearestNeighboursCounter++;
                }
                else
                {
                    // Otherwise, add neighbour to the list, only if its distance is lower than the max neighbour.
                    if (neighbour.distance2D < sortedList.Values[k - 1].distance2D)
                    {
                        // SortedList throws an exception if there is a duplicated key.
                        if (sortedList.ContainsKey(neighbour.distance2D) == true)
                            continue;
                        sortedList.RemoveAt(k - 1);                         // Remove the k-th neighbour (max value).
                        sortedList.Add(neighbour.distance2D, neighbour);    // Add (sorted) the neighbour.
                    }
                }
            }
        }

        // Set the neighbours into openpose pose.
        opPose.neighbours = SortedList_To_List(sortedList.Values);
    }

    private static void debugNearestProjections(SortedList<float, Neighbour> sortedList)
    {
        string s = "";
        s += "Until now, The nearest projections are (distances are shown):\n";
        IList<float> keys = sortedList.Keys;
        for (int i = 0; i < keys.Count; i++)
        {
            s += keys[i] + "\n";
        }
        Debug.Log(s);
    }


    private List<Neighbour> SortedList_To_List(IList<Neighbour> il)
    {
        List<Neighbour> list = new List<Neighbour>();
        foreach(Neighbour n in il)
        {
            list.Add(n);
        }
        return list;
    }

}
