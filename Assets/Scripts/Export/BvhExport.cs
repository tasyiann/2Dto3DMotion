using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System.Text;

/**
 * Explanation on bvh rotations:
 *  https://forums.autodesk.com/t5/fbx-forum/eulerangles-quaternions-and-the-right-rotationorder/td-p/4166203
 * 
 * 
 * 1. To confirm the rotation order of your BVH file. In BVH file, if its channel like this:
CHANNELS 6 Xposition Yposition Zposition Xrotation Yrotation Zrotation
Then the transformations actually happened from the last one to the first one: Zrotation Yrotation Xrotation Zposition Yposition Xposition (rotate first, then translate)

2. Currently in FBX, all rotation are treated as in XYZ rotation order, for example, in the KFbxXMatrix, which is used to represent the affine transform, it only works in XYZ rotation order.
This limitation might be fixed in the future, but that's another thing and will not come soon.
 * 
 */

/* Important:
   Make sure that: the template, and all the bvh used in this program, have the same HIERARCHY. */

public class BvhExport
{
    private List<List<Rotations>> base_rotationFiles = Base.base_rotationFiles;

    private string HierarchyBody { get; set; }
    private string MotionBody { get; set; }
    private Neighbour[] Estimation { get; set; }
    private string TemplateFilename;
    private List<string> MotionLines { get; set; }


    public BvhExport(string templateFilename, Neighbour[] solution=null)
    {
        TemplateFilename = templateFilename;
        HierarchyBody = FileToString(TemplateFilename);
        Estimation = solution;
        if (solution != null)
        {
            MotionLines = CreateMotionLines();
            MotionBody = CreateMotionBody();
        }
    }

    private string FileToString(string TemplateFilename)
    {
        string text = System.IO.File.ReadAllText(TemplateFilename);
        return text;
    }

    /// <summary>
    /// TODO: Apply rotation and translation of root.
    /// </summary>
    /// <returns></returns>
    private List<string> CreateMotionLines()
    {
        List<string> lines = new List<string>();
        foreach(Neighbour n in Estimation)
        {
            // If estimation in that frame does not exist.
            if (n == null)
                continue;

            List<Vector3> rotations = base_rotationFiles[n.projection.rotationFileID][n.projection.frameNum].getAllRotations();
            Vector3 defaultRootRotation = new Vector3(90, 90, 0); // Default. Please don't delete this. It is useful to debug.
            Vector3 rootRotation = defaultRootRotation;
            float angle = n.projection.angle;
            rootRotation = getFinalRootRotation(rotations[0], angle);
            //rootRotation = Model3D.BvhToUnityRotation(rootRotation, Model3D.AxisOrder.XYZ).eulerAngles;
            lines.Add(CreateMLine(Vector3.zero, rootRotation, rotations));
        }
        return lines;
    }



    private string CreateMLine(Vector3 rootPosition, Vector3 rootRotation, List<Vector3> rotations)
    {
        StringBuilder s = new StringBuilder();
        // Apply root's translation and rotation.
        s.Append(rootPosition.x + " " + rootPosition.y + " " + rootPosition.z + " ");
        s.Append(rootRotation.z + " " + rootRotation.x + " " + rootRotation.y + " ");

        for(int i=1; i<rotations.Count; i++) // start at 1 ==> skip rotation of the root.
        //foreach (Vector3 eul in rotations)
        {
            s.Append(rotations[i].z + " " + rotations[i].x + " " + rotations[i].y + " ");
        }
        s.Append("\n");
        return s.ToString();
    }


    private Vector3 getFinalRootRotationFromModel(Vector3[] joints, float angle = 0)
    {
        // Calculate root Quaternion
        Vector3 rootPosition = (joints[8] + joints[11]) / 2;
        Quaternion x = Model3D.YLookRotation((joints[1] - rootPosition), Vector3.up);
        Quaternion y = Model3D.XLookRotation(joints[8] - joints[11], Vector3.up);
        Quaternion z = y;

        // Make a rotation matrix out of every Euler value from the bvh file. 
        Matrix4x4 lRotationX_Matrix, lRotationY_Matrix, lRotationZ_Matrix;
        // Instead of "euler" use "angle-axis"
        lRotationX_Matrix = Matrix4x4.Rotate(x);
        lRotationY_Matrix = Matrix4x4.Rotate(y);
        lRotationZ_Matrix = Matrix4x4.Rotate(z);

        //Multiply them in the rotation order, in you case ZXY, to get the complete rotation matrix.
        Matrix4x4 lRotationMatrix = lRotationY_Matrix * lRotationX_Matrix * lRotationZ_Matrix;

        //Get the Euler rotation values in XYZ rotation order, because KFbxXMatrix::GetR() only return rotation vector in XYZ rotation order.
        Vector3 lRotationInXYZOrder = lRotationMatrix.rotation.eulerAngles;

        //Set lRotationInXYZOrder,lRotationInXYZOrder,lRotationInXYZOrder as x, y, z rotation Euler angle to the animcurve.
        //What the above did is actually to convert the Euler rotation angle in ZXY order to XYZ order.
        return lRotationInXYZOrder;
    }


    private Vector3 getFinalRootRotation(Vector3 rotationFromBvh, float angle=0)
    {
       

        // Make a rotation matrix out of every Euler value from the bvh file. 
        /*
        Vector3 lRotationX, lRotationY, lRotationZ;
        lRotationX = new Vector3(rotationFromBvh.x, 0.0f, 0.0f);
        lRotationY = new Vector3(0.0f, rotationFromBvh.y, 0.0f);
        lRotationZ = new Vector3(0.0f, 0.0f, rotationFromBvh.z);
        */
        Matrix4x4 lRotationX_Matrix, lRotationY_Matrix, lRotationZ_Matrix, lRotationY_Angle_Matrix_OK, lRotationY_Angle_Matrix_INVERSE;
        /*
        lRotationX_Matrix = Matrix4x4.Rotate(Quaternion.Euler(lRotationX));
        lRotationY_Matrix = Matrix4x4.Rotate(Quaternion.Euler(lRotationY));
        lRotationZ_Matrix = Matrix4x4.Rotate(Quaternion.Euler(lRotationZ));
        */
        
        // Instead of "euler" use "angle-axis"
        lRotationX_Matrix = Matrix4x4.Rotate(Quaternion.AngleAxis(rotationFromBvh.x, Vector3.right));
        lRotationY_Matrix = Matrix4x4.Rotate(Quaternion.AngleAxis(rotationFromBvh.y, Vector3.up));
        lRotationZ_Matrix = Matrix4x4.Rotate(Quaternion.AngleAxis(rotationFromBvh.z, Vector3.forward));
        lRotationY_Angle_Matrix_OK = Matrix4x4.Rotate(Quaternion.AngleAxis(angle, Vector3.up));
        lRotationY_Angle_Matrix_INVERSE = Matrix4x4.Rotate(Quaternion.AngleAxis((360+angle), Vector3.up));

        //Multiply them in the rotation order, in you case ZXY, to get the complete rotation matrix.
        Matrix4x4 lRotationMatrix = lRotationY_Matrix * lRotationX_Matrix * lRotationZ_Matrix;
        // Apply the angle rotation
        //lRotationMatrix = lRotationMatrix * lRotationY_Angle_Matrix;

        float debugangle = Quaternion.Angle(lRotationMatrix.rotation, (lRotationMatrix * lRotationY_Angle_Matrix_OK).rotation);
        if (Mathf.Abs(debugangle - angle) < 1f)
        {
            lRotationMatrix = lRotationMatrix * lRotationY_Angle_Matrix_OK;
            Debug.Log(" OK: Angle to rotate: " + angle + " Actual rotation: " + debugangle);
        }
        else
        {
            debugangle = Quaternion.Angle(lRotationMatrix.rotation, (lRotationMatrix * lRotationY_Angle_Matrix_INVERSE).rotation);
            lRotationMatrix = lRotationMatrix * lRotationY_Angle_Matrix_INVERSE;
            Debug.Log(" INVERSE: Angle to rotate: " + angle + " Actual rotation: " + debugangle);
        }

        //Get the Euler rotation values in XYZ rotation order, because KFbxXMatrix::GetR() only return rotation vector in XYZ rotation order.
        Vector3 lRotationInXYZOrder = lRotationMatrix.rotation.eulerAngles;
        
        //Set lRotationInXYZOrder,lRotationInXYZOrder,lRotationInXYZOrder as x, y, z rotation Euler angle to the animcurve.
        //What the above did is actually to convert the Euler rotation angle in ZXY order to XYZ order.
        return lRotationInXYZOrder;
    }







    private string CreateMotionBody()
    {
        StringBuilder s = new StringBuilder();
        s.Append("MOTION\nFrames: " + MotionLines.Count + "\nFrame Time: " + 0.0083333 + "\n");
        // First line T-pose
        s.Append("0 0 0 -0 90 90 9.69808e-018 1.34041e-020 0.158382 1.41245e-030 7.06225e-031 2.48481e-017 -7.30747e-017 7.61111e-019 -1.19349 3.40995e-017 1.65719e-019 0.556895 -2.12238e-008 -1.53545e-005 -90.1584 -2.50273e-005 -2.47192e-012 6.55612e-006 1.52588e-005 -6.98387e-015 174.455 180 -4.60993e-005 -89.8417 -5.49628e-005 -2.25992e-011 3.612e-005 1.52588e-005 6.98387e-015 174.455 -3.94791 -0.248796 -175.995 -8.56334 0.17593 -0.870008 4.60942 -0.0775208 176.868 177.984 -0.125371 -176.036 -4.37326 0.0836638 -0.877309 2.35438 -0.0332512 176.598\n");
        foreach (string line in MotionLines)
        {
            s.Append(line);
        }
        return s.ToString();
    }


    public void CreateBvhFile(string filename)
    {
        System.IO.File.WriteAllText(filename, HierarchyBody + MotionBody);
    }
    
    public void debugRotationsAxis()
    {
        debugRotationsVersion(-1, -1, -1, "1.bvh");
        debugRotationsVersion(-1, -1,  1, "2.bvh");
        debugRotationsVersion(-1,  1, -1, "3.bvh");
        debugRotationsVersion(-1,  1,  1, "4.bvh");
        debugRotationsVersion( 1, -1, -1, "5.bvh");
        debugRotationsVersion( 1, -1,  1, "6.bvh");
        debugRotationsVersion( 1,  1, -1, "7.bvh");
        debugRotationsVersion( 1,  1,  1, "8.bvh");
    }
    
    private void debugRotationsVersion(int x, int y, int z, string filename)
    {
        string s = HierarchyBody;
        s += "MOTION\nFrames: " + base_rotationFiles[0].Count + "\nFrame Time: " + 0.0083333 + "\n";
        foreach (Rotations r in base_rotationFiles[0])
        {
            s += "0 0 0 ";
            List<Vector3> v = r.getAllRotations();
            foreach (Vector3 eul in v)
            {
                s+= z*eul.z + " " + x*eul.x + " " + y*eul.y + " ";
            }
            s += "\n";
        }
        System.IO.File.WriteAllText(filename, s);
    }
}
