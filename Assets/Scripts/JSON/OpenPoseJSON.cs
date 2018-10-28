using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class OpenPoseJSON {

    public static int figuresCounterInJson = 0;         // Counter of figures in a json file.
    //private static int KEYPOINTS_NUMBER = 14;         
    public List<OPFrame> frames = new List<OPFrame>();  // List of Frames.
    public List<Person> people = new List<Person>();    // List of people [THIS SHOULD BE OUR FINAL OBJECT].


    /* Parse all frames from a video. Takes the directory as a parameter. */
    public List<OPFrame> parseAllFiles(string path)
    {
        /* Get the list of files in the directory. */
        string[] fileEntries = Directory.GetFiles(path);
        foreach (string fileName in fileEntries)
        {
            if(Path.GetExtension(fileName).CompareTo(".json")==0)
                frames.Add(parsefile(fileName));
        }

        Debug.Log("Reading OpenPose figures.. Done!");
        return frames;
    }

    /* Parse just one frame. */
    public OPFrame parsefile(string path)
    {
        /* Reset the people counter. */
        figuresCounterInJson = 0;

        /* Parse Json file and Create a Json Object with all info */
        JsonFileStruct jsonObj = JsonConvert.DeserializeObject<JsonFileStruct>(File.ReadAllText(@path));

        /* Create the frame. */
        OPFrame frame = new OPFrame();

        /* Iterate poses in json Object */
        foreach (Keypoints k in jsonObj.people)
        {
            /* Initialise a pose */
            OPPose pose = new OPPose(figuresCounterInJson);
            frame.figures.Add(pose);
            // Fill person's bodypositions
            pose.fillBodyPositions(k);
            figuresCounterInJson++;
        }


        // Calculations na ginoun panw sta people.

        /* * * * * * * * DO THE CALCULATIONS ON PEOPLE. * * * *  * * */
        /* Convert x,y positions ----> positions from root */
        for(int i=0; i<frame.figures.Count; i++)
        {
            // Step 1: Might not be usefull.
            frame.figures[i].convertPositionsToRoot();
            // Step 2: Scaling is important.
            frame.figures[i].scalePositions();
        }
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

        return frame;
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
