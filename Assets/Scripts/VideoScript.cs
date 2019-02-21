using UnityEngine;
using System.Collections;
using System.IO;

public class VideoScript : MonoBehaviour {

	// capture number
	private int prefix;

	public bool play = false;

	// number of image
	private int i;
	
	public int frameRate = 30;

    public string savePath = "./capture";

	void Start()
	{
        System.IO.Directory.CreateDirectory(savePath);
        Debug.Log("Saving Images in: " + savePath);
		i = 0;

        Time.captureFramerate = frameRate;
    }

	// to encode the resulting video use something like:
	// 
	// mencoder.exe mf://*.png -mf fps=30:type=png -vf scale=720:480 -ovc x264 -x264encopts preset=slow:tune=animation:crf=20 -o video.avi
	// use mode of running the application "Fantastic" for maximum quality
	// note that the images below in CaptureScreenshot are saved twice as large to increase the quality of final video. 
	//
	// Update is called once per frame
	void LateUpdate ()
	{
		if (play)
		{
            // capture screenshot
            ScreenCapture.CaptureScreenshot(savePath + "/" + i.ToString("D8")+ ".png", 1);
			i++;
		}
	}

    /*
	void OnApplicationQuit()
	{
	}
    */
}