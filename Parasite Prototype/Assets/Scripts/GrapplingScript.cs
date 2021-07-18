using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GrapplingScript : MonoBehaviour
{
    [Header("Layer Masks")]
    public LayerMask whatIsGrappleable;     // Assign the layer that stuff has to be on to be able to grapple to it
    public LayerMask ignoreLinecast;        // Assign the layer that stuff the linecast should ignore. The linecast scans between the player and the hook

    [Header("Assign Objects")]
    public Transform launchPointT;          // The point the tentacle originates from
    public Transform hook;                  // The object placed at the end of the tentacle
    public Collider2D hookCollider;         // The collider on the hook
    public GameObject player;               // The player gameobject
    public Camera playerCamera;             // The camera viewing the player

    [Header("Spring Settings")]
    public float jointDistance = 0.8f;      // The distance that the tentacle tries to maintain between the player and hook point (I think?)
    public float jointDamping = 7f;         // How much spring movement is reduced
    public float jointFrequency = 5f;       // How fast the spring oscillates

    [Header("Hook Settings")]
    public float hookDistance = 1f;         // Used by RopeBridge script to calculate the length of individual rope segments
    public float maxDistance = 20f;         // The max distance the tentacle can be shot out from the launchPointT;
    public float rangeReduction = 1f;       // Since the hook is a physical object, the edges of the hook sometimes extend further than the raycast. This float pulls it in a little bit so the furthest extent of the hook object will be reached by the raycast.
    public float hookSpeed = 0.1f;          // UNUSED VARIABLE - Ideally the hook should lerp from the origin point to the mouse when the player clicks instead of teleporting instantly. This is an artefact of me trying (and failing) to set that up.

    [Header("Bools")]
    public bool isGrappled = false;         // Returns true while the player is grappling to something
    public bool enemyStuck = false;         // Returns true while an enemy is impaled on the tentacle

    public CameraShake cameraShake;         // Accesses the variables in the Camera Shake script

    [Header("Private")]
    public RaycastHit2D grappleHit;         // A raycast used to see if the player hits something when they try to shoot out the tentacle
    private Vector3 mousePosition;          // Stores the position of the mouse on the screen
    string hitObject;                       // Used for debugging. Used to print name of object hit in linecast
    private Vector2 grapplePoint;           // The point that the tentacle hits when it grapples to something
    private SpringJoint2D joint;            // The spring joint applied between the player and the grapple point while they're grappling
    private Vector2 distanceVector;         // Used in the raycast to determine if an object is in range of being grappled
    private Vector2 enemyThrowForce;        // NOT CURRENTLY WORKING - The force enemies are thrown off the tentacle when it's recalled
    private Vector3 hookDistanceVector;     // Used to calculate where the hook should go when the player clicks
    public float shakeDuration = 0.15f;     // How long the screen should shake for when the player grapples
    public float shakeMagnitude = 0.4f;     // How much the scren should shake when the player grapples

    private void Awake()
    {
        enemyThrowForce.Set(10, 1);         // Sets the vector that controls how much the enemies are thrown by
    }

    private void Update()
    {
        hookDistance = Vector2.Distance(launchPointT.position, hook.position); // Used by RopeBridge script to calculate the length of individual rope segments

        if (Input.GetKeyDown(KeyCode.Escape))   // Reloads the scene when escape is pressed
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetMouseButton(0) && isGrappled == false)     // If the player holds the mouse button down and they're not grappled, call hook control. Allows the player to have the tentacle follow the mouse while its held down.
        {
            HookControl();
        }

        if (isGrappled == true)     // While the player is grappled, call stick hook. Sticks the player to the grapple point.
        {
            StickHook();
        }

        if (enemyStuck == true)     // While an enemy is stuck to the tentacle, call enemy stuck. Sticks the enemy to the end of the tentacle.
        {
            EnemyStuck();
        }

        if (Input.GetMouseButtonDown(0) && enemyStuck == false) // Below makes it so if you click behind a wall, it will automatically snap you to the wall between you and the cursor
        {
            RaycastHit2D linecastHit = Physics2D.Linecast(launchPointT.position, mousePosition, ignoreLinecast);

            if (linecastHit)
            {
                ShootGrapple();

                // Debugging tools below. Prints when funtion runs. Prints name of object hit and distance between it and the player
                    /*Debug.Log("In da way");
                    hitObject = linecastHit.collider.gameObject.name;
                    Debug.Log(hitObject);
                    Debug.Log(linecastHit.distance); */
            }

        }
        else if (Input.GetMouseButtonUp(0))     // If the enemy releases the mouse, calls stop grapple. Returns the hook to the player, resets relevant bools
        {
            StopGrapple();
        }
    }


    public void ShootGrapple()      // Shoots a raycast from the player towards the cursor. If the raycast hits, call grapple and shake the screen.
    {
        distanceVector = playerCamera.ScreenToWorldPoint(Input.mousePosition) - launchPointT.position;
        grappleHit = Physics2D.Raycast(launchPointT.position, distanceVector.normalized, maxDistance, whatIsGrappleable);
        if (grappleHit)
        {
            Grapple(grappleHit);

            // Shakes the screen on hit
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        }
    }

    void StopGrapple()              // Destroys all spring joints on the player, pulls the tentacle back to the player, resets bools and drops held enemies
    {
        
        // Gets an array of all spring joints on player and destroys them
        // This acts as a failsafe just incase multiple joints are created accidently
        // Still doesn't work if you click fast enough
        Joint2D[] springJoints = player.GetComponents<SpringJoint2D>();
        foreach (Joint2D joint in springJoints)
        {
            Destroy(joint);
        }

        //Makes hook return to player when mouse stops being held down
        hook.position = launchPointT.position;

        // Resets bools
        isGrappled = false;
        hookCollider.enabled = true;
        
        //Drops enemies held
        HookScript hookScript = GetComponentInChildren<HookScript>();
        enemyStuck = false;
        hookScript.enemy.GetComponent<Collider2D>().enabled = true;
        hookScript.enemy.GetComponent<Rigidbody2D>().AddRelativeForce(enemyThrowForce); //need to make it so when you let go of player it shoots them
    }

    void Grapple(RaycastHit2D hit)  // Creates a spring joint between the player and the point the grapple raycast hits. Contains setters that control properties of spring
    {
        
        // Creates a springjoint between the player and where they grappled
        grapplePoint = hit.point;
        joint = player.gameObject.AddComponent<SpringJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;
        joint.enableCollision = true;

        isGrappled = true;

        float distanceFromPoint = Vector2.Distance(launchPointT.position, grapplePoint);


        //distance to grapple point
        joint.distance = /*distanceFromPoint **/ jointDistance;

        //grapple variables
        joint.dampingRatio = jointDamping;
        joint.frequency = jointFrequency;
        
    }

    private void OnDrawGizmos()     // Draws a circle to show the radius the player can grapple in, and a line that shows where the player is aiming
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(launchPointT.position, maxDistance);

        // Draws a line to show where the player will grapple
        Vector2 distanceVector = playerCamera.ScreenToWorldPoint(Input.mousePosition) - launchPointT.position;
        Debug.DrawLine(launchPointT.position, distanceVector, Color.red);
    }

    void EnemyStuck()               // Sticks the enemy hit to the end of the tentacle while being called
    {
        HookScript hookScript = GetComponentInChildren<HookScript>();
        hookScript.enemy.transform.position = hook.transform.position;
        hookScript.enemy.GetComponent<Collider2D>().enabled = false;
    }

    void HookControl()              // Makes the end of the tentacle (the hook) stay on the mouse while the function is being called
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = launchPointT.position.z - Camera.main.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        hookDistanceVector = mousePosition - launchPointT.position;

        hookDistanceVector = Vector3.ClampMagnitude(hookDistanceVector, maxDistance - rangeReduction);

        hook.position = launchPointT.position + hookDistanceVector;

        //This is me trying and failing to make the hook lerp from the player to the mouse instead of teleporting
             //hook.position = Vector3.Lerp(launchPointT.position, hook.position, hookSpeed);


    }

    void StickHook()         //Makes hook stick to the grapple point when called
    {
        hook.position = grapplePoint;
        hookCollider.enabled = false;
    }
}
