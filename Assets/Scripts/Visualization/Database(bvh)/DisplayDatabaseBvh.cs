using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

using Winterdust;
public class DisplayDatabaseBvh : MonoBehaviour {

    private string dirName = Base.Path + "\\BvhFiles\\";
    private string info = "";
    public Text text;

	void Start () {
        loadBvhFiles();
	}
	

    void loadBvhFiles()
    {

        string[] fileEntries = Directory.GetFiles(dirName);
        int framesCount = 0;
        double timeCount = 0;
        foreach (string fileName in fileEntries)
        {
            if (fileName.EndsWith(".bvh"))
            {
                info += fileName + "\n";
                BVH bvh = new BVH(fileName);
                var random = new System.Random();
                var color = String.Format("#{0:X6}", random.Next(0x1000000));
                bvh.makeDebugSkeleton(true, color, 0.5f);
                framesCount += bvh.frameCount;
                timeCount += bvh.getDurationSec();
            }
        }
        info += "Total frames: " + framesCount+"\n";
        info += "Total duration: " + timeCount + " sec\n";
        text.text = info;
    }
}
