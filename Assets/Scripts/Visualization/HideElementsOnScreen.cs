using UnityEngine;
using System.Collections;

public class HideElementsOnScreen : MonoBehaviour
{

    public GameObject[] objectsToHide; // Assign in inspector
    
    private bool isShowing;

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            isShowing = !isShowing;
            foreach(GameObject obj in objectsToHide)
                obj.SetActive(isShowing);
        }
    }
}
