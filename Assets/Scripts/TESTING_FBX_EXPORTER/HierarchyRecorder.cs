using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class HierarchyRecorder : MonoBehaviour
{
    // The clip the recording is going to be saved to.
    private AnimationClip clip;
    public string Clipfilename;

    // Checkbox to start/stop the recording.
    public bool record = false;

    // The main feature: the actual recorder.
    private GameObjectRecorder m_Recorder;

    void Start()
    {
        // Create the GameObjectRecorder.
        m_Recorder = new GameObjectRecorder(gameObject);

        // Bind all the Transforms on the GameObject and all its children.
        m_Recorder.BindComponentsOfType<Transform>(gameObject, true);
    }

    // The recording needs to be done in LateUpdate in order
    // to be done once everything has been updated
    // (animations, physics, scripts, etc.).
    void LateUpdate()
    {
        if (Clipfilename == null)
            return;

        if (record)
        {
            // As long as "record" is on: take a snapshot.
            m_Recorder.TakeSnapshot(Time.deltaTime);
        }
        else if (m_Recorder.isRecording)
        {
            // Create clip in the specified directory
            clip = new AnimationClip();

            // "record" is off, but we were recording:
            // save to clip and clear recording.
            m_Recorder.SaveToClip(clip);
            m_Recorder.ResetRecording();
            
            // Save clip.
            AssetDatabase.CreateAsset(clip, @"Assets/Clips/" + Clipfilename + "ANIM.anim");
            Debug.Log("Animation clip has been created.");


        }
    }
}