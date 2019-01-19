using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBVH5 : MonoBehaviour
{

    string bvhOutput;
    public string savePath = "C:\\animations\\animation.bvh";
    public int fps = 60;

    Quaternion rootRotationOffset;

    //ARMATURE BONES SECTION
    public Transform hip;
    public Transform spine;
    public Transform chest;

    public Transform shoulderR;
    public Transform upper_armR;

    public class DecoratedBone
    {
        public Transform bone;
        public DecoratedBone parent; // decorated bone parent, set in a later loop
        public Vector3 rest_bone_head; // blender armature bone head position
        public Matrix4x4 rest_arm_mat; // blender rest matrix (armature space)
        public Matrix4x4 rest_arm_imat; // rest_arm_mat inverted
    }
    List<DecoratedBone> dBones;

    //INITIALIZATION SECTION
    void OnEnable()
    {
        dBones = new List<DecoratedBone>();
        rootRotationOffset = hip.rotation;

        int level = 0;

        bvhOutput = "HIERARCHY\n";
        bvhOutput += "ROOT hip\n";

        var dBone = new DecoratedBone();
        dBone.bone = hip;
        dBone.rest_bone_head = hip.position;

        var rest_arm_mat = GetMatrix(dBone, rootRotationOffset);

        dBone.rest_arm_mat = rest_arm_mat;
        dBone.rest_arm_imat = rest_arm_mat.inverse;

        dBones.Add(dBone);

        bvhOutput += "{\n";
        var offset = hip.position;
        bvhOutput += "\tOFFSET " + offset.x.ToString("F6") + " " + (-offset).z.ToString("F6") + " " + offset.y.ToString("F6") + "\n";
        bvhOutput += "\tCHANNELS 6 Xposition Yposition Zposition Xrotation Yrotation Zrotation\n";

        var spineBone = InsertHierarchy(ref level, "spine", spine, dBone);
        InsertHierarchy(ref level, "chest", chest, spineBone);
        AscendLevel(ref level, chest, chest.GetChild(0), 1);
        var shoulderRBone = InsertHierarchy(ref level, "shoulder.R", shoulderR, spineBone);
        InsertHierarchy(ref level, "upper_arm.R", upper_armR, shoulderRBone);
        AscendLevel(ref level, upper_armR, upper_armR.GetChild(0));

        bvhOutput += "}\n";

        bvhOutput += "MOTION\n";
        bvhOutput += "Frames: 1\n";
        bvhOutput += "Frame Time:\t" + (1.0f / fps).ToString("F6") + "\n";

        StartCoroutine(RecordRoutine()); // Finished building header info, Begin recording keyframes
    }

    void OnDestroy()
    {
        System.IO.File.WriteAllText(savePath, bvhOutput);
        print("SAVED FILE!!!");
    }

    // HEADER SECTION HELPERS
    DecoratedBone InsertHierarchy(ref int level, string name, Transform bone, DecoratedBone parent)
    {
        /* Creats hierarchy header info for bones with parents */
        string tabs = GetLevelTabs(level);
        bvhOutput += tabs + "JOINT " + name + "\n";
        bvhOutput += tabs + "{\n";
        var dBone = new DecoratedBone();
        dBone.bone = bone;
        dBone.parent = parent;

        dBone.rest_bone_head = bone.position;
        var rest_arm_mat = GetMatrix(dBone, rootRotationOffset);

        dBone.rest_arm_mat = rest_arm_mat;
        dBone.rest_arm_imat = rest_arm_mat.inverse;
        dBones.Add(dBone); // Add bone to the list of bones (in the order it was created in the hiereachy/header).

        var offset = bone.position - parent.bone.position;
        bvhOutput += tabs + "\tOFFSET " + offset.x.ToString("F6") + " " + (-offset).z.ToString("F6") + " " + offset.y.ToString("F6") + "\n";
        bvhOutput += tabs + "\tCHANNELS 3 Xrotation Yrotation Zrotation\n";
        level++; // Increase the level in the hierarchy (increases depth in .bvh file and ensures curly braces will be added properly)

        Debug.DrawLine(parent.bone.position, bone.position, Color.cyan, 10.0f); // Outline the skeleton in the viewport for debugging
        return dBone;
    }

    string GetLevelTabs(int level)
    {
        string tabs = "";
        for (int lev = 0; lev <= level; lev++)
        {
            tabs += "\t";
        }
        return tabs;
    }

    void EndSite(int level, Transform bone, Transform endEffector)
    {
        string tabs = GetLevelTabs(level);
        bvhOutput += tabs + "End Site\n";
        bvhOutput += tabs + "{\n";
        var offset = endEffector.position - bone.position;
        bvhOutput += tabs + "\tOFFSET " + offset.x.ToString("F6") + " " + (-offset).z.ToString("F6") + " " + offset.y.ToString("F6") + "\n"; // 1 0 0\n";
        bvhOutput += tabs + "}\n";

        Debug.DrawRay(bone.position, offset, Color.cyan, 10.0f);
    }

    void AscendLevel(ref int level, Transform bone, Transform endEffector, int stopLevel = 0)
    {
        EndSite(level, bone, endEffector);

        for (int lev = level; lev > stopLevel; lev--)
        {
            for (int tab = 0; tab < lev; tab++)
            {
                bvhOutput += "\t";
            }
            bvhOutput += "}\n";
        }
        level = stopLevel;
    }


    //RECORDED MOTION SECTION
    IEnumerator RecordRoutine()
    {
        /*A simplified re-write of Blender's export_bvh.py script https://github.com/sftd/blender-addons/blob/master/io_anim_bvh/export_bvh.py*/
        string line; // The string used to build each line of data.
        float time = Time.time;
        float interval = 1.0f / fps; // The record frame interval.
        while (isActiveAndEnabled)
        { // Runs while the scene is actively running.
            yield return new WaitUntil(() => (Time.time - time) >= interval); // Wait for desired time interval.
            time = Time.time; // Reset the timer.
            yield return new WaitForEndOfFrame(); // Wait for all transforms to be updated.

            line = ""; // Clear the line data and prepare for next line of data.

            //First do calculations for the root (hip) bone.
            var p = dBones[0].rest_bone_head;
            var trans = Matrix4x4.Translate(p);
            var itrans = Matrix4x4.Translate(-p);

            var pose_mat = GetMatrix(dBones[0], rootRotationOffset);
            var rest_arm_imat = dBones[0].rest_arm_imat;

            var mat_final = pose_mat * rest_arm_imat;
            mat_final = itrans * mat_final * trans;

            var loc = Vector3.zero; // mat_final.GetPosition(); 
            var rot = Vector3.zero; // mat_final.GetRotation().eulerAngles; 
            line += // Add root's (hip bone) data to the line of data.
                (-loc.x).ToString("F6") + " " + //Output location of root bone
                (-loc.z).ToString("F6") + " " +
                (loc.y).ToString("F6") + " " +

                (rot.x).ToString("F6") + " " + //Output rotation of root bone
                (rot.z).ToString("F6") + " " +
                (-rot.y).ToString("F6") + " ";

            // Now do calculations for each child bone
            for (int i = 1; i < dBones.Count; i++)
            {

                p = dBones[i].rest_bone_head;
                trans = Matrix4x4.Translate(p);
                itrans = Matrix4x4.Translate(-p);

                mat_final =
                    dBones[i].parent.rest_arm_mat * GetMatrix(dBones[i].parent).inverse *
                    GetMatrix(dBones[i]) * dBones[i].rest_arm_imat;
                mat_final = itrans * mat_final * trans;

                //loc = mat_final.GetPosition () + (trackedObjects [i].rest_bone_head - trackedObjects [i].parent.rest_bone_head); //Position is not required for non-root bones
                rot = Vector3.zero;//mat_final.GetRotation().eulerAngles;

                line += // Add children's data to the line of data.
                    (rot.x).ToString("F6") + " " + //Output rotation of child bone
                    (rot.z).ToString("F6") + " " +
                    (-rot.y).ToString("F6") + " ";
            }
            line += "\n"; // Done adding line.

            if (Application.isPlaying)
            {
                bvhOutput += line; // Add the data to the file's contents only if the scene is actively running.
            }

        }
    }

    Matrix4x4 GetMatrix(DecoratedBone dBone, Quaternion offsetRotation = default(Quaternion))
    {
        //return trackedObject.obj.localToWorldMatrix;
        return Matrix4x4.TRS(
            dBone.bone.position,
            offsetRotation.Equals(default(Quaternion)) ? dBone.bone.rotation : dBone.bone.rotation * offsetRotation, //the offsetRotation (aka hip.rotation) should be applied to all bones with parents.
            Vector3.one
        );
    }
}