using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEventArgs : EventArgs
{
    public readonly Camera cam;
    public CameraEventArgs(Camera c)
    {
        cam = c;
    }
}
public class OnCameraPostRenderEventRaiser : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
    }

    public event EventHandler<CameraEventArgs> OnPostRenderEvent;

    private void OnPostRender()
    {
        OnPostRenderEvent?.Invoke(this , new CameraEventArgs(_cam));
    }
}
