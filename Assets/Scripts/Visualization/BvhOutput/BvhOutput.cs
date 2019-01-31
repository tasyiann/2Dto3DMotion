using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Winterdust;

public class BvhOutput : MonoBehaviour {

    public Transform t_camera;
    private Vector3 position;
    BVH bvh = null;
    GameObject skeleton = null;
    private int currentFrame = -1;
    int numOfFrames = 0;
    public VMEstimation estimationScript;


    void Start () {

        initialiseBVH(Base.base_CurrentDir);        // Initialise BVH instance.
        position = t_camera.position;               // Set the position of bvh.
        skeleton.transform.position = new Vector3(position.x, position.y, position.z+35f);     // Translate the skeleton, so it is visible from camera.
        Debug.Log("Bvh length:" + bvh.frameCount);
	}

    private void Update()
    {
        currentFrame = estimationScript.ChooseProjection;

        // Set frame to skeleton
        if (skeleton != null)
        {
            bvh.moveSkeleton(skeleton, currentFrame);
        }


        
    }

    public void initialiseBVH(string dir)
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
            bvh = new BVH(bvhfilename);
            numOfFrames = bvh.frameCount;
            skeleton = bvh.makeDebugSkeleton(false, "ffffff", 0.5f);
            
        }
    }
}
