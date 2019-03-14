using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class OpenPoseJSON {

    
    private readonly List<OPFrame> frames;          // The reference of Video Frames.
    private static FigureIdentifier figureIdentifier = Base.figureIdentifier;

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
        // Iterate poses in json Object : For each figure in the frame.
        int currentFigureIndex = 0;

        foreach (Keypoints k in jsonObj.people)
        {
            // Initialise a pose, fill bodypositions, normalize data, identification : all done in contructor.
            // Use last frames, to interpolate scaling.
            OPPose pose = new OPPose(k, frames, currentFrameIndex, currentFigureIndex);
            frame.figures.Add(pose);
            currentFigureIndex++; // Go to the next figure in the same frame.
        }
        frame.figures = figureIdentifier.sortFiguresInFrame(frame);
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
        Debug.Log(str);
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



























    ///* ::::::::::::::::::::::: FIND WHICH FIGURES ARE USELESS AND REMOVE THEM :::::::::::::::::: */



    ///* :::::::::::::::::::::::: 2. PROBLEM OF FILLING THE BLANKS ::::::::::::::::::::::::::: */



    

    //public List<Person> getPeople()
    //{
    //    giveIDsToFigures();
    //    return people;
    //}
   
    ///* :::::::::::::::::::::: 3. PROBLEM OF IDENTIFYING PEOPLE IN IMAGE ::::::::::::::::::::::*/
    ///* _______________________________ Select people ___________________________________ */
    //private void giveIDsToFigures()
    //{
    //    int c = 0;

    //    /* First frame: */
    //    /* Assign a Person to each figure. */
    //    foreach(OPPose p in frames[0].figures)
    //    {
    //        Person person = new Person(c);
    //        c++;
    //        person.poses.Add(p);
    //        people.Add(person);
    //    }
   

    //    /* If figure does not correspond to any of the Persons, then create a new person. */
    //    for (int i=1; i<frames.Count; i++)
    //    {
    //        foreach(OPPose p in frames[i].figures)
    //        {
    //            /* Assign the pose to a Person. */
    //            Person personToBeIdentifiedWith = getClosestPerson(p,i);
    //            if(personToBeIdentifiedWith == null)
    //            {
    //                Person newPerson = new Person(c);
    //                c++;
    //                newPerson.poses.Add(p);
    //                people.Add(newPerson);
    //            }
    //            else
    //            {
    //                personToBeIdentifiedWith.poses.Add(p);
    //            }
    //        }
    //    }
    //}



    ///* Returns the nearest Person (from people) of the pose. 
    // * If Person is not found, then returns NULL.
    // */
    //private Person getClosestPerson(OPPose pose, int frameNum)
    //{
    //    float min = 0;
    //    Person closestPerson = null;
    //    foreach(Person p in people)
    //    {
    //        /* Be carefull not to get out of the pose array. */
    //        if (frameNum - 1 >= p.poses.Count)
    //            continue;

    //        float distance = distanceOf2Poses(p.poses[frameNum - 1],pose);
    //        if (distance < min)
    //        {
    //            min = distance;
    //            closestPerson = p;
    //        }
    //    }

    //    /* Now we got the closest person.
    //     * But is it the same person?
    //     */
    //    if (min > 100)
    //        return null;

    //    return closestPerson;
    //}

    ///* Returns the sum of the distance of each joint, of 2 poses. */
    ///*
    //private float distanceOf2Poses(OPPose p1, OPPose p2)
    //{
    //    float sum = 0;
    //    for(int i=0; i<KEYPOINTS_NUMBER; i++)
    //    {
    //        Vector2 j1 = p1.jointsRAW[i]; // USE RAW JSON FILE
    //        Vector2 j2 = p2.jointsRAW[i];
    //        // TODO: What I'll do in that case?
    //        if ((j1.x == 0 && j1.y == 0) || (j2.x == 0 && j2.y == 0))
    //            continue;

    //        sum += Vector2.Distance(j1,j2);
    //    }
    //    return sum;
    //}
    //*/
    ///* ______________________________________________________________________________________ */








    ///* Debug frames. */
    //private void debugFrames()
    //{
    //    /* Debug Frames */
    //    foreach (OPFrame f in frames)
    //    {
    //        StringBuilder s = new StringBuilder();
    //        foreach (OPPose p in f.figures)
    //        {
    //            s.Append(p);
    //        }

    //        Debug.Log("Frame[" + f.number+"]\n"+s);
    //    }
    //}



   
    ///* Debug people. */
    //private void debugPeople()
    //{

    //    foreach(Person person in people)
    //    {
    //        StringBuilder s = new StringBuilder();
    //        foreach (OPPose pose in person.poses)
    //        {
    //            s.Append(pose);
    //        }
    //        Debug.Log("Person[" + person.id + "]\n" + s);
    //    }
    //}



















}
