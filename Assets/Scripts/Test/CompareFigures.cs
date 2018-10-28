using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Winterdust;
using System.IO;
public class CompareFigures : MonoBehaviour {
    public Material Material;           // The material used in gl lines.
    public string directory;
    public Text title;
    public Image image;
    public bool showGrid;
    private GLDraw gL;                  // GL lines.
    OPPose pose;
    Vector3[] bvhJoints;


    private void Start()
    {
        gL = new GLDraw(Material);
        OpenPoseJSON op = new OpenPoseJSON();
        pose = op.parsefile(directory+"keypoints.json").figures[0];
        // T-Pose
        BvhReader br = new BvhReader(10, -24, "less");
        List<BvhProjection> list = br.parseBvhProjections("less\\TPose.bvh");
        Debug.Log("Sizeoflist:" + list.Count);
        bvhJoints = list[0].joints;



        byte[] bytes = File.ReadAllBytes(directory+"image.png");
        Texture2D texture = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1080, 1920), new Vector2(0.5f, 0.0f), 1.0f);

        image.GetComponent<UnityEngine.UI.Image>().sprite = sprite;

        title.text = directory;





    }


	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnPostRender()
    {
        if (showGrid)
        {
            gL.drawAxes(Color.gray);
        }


        gL.drawFigure(false,Color.red, pose.joints, pose.available, new Vector3(0, 0, 0));
        gL.drawFigure(true,Color.white, bvhJoints, null, new Vector3(0, 0, 0));
    }
}
