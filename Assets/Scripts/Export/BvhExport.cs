using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System.Text;
using System.IO;
using System;
using System.Globalization;


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
        int counter = 0;
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
            rootRotation = getFinalRootRotation(rotations[0],angle,counter);
            //rootRotation = Model3D.BvhToUnityRotation(rootRotation, Model3D.AxisOrder.XYZ).eulerAngles;
            lines.Add(CreateMLine(Vector3.zero, rootRotation, rotations));
            counter++;
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
        // Quaternion qBefore = y * x * y; // Multiply them in the rotation order, in case of ZXY.


        return Model3D.getRootRotation_Euler(joints);
    }


    private Vector3 getFinalRootRotation(Vector3 rotationFromBvh, float angle=0, int counter=0)
    {


        // [INITIALIZE]: Convert Euler to Quaternion for each axis. 
        Quaternion qX = Quaternion.AngleAxis(rotationFromBvh.x, Vector3.right);
        Quaternion qY = Quaternion.AngleAxis(rotationFromBvh.y, Vector3.up);
        Quaternion qZ = Quaternion.AngleAxis(rotationFromBvh.z, Vector3.forward);
        Quaternion qBefore = qY * qX * qZ; // Multiply them in the rotation order, in case of ZXY.
        //qBefore = Quaternion.Euler(rotationFromBvh);

        // [ROTATE]: Get the quaternion of rotating around y axis.
        Quaternion YawRotationQuaternion = Quaternion.AngleAxis(angle, Vector3.up);

        // [DEBUG ANGLE]
        Vector3 debugangleaxis; float debugangle;
        YawRotationQuaternion.ToAngleAxis(out debugangle, out debugangleaxis);
        //Debug.Log(angle + " >> ANGLE AXIS: "+ debugangleaxis + "debug angle: "+debugangle);

        // [APPLY ROTATION AROUND Y]
        Quaternion qAfter;
        qAfter = qBefore;
        qAfter = qY * YawRotationQuaternion * qX * qZ;
        //qAfter = qBefore * YawRotationQuaternion;

        // [EXTRA CALCULATIONS]:
        float signedAngle = GetSignedAngle(qBefore, qAfter, Vector3.up);
        // Angle axis of signed angle:
        Vector3 angleAxis;
        Quaternion x = (qAfter * Quaternion.Inverse(qBefore));
        x.ToAngleAxis(out angle, out angleAxis);
        // more
        Quaternion qInverseAfter = Quaternion.Inverse(qAfter);
        Quaternion qCenterBefore = Quaternion.Inverse(qBefore);
        float y_RotationAngle = Quaternion.Angle(qBefore, qAfter);
        float y_AngleBefore = qBefore.eulerAngles.y;
        float y_AngleAfter = qAfter.eulerAngles.y;

        // [RETURN IN XYZ FORM]
        return qAfter.eulerAngles;
    }


    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        angle = angle * Mathf.Deg2Rad;
        Quaternion quaternion;
        float num2 = angle * 0.5f;
        float num = (float)Mathf.Sin((float)num2);
        float num3 = (float)Mathf.Cos((float)num2);
        quaternion.x = axis.x * num;
        quaternion.y = axis.y * num;
        quaternion.z = axis.z * num;
        quaternion.w = num3;
        return quaternion;

    }


    private static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
    {
        Quaternion quaternion;
        double x = quaternion1.x;
        double y = quaternion1.y;
        double z = quaternion1.z;
        double w = quaternion1.w;
        double num4 = quaternion2.x;
        double num3 = quaternion2.y;
        double num2 = quaternion2.z;
        double num = quaternion2.w;
        double num12 = (y * num2) - (z * num3);
        double num11 = (z * num4) - (x * num2);
        double num10 = (x * num3) - (y * num4);
        double num9 = ((x * num4) + (y * num3)) + (z * num2);
        quaternion.x = (float) (((x * num) + (num4 * w)) + num12);
        quaternion.y = (float) (((y * num) + (num3 * w)) + num11);
        quaternion.z = (float) (((z * num) + (num2 * w)) + num10);
        quaternion.w = (float) ((w * num) - num9);
        return quaternion;
    }


    public static float GetSignedAngle(Quaternion A, Quaternion B, Vector3 axis)
    {
        float angle = 0f;
        Vector3 angleAxis = Vector3.zero;
        Quaternion x = (B * Quaternion.Inverse(A));
        x.ToAngleAxis(out angle, out angleAxis);
        if (Vector3.Angle(axis, angleAxis) > 90f)
        {
            angle = -angle;
        }
        // Debug.Log("Angle axis: "+angleAxis +"angle: "+angle+" singed angle: "+Mathf.DeltaAngle(0f,angle));
        return Mathf.DeltaAngle(0f, angle);
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












    // ------------- TESTING AREA --------------------
    // Get the rotations from file.

    public BvhExport(string templateFilename, string rotationFile, string output)
    {

        List<Rotations> rotationsInFile = InitializeRotations(rotationFile);        // Get rotations


        // Hierarchy body
        TemplateFilename = templateFilename;
        HierarchyBody = FileToString(TemplateFilename);


        // Create motion body
        int counter = 0;
        MotionLines = new List<string>();
        foreach (Rotations frameRotations in rotationsInFile)
        {
            List<Vector3> rotations = frameRotations.getAllRotations();
            Vector3 defaultRootRotation = new Vector3(90, 90, 0); // Default. Please don't delete this. It is useful to debug.
            Vector3 rootRotation = defaultRootRotation;
            float angle = 180;
            rootRotation = getFinalRootRotation(rotations[0], angle, counter);
            MotionLines.Add(CreateMLine(Vector3.zero, rootRotation, rotations));
            counter++;
        }


        // Motion body string total
        StringBuilder s = new StringBuilder();
        s.Append("MOTION\nFrames: " + MotionLines.Count + "\nFrame Time: " + 0.0083333 + "\n");
        // First line T-pose
        s.Append("0 0 0 -0 90 90 9.69808e-018 1.34041e-020 0.158382 1.41245e-030 7.06225e-031 2.48481e-017 -7.30747e-017 7.61111e-019 -1.19349 3.40995e-017 1.65719e-019 0.556895 -2.12238e-008 -1.53545e-005 -90.1584 -2.50273e-005 -2.47192e-012 6.55612e-006 1.52588e-005 -6.98387e-015 174.455 180 -4.60993e-005 -89.8417 -5.49628e-005 -2.25992e-011 3.612e-005 1.52588e-005 6.98387e-015 174.455 -3.94791 -0.248796 -175.995 -8.56334 0.17593 -0.870008 4.60942 -0.0775208 176.868 177.984 -0.125371 -176.036 -4.37326 0.0836638 -0.877309 2.35438 -0.0332512 176.598\n");
        foreach (string line in MotionLines)
        {
            s.Append(line);
        }

        System.IO.File.WriteAllText(output, HierarchyBody + s);

}

    private static List<Rotations> InitializeRotations(string fileName)
    {
        List<Rotations> rotationsFile = new List<Rotations>();
        StreamReader sr = File.OpenText(fileName);
        string tuple = string.Empty;
        while ((tuple = sr.ReadLine()) != null)
        {
            rotationsFile.Add(Base.StringToRotations(tuple));
        }
        sr.Close();

        Debug.Log(">Rotations have been read.");
        return rotationsFile;
    }



}
