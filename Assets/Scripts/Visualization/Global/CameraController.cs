
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float scrollSpeed = 20.0f;

	// Update is called once per frame
	void Update () {


        Vector3 pos = transform.position;
        if (Input.GetKey("w"))// || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.y += panSpeed * Time.deltaTime; // Move relative to time, not to frameRate
        }
        if (Input.GetKey("s"))// || Input.mousePosition.y <=  panBorderThickness)
        {
            pos.y -= panSpeed * Time.deltaTime; // Move relative to time, not to frameRate
        }
        if (Input.GetKey("d"))// || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime; // Move relative to time, not to frameRate
        }
        if (Input.GetKey("a"))// || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime; // Move relative to time, not to frameRate
        }


        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.z += scroll * scrollSpeed *100f* Time.deltaTime;

        transform.position = pos;
	}
}
