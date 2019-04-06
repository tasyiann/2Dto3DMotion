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
    /// The scaling method being used. Default is SCALE_HEIGHT.
    /// </summary>
    public EnumScaleMethod ScaleMethod = EnumScaleMethod.SCALE_HEIGHT;
    /// <summary>
    /// How many projections per frame database is made of.
    /// </summary>
    public int projectionsPerFrame = 30;


}
