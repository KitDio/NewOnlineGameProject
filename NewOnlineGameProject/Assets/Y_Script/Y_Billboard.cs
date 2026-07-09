using UnityEngine;

public class Y_Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        Debug.Log("Camera = " + cam);
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;

            if (cam == null)
                return;
        }

        transform.forward = cam.transform.forward;
    }
}