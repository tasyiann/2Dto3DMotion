using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataBaseParameters", menuName ="ScriptableObject/DataBaseParameters")]
public class DataBaseParameters : ScriptableObject
{
    /* * * * * * * * * * * * * * * * Database info * * * * * * * * * * * * * * * */
    /// <summary>
    /// Parent path of database. Folders in path should follow specific rules. 
    /// </summary>
    public string databasePath = @"Databases\NEW";
    /// <summary>
    /// Name of cluster's folder. Folders should follow specific rules. 
    /// </summary>
    public string clusteringFolder = "5000-clusters";
    /// <summary>
    /// The scaling method being used. Default is SCALE_LIMBS.
    /// </summary>
    public EnumScaleMethod ScaleMethod = EnumScaleMethod.SCALE_LIMBS;
    /// <summary>
    /// How many projections per frame database is made of.
    /// </summary>
    public int projectionsPerFrame = 30;


}
