using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform destination;
    public Transform camera;
    public float camSpeed;
    
    void Update()
    {

        camera.position = new Vector3(destination.position.x, destination.position.y, -10);

    }
}
