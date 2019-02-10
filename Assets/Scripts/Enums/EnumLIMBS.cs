using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
// This is the order of the bones being saved in file.
public enum EnumBONES { UP_TORSO, RIGHT_TORSO, LEFT_TORSO, SPINE_TO_R_UPLEG, SPINE_TO_L_UPLEG, PELVIS, TORSO_HEIGHT}


public class ConfigureLIMBS
{
    public static int[,] pairs = new int[7, 2] { 
        { 2, 5 },   // 0. UP_TORSO 
        { 2, 8 },   // 1. RIGHT_TORSO 
        { 5, 11},   // 2. LEFT_TORSO 
        { 1, 8 },   // 3. SPINE_TO_R_UPLEG
        { 1, 11},   // 4. SPINE_TO_L_UPLEG
        { 8, 11},   // 5. PELVIS
        { 1, -1}    // 6. TORSO_HEIGHT (-1 point is the root (8 + 11) / 2 )
    };

    // TEST A
    public static EnumBONES[] bonesToUse_A = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.RIGHT_TORSO, EnumBONES.LEFT_TORSO, EnumBONES.SPINE_TO_R_UPLEG, EnumBONES.SPINE_TO_L_UPLEG, EnumBONES.PELVIS, EnumBONES.TORSO_HEIGHT };
    // TEST B
    public static EnumBONES[] bonesToUse_B = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.RIGHT_TORSO, EnumBONES.LEFT_TORSO, EnumBONES.PELVIS, EnumBONES.TORSO_HEIGHT };
    // TEST C
    public static EnumBONES[] bonesToUse_C = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.RIGHT_TORSO, EnumBONES.LEFT_TORSO, EnumBONES.SPINE_TO_R_UPLEG, EnumBONES.SPINE_TO_L_UPLEG, EnumBONES.PELVIS };
    // TEST D
    public static EnumBONES[] bonesToUse_D = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.PELVIS, EnumBONES.TORSO_HEIGHT };
    // TEST E
    public static EnumBONES[] bonesToUse_E = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.SPINE_TO_R_UPLEG, EnumBONES.SPINE_TO_L_UPLEG, EnumBONES.PELVIS };
    // TEST F
    public static EnumBONES[] bonesToUse_F = new EnumBONES[] { EnumBONES.UP_TORSO, EnumBONES.RIGHT_TORSO, EnumBONES.LEFT_TORSO, EnumBONES.PELVIS };
    // ALL TESTS
    public static EnumBONES[][] tests = new EnumBONES[][] { bonesToUse_A, bonesToUse_B, bonesToUse_C, bonesToUse_D, bonesToUse_E, bonesToUse_F };

    // Scale Factor
    public static float correctScaleFactor = 0.25f;

    public static float[] runTestGetErrors(Vector3[] figure3D, float[] errors, int[][] bonePreferences)
    {


        // Initialise 3D Scaled Figure
        List<Vector3> scaledFigure = new List<Vector3>();
        foreach(Vector3 joint in figure3D)
        {
            scaledFigure.Add(joint * correctScaleFactor);
        }
        // Get bone sizes
        List<float> boneSizes = Scaling.getBonesSizes(scaledFigure.ToArray());

        // Initialise 2D figure
        List<Vector3> Figure2D = new List<Vector3>();
        foreach (Vector3 joint in figure3D)
        {
            Figure2D.Add(new Vector3(joint.x, joint.y, 0f));
        }


        // Get scale factor for each test
        //foreach (EnumBONES[] test in tests)
        for(int i=0; i<tests.Length; i++)
        {
            EnumBONES[] test = tests[i];
            float sf;
            EnumBONES bone;
            Scaling.getGlobalScaleFactor_USING_LIMBS_TESTS(Figure2D.ToArray(),test, boneSizes, bonePreferences[i], out bone,out sf);
            errors[i] += Mathf.Abs(correctScaleFactor - sf);
            //Debug.Log("Error is "+errors[i] + " sf is: "+ sf + "correctScaleFactor: "+correctScaleFactor+ "bones: "+bonePreferences[i].ToString());
        }
        return errors;
    }

    private static List<BvhProjection> data;
    private static int iterations = 3000; 
    public static void runTests() {

        data = Base.base_representatives;

        if (data == null)
        {
            Debug.Log("Data is null! Exiting...");
            return;
        }

        // Where to save errors
        float[] errors = new float[tests.Length];
        // Save bone preference
        int[][] bonePreference = new int[tests.Length][];
        // (int[] test in bonePreference)  // init
        for(int i=0; i<bonePreference.Length; i++)
        {
            bonePreference[i] = new int[7]; // init
        }

        for(int projectionIndex = 0; projectionIndex < iterations; projectionIndex++)
        {
            // Retrieve figure
            Vector3[] figure = data[projectionIndex].joints;
            runTestGetErrors(figure, errors, bonePreference); 
        }

        // Print results
        // Get averages
        int counter = 0;
        string s = "";
        foreach (float error in errors) // Iterate each DOKIMI
        {
            s += "Dokimi " + counter + ": Mean = " + (error / (float)iterations) + "\n";
            for (int k = 0; k < bonePreference[counter].Length; k++)
            {
                String boneName = ((EnumBONES)k).ToString();
                s += boneName + ": " + bonePreference[counter][k] + "\n";
            }
            s += "\n\n";
            counter++;
        }
        Debug.Log(s);

    }

};
