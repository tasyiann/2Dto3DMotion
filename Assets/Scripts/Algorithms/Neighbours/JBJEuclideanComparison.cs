using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;

[System.Serializable()]
public class JBJEuclideanComparison : AlgorithmSetNeighbours{


    public override void SetNeighbours(OPPose opPose, int k, List<List<BvhProjection>> clusters, List<BvhProjection> representatives)
    {
        int neighboursCounter = 0;
        SortedList<float,Neighbour> sortedList = new SortedList<float,Neighbour>();
        // Find the right cluster to search.
        //int cluster_index = findClusterToSearch(opPose, representatives);
        int cluster_index = 0; // Just for now, to debug the rest.
        List<BvhProjection> cluster = clusters[cluster_index];
        foreach(BvhProjection projection in cluster)
        {
            // Get the distance between the projection and the openpose pose, and create Neighbour.
            Neighbour neighbour = new Neighbour(projection, projection.Distance2D(opPose));
            // Tf there is space for some more neighbours, add neighbour no matter what.
            if (neighboursCounter < k)
            {
                // SortedList throws an exception if there is a duplicated key.
                if (sortedList.ContainsKey(neighbour.distance) == true)
                    continue;
                sortedList.Add(neighbour.distance, neighbour);
                neighboursCounter++;
            }
            else
            {
                // Otherwise, add neighbour to the list, only if its distance is lower than the max neighbour.
                if (neighbour.distance < sortedList.Values[k - 1].distance)
                {
                    // SortedList throws an exception if there is a duplicated key.
                    if (sortedList.ContainsKey(neighbour.distance) == true)
                        continue;
                    sortedList.RemoveAt(k - 1);                       // Remove the k-th neighbour (max value).
                    sortedList.Add(neighbour.distance, neighbour);    // Add (sorted) the neighbour.
                }
            }
        }
        // Set the neighbours into openpose pose.
        opPose.neighbours = SortedList_To_List(sortedList.Values);
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
