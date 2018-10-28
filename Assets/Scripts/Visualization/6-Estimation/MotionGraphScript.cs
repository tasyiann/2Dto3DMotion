using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MotionGraphScript : MonoBehaviour {


    GLDraw gL;
    Neighbour[] estimation;
    int CurrentFrame;
    VMEstimation vmestimation;
    public EnumJoint chosenJoint;
    public GameObject go;
    public Text title;
    public Material material;
    private void OnPostRender()
    {
        if (vmestimation != null)
            CurrentFrame = vmestimation.ChooseProjection;
        else
            Debug.Log("modeldebug is null");

        if (estimation != null)
        {
            gL.drawMotionGraphAllJoints(estimation, (int)chosenJoint, CurrentFrame);
        }
        else
            estimation = DataParsing.estimation;
    }


    private void Start()
    {
        gL = new GLDraw(material);
        vmestimation = go.GetComponent(typeof(VMEstimation)) as VMEstimation;
    }


    private void LateUpdate()
    {
        // Change camera position
        transform.position = new Vector3(CurrentFrame * 0.2f + 0.2f, transform.position.y, transform.position.z);
        // Change Title
        title.text = chosenJoint.ToString();
    }
}
