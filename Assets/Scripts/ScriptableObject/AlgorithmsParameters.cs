using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AlgorithmsParameters", menuName = "ScriptableObject/AlgorithmsParameters")]
public class AlgorithmsParameters : ScriptableObject
{

    /* * * * * * * * * * * * Search best projections Algorithm * * * * * * * * * */
    /// <summary>
    /// How many clusters to search in, in order to find the best-2D-matched projections.
    /// Algorithm finds the representatives with the minimum euclidean distance.
    /// </summary>
    public int numberOfClustersToSearch = 25;
    /// <summary>
    /// Number of best matched projections.
    /// </summary>
    public int k = 20;
    /// <summary>
    /// Algorithm of searching for the k best projections.
    /// </summary>
    public AlgorithmSetNeighbours neighboursAlgorithm = new JBJEuclideanComparison();

    /* * * * * * * * * * * * * 3D Estimation Algorithm * * * * * * * * * * * * */
    /// <summary>
    /// Algorithm of finding the 3D estimation of input.
    /// </summary>
    public AlgorithmEstimation estimation3dAlgorithm = new PrevFrameWindow3D();
    /// <summary>
    /// The length of the window. A window is defined as the previous m frames of a 3D animation frame.
    /// </summary>
    public int m = 4;
    /// <summary>
    /// Indices of joints in bvh files, that are being used to compare rotations using Lee et al. algorithm.
    /// These indices corresponds to the default joints used in openpose output also.
    /// </summary>
    public int[] orderOfComparableRotations = { 4, 2, 8, 9, 10, 5, 6, 7, 14, 15, 16, 11, 12, 13 };

}
