using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tomis.UnityEditor.Utilities;
using UnityEngine;
using AnimationFilesHacker;

public class SequenceFromFileScript : MonoBehaviour
{
    [Header("Import Data")]
    [FileSelect(AssetRelativePath = true, ButtonName = "Input", SelectMode = FileSelectionMode.File)]
    public string inputPath;
    public Vector3 offsetBetweenFigures = new Vector3(4f, 0, 0);
    public int rate = 1;
    public Vector3 startingPoint;
    public GameObject prefab;

    private Vector3 position;
    private GameObject container;
    private List<AnimationFrame> frames;
    private List<GameObject> gameObjs = new List<GameObject>();
    private Model3D modelController;

    void Start()
    {
        LoadInput();
        InstantiateSkeletons();
    }


    private void LoadInput()
    {
        frames = new List<AnimationFrame>();
        if (inputPath != null && inputPath.Length != 0)
        {
            DataLoader loader = new DataLoader(inputPath);
            frames = loader.getAllFrames(DataLoader.DataType.UCY_Ik3D);
        }
    }


    private void InstantiateSkeletons()
    {
        container = new GameObject(Path.GetRandomFileName());
        int currentFrame = 0;
        while (currentFrame < frames.Count)
        {
            createFigure(currentFrame);
            currentFrame += rate;
        }
    }


    private void createFigure(int currentFrame)
    {
        GameObject go = Instantiate(prefab);
        new Model3D(go.transform).moveSkeleton(frames[currentFrame].SkeletonJoints.ToArray());
        go.transform.SetParent(container.transform);
        go.name = "figure" + currentFrame;
        go.transform.position = position;
        position += offsetBetweenFigures;
        gameObjs.Add(go);
    }


}
