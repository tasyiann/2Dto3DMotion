using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System.Text;
using System.IO;
using System.Globalization;

public class TestingScript : MonoBehaviour {

    /* Gia to translation tou root, xrisimopoiite auto p epistrefetai apo to winderdust kai oxi apo to arxeio opws einai..*/
    private void fromFile()
    {
        string filename = "less\\OP_86_05.bvh";
        BVH bvh = new BVH(@filename);
        Debug.Log(">> "+bvh.pathToBvhFileee);
        /* Get rotations of the source file. */
        
        string alltext = System.IO.File.ReadAllText(filename);
        //string motion = alltext.Substring(alltext.LastIndexOf("Frame Time:"));
        // Debug.Log(motion);

        /* Try sth new*/
        string[] lines = File.ReadAllLines(filename);
        int index=0;
        while (!lines[index].StartsWith("Frame Time"))
            index++;
        index++; // skip that line
        /* So now, we have first line of rotations.*/

        List<Vector3[]> frames = new List<Vector3[]>();

        //int iteration = 0;
        while(index < lines.Length)
        {
           // Debug.Log(lines[index]);
           // Debug.Log(index + "\\" + lines.Length);
            Vector3[] rotations = new Vector3[bvh.boneCount];

            /* Get the numbers.*/
            string [] numText = lines[index].Split(' ');
            
            /* Get translation x,y,z of root*/
            /* which is 0,1,2 */
            float x=0, y=0, z=0;
            int boneIndex = 0;
            for(int j=3; j<=bvh.boneCount*3; j += 3)
            {
                z = float.Parse(numText[j], CultureInfo.InvariantCulture.NumberFormat);
                x = float.Parse(numText[j+1], CultureInfo.InvariantCulture.NumberFormat);
                y = float.Parse(numText[j+2], CultureInfo.InvariantCulture.NumberFormat);
            
                rotations[boneIndex] = new Vector3(x, y, z);
                boneIndex++;
            }
            frames.Add(rotations);
            //iteration++;
            index++;
        }





        string hirerarchy = System.IO.File.ReadAllText("template.bvh");




        // Debug frame
        StringBuilder d = new StringBuilder();
        d.Append("MOTION\nFrames: " + bvh.frameCount + "\nFrame Time: " + 0.0083333 + "\n");

        for (int i = 0; i < bvh.frameCount; i++)
        {
            // Translation of root
            Matrix4x4 m = bvh.allBones[0].getWorldMatrix(ref bvh.allBones, i); // from bvh
           // Debug.Log(bvh.allBones[0].localFramePositions[i]);
            d.Append(m.m03+" "+m.m13+" "+m.m23+" ");

            for (int p = 0; p < bvh.boneCount; p++)
            {
                // Rotations
                // Vector3 v = bvh.allBones[p].localFrameRotations[i].eulerAngles;
                Vector3 v = frames[i][p];
                d.Append(v.z + " " + v.x + " " + v.y + " ");
            }
            d.Append("\n");
        }
        System.IO.File.WriteAllText("mybvhcreation.bvh", hirerarchy+d.ToString());
    }

}
