using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform destination;       // The point the camera should follow
    public Transform camera;            // The camera that should follow the obejct
    public float camSpeed;              // The speed the camera should move
    
    void Update()
    {

        camera.position = new Vector3(destination.position.x, destination.position.y, -10);     // Moves the camera to the x and y of the destination, keeping it's z at -10

    }
}
