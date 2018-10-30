using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System.Text;

/* Important:
   Make sure that: the template, and all the bvh used in this program, have the same HIERARCHY. */

public class BvhExport : Base
{

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


    private List<string> CreateMotionLines()
    {
        List<string> lines = new List<string>();
        foreach(Neighbour n in Estimation)
        {
            List<Vector3> rotations = base_rotationFiles[n.projection.rotationFileID][n.projection.frameNum].getAllRotations();
            lines.Add(CreateMLine(Vector3.zero, new Vector3(-0,90,90), rotations));
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

    private string CreateMotionBody()
    {
        StringBuilder s = new StringBuilder();
        s.Append("MOTION\nFrames: " + MotionLines.Count + "\nFrame Time: " + 0.0083333 + "\n");
        // First line T-pose
        s.Append("6.90105 11.8871 -0.957331 -0 90 90 9.69808e-018 1.34041e-020 0.158382 1.41245e-030 7.06225e-031 2.48481e-017 -7.30747e-017 7.61111e-019 -1.19349 3.40995e-017 1.65719e-019 0.556895 -2.12238e-008 -1.53545e-005 -90.1584 -2.50273e-005 -2.47192e-012 6.55612e-006 1.52588e-005 -6.98387e-015 174.455 180 -4.60993e-005 -89.8417 -5.49628e-005 -2.25992e-011 3.612e-005 1.52588e-005 6.98387e-015 174.455 -3.94791 -0.248796 -175.995 -8.56334 0.17593 -0.870008 4.60942 -0.0775208 176.868 177.984 -0.125371 -176.036 -4.37326 0.0836638 -0.877309 2.35438 -0.0332512 176.598\n");
        foreach (string line in MotionLines)
        {
            s.Append(line);
        }
        return s.ToString();
    }


    public void CreateBvhFile(string filename)
    {
        debugRotations();
        System.IO.File.WriteAllText(filename, HierarchyBody + MotionBody);
    }

    public void debugRotations()
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
