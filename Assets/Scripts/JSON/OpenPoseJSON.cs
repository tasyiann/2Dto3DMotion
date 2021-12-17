using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


public class OpenPoseJSON {

    
    private readonly List<OPFrame> frames;          // The reference of Video Frames.
    private FigureIdentifier figureIdentifier = new FigureIdentifier();

    public OpenPoseJSON(List<OPFrame> Frames)
    {
        frames = Frames;    // Set the reference of Frames, so we can apply changes on that.
                            // We keep track and store the frames of the video.
    }

    ///<summary>Parse all frames from a video.</summary>
    ///<param name="path">The directory path of OpenPose JSON files.</param>
    public List<OPFrame> ParseAllFiles(string path)
    {
        // Get the list of files in the directory.
        int frameIndexCounter = 0;
        string[] fileEntries = Directory.GetFiles(path);
        foreach (string fileName in fileEntries)
        {
            if (Path.GetExtension(fileName).CompareTo(".json") == 0)
            {
                frames.Add(Parsefile(fileName,frameIndexCounter));
                frameIndexCounter++;
            }
                
        }

        Debug.Log("Reading OpenPose figures.. Done!");
        return frames;
    }

    ///<summary>Parse just one frame from a video.</summary>
    ///<param name="path">The directory path of OpenPose JSON file.</param>
    public OPFrame Parsefile(string path, int currentFrameIndex)
    {
        // Parse Json file and Create a Json Object with all info.
        JsonFileStruct jsonObj = JsonConvert.DeserializeObject<JsonFileStruct>(File.ReadAllText(@path));
        OPFrame frame = new OPFrame();

        foreach (Keypoints k in jsonObj.people)
        {
            // Initialise a pose, fill bodypositions, normalize data, identification : all done in contructor.
            // Use last frames, to interpolate scaling.
            OPPose pose = new OPPose(k, frames, currentFrameIndex, figureIdentifier);
            frame.figures.Add(pose);
        }
        //frame.figures = figureIdentifier.sortFiguresInFrame(frame);
        sortFiguresByX(frame);



        //sortFiguresByTheirID(frame);
        // TODO: SORT THEM BY ID:)
        return frame;
    }


    private void sortFiguresByTheirID(OPFrame frame)
    {
        if (frame == null || frame.figures == null || frame.figures.Count == 0)
            return;

        SortedList<int, OPPose> list = new SortedList<int, OPPose>();
        foreach (OPPose pose in frame.figures)
            list.Add(pose.id,pose);

        frame.figures = new List<OPPose>(list.Values);
    }


    // Temporally used.
    private void sortFiguresByX(OPFrame frame)
    {
        if (frames == null || frame.figures == null|| frame.figures.Count<=1)
            return;

        SortedList<float, OPPose> list = new SortedList<float, OPPose>();
        string str = "";
        foreach (OPPose pose in frame.figures)
        {
            float keyX=0;
            
            for(int i=0; i<pose.jointsIMAGE.Length; i++)
            {
                if (pose.available[i])
                {
                    keyX = pose.jointsIMAGE[i].x;
                    str += keyX + " ";
                    break;
                }     
            }
            
            list.Add(keyX,pose);
        }
        // Debug.Log(str);
        // Assign the id.
        List<OPPose> listNew = new List<OPPose>(list.Values);
        int id = 0;
        foreach (OPPose pose in listNew)
        {
            pose.id = id;
            id++;
        }
        frame.figures = listNew;
    }

}
