using UnityEngine;
using System.Collections;

public class cameraScreenResolution : MonoBehaviour
{
    public bool mantainWidth = true;
    [Range(-1,1)]
    public int adaptPosition;

    float defaultWidth;
    float defaultHeight;

    Vector3 CameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CameraPos = Camera.main.transform.position;
        defaultHeight = Camera.main.orthographicSize;
        defaultWidth = Camera.main.orthographicSize * Camera.main.aspect;
    }

    // Update is called once per frame
    void Update()
    {
     if (mantainWidth) {
        Camera.main.orthographicSize = defaultWidth / Camera.main.aspect;
        Camera.main.transform.position = new Vector3(CameraPos.x,adaptPosition*(defaultHeight - Camera.main.orthographicSize), CameraPos.z);
     }  else {
        Camera.main.transform.position = new Vector3(adaptPosition*(defaultWidth - Camera.main.orthographicSize * Camera.main.aspect), CameraPos.y, CameraPos.z);
     } 
    }
}
