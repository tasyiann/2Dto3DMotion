using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

[System.Serializable()]
public class JBJEuclideanComparison : AlgorithmSetNeighbours
{



    public override void SetNeighbours(OPPose opPose, int k, List<Cluster> clusters)
    {
        int amountOfClustersToSearch = Base.numClustersToSearch;

        SortedList<float, Neighbour> neighbours = new SortedList<float, Neighbour>();
        // Find the nearest clusters to search.
        IList<Cluster> nearestClusters = nearestClustersToSearch(opPose, clusters, amountOfClustersToSearch);
        opPose.setSelectedClusters(nearestClusters); // So we can debug this later.
        int nearestNeighboursCounter = 0;

        foreach (Cluster cluster in nearestClusters)
        {
            foreach (BvhProjection projection in cluster.projections)
            {
                float distance = projection.Distance2D(opPose);             // Calculate distance.
                Neighbour neighbour = new Neighbour(projection, distance);   // Create new Neighbour with the projection.
                if (nearestNeighboursCounter < k)
                {
                    // SortedList throws an exception if there is a duplicated key.
                    if (neighbours.ContainsKey(neighbour.distance2D) == true)
                        continue;
                    neighbours.Add(neighbour.distance2D, neighbour);
                    nearestNeighboursCounter++;
                }
                else
                {
                    // Otherwise, add neighbour to the list, only if its distance is lower than the max neighbour.
                    if (neighbour.distance2D < neighbours.Values[k - 1].distance2D)
                    {
                        // SortedList throws an exception if there is a duplicated key.
                        if (neighbours.ContainsKey(neighbour.distance2D) == true)
                            continue;
                        neighbours.RemoveAt(k - 1);                         // Remove the k-th neighbour (max value).
                        neighbours.Add(neighbour.distance2D, neighbour);    // Add (sorted) the neighbour.
                    }
                }
            }
        }
        // Set the neighbours into openpose pose.
        opPose.neighbours = SortedList_To_List(neighbours.Values);
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
        foreach (Neighbour n in il)
        {
            list.Add(n);
        }
        return list;
    }

}
