using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Winterdust;

public class BvhOutput : MonoBehaviour {


	void Start () {
        previewBvh(Base.base_CurrentDir);
	}
	

    public void previewBvh(string dir)
    {
        /* Get the list of files in the directory. */
        if (dir == null)
        {
            Debug.Log("Invalid directory! Can not find bvh file.");
            return;
        }
        Debug.Log("Directory: "+dir);
        string[] fileEntries = Directory.GetFiles(dir);
        string bvhfilename=null;
        foreach (string fileName in fileEntries)
        {
            Debug.Log("File entry: " + fileName);
            string extension = Path.GetExtension(fileName);
            Debug.Log("File extension: " + extension);
            if (extension.CompareTo(".bvh") == 0)
            {
                bvhfilename = fileName;
            }

        }
        if (bvhfilename == null)
        {
            Debug.Log("Bvh not found in this directory!");
        }
        else
        {
            BVH bvh = new BVH(bvhfilename);
            bvh.makeDebugSkeleton();
        }
    }
}
