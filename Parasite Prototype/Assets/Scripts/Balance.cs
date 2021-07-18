using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class Balance : MonoBehaviour
{

    public float targetRoatation;       // The rotation the limbs will attempt to reach
    public Rigidbody2D limb;            // The limb that should be trying to balance
    public float force;                 // The force applied the limb applies to try and reach the target rotation
    public GameObject burstPoint;       // The object containing the GrapplingScript

    void Update()
    {
        GrapplingScript grappleScript = burstPoint.GetComponent<GrapplingScript>();     // Get the grapple script so we can access its variables

        if (grappleScript.isGrappled == false)  // Runs if the player is not grappled
        {
            limb.MoveRotation(Mathf.LerpAngle(limb.rotation, targetRoatation, force * Time.fixedDeltaTime));        //Rotates the limbs towards the target rotation at a rate of force * time
        }

    }
}
