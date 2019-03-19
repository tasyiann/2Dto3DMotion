using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Database { 

    public List<List<Rotations>> rotationFiles;          // All rotation files.
    public List<Cluster> clusters;                       // All clusters.
    public List<List<BvhProjection>> not_clustered;      // All projections not clustered.


    public Database(List<Cluster> base_clusters, List<List<BvhProjection>> base_not_clustered, List<List<Rotations>> base_rotationFiles)
    {
        rotationFiles = base_rotationFiles;
        clusters = base_clusters;
        not_clustered = base_not_clustered;
    }

}
