using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class Balance : MonoBehaviour
{

    public float targetRoatation;
    public Rigidbody2D rb;
    public float force;
    public GameObject burstPoint;

    void Update()
    {
        GrapplingScript grappleScript = burstPoint.GetComponent<GrapplingScript>();

        if (grappleScript.isGrappled == false)
        {
            rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetRoatation, force * Time.fixedDeltaTime));
        }

    }
}
