using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class Rotations  {

    private List<Vector3> rotations;
    public Rotations(List<Vector3> rotationsxyz)
    {
        rotations = rotationsxyz;
    }
    public List<Vector3> getAllRotations() { return rotations; }

    public List<Vector3> getComparableRotations()
    {
        List<Vector3> list = new List<Vector3>();
        int[] indexes = Base.base_orderOfComparableRotations;
        foreach (int rotationIndex in indexes)
        {
            list.Add(rotations[rotationIndex]);
        }
        return list;
    }
}
