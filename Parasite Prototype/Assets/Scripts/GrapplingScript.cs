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
    public LayerMask whatIsGrappleable;
    public LayerMask ignoreLinecast;

    [Header("Assign Objects")]
    public Transform launchPointT;
    public Transform hook;
    public Collider2D hookCollider;
    public GameObject player;
    public Camera playerCamera;

    [Header("Spring Settings")]
    public float jointDistance = 0.8f;
    public float jointDamping = 7f;
    public float jointFrequency = 5f;

    [Header("Hook Settings")]
    public float hookDistance = 1f; // Used by RopeBridge script to calculate the length of individual rope segments
    public float maxDistance = 20f;
    public float rangeReduction = 1f; // Since the hook is a physical object, the edges of the hook sometimes extend further than the raycast. This float pulls it in a little bit so the furthest extent of the hook object will be reached by the raycast.
    public float hookSpeed = 0.1f;

    [Header("Bools")]
    public bool isGrappled = false;
    public bool enemyStuck = false;

    public CameraShake cameraShake;

    [Header("Private")]
    public RaycastHit2D grappleHit;
    private Vector3 mousePosition;
    string hitObject;
    private LineRenderer lr;
    private Vector2 grapplePoint;
    private SpringJoint2D joint;
    private Vector2 distanceVector;
    private Vector2 yeehaw;
    private Vector3 distanceV3;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.4f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        yeehaw.Set(10, 1);
    }

    private void Update()
    {
        hookDistance = Vector2.Distance(launchPointT.position, hook.position); // Used by RopeBridge script to calculate the length of individual rope segments

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetMouseButton(0) && isGrappled == false)
        {
            HookControl();
        }

        if (isGrappled == true)
        {
            StickHook();
        }

        if (enemyStuck == true)
        {
            EnemyStuck();
        }

        if (Input.GetMouseButtonDown(0) && enemyStuck == false) // Below makes it so if you click behind a wall, it will automatically snap you to the thing between you and the cursor
        {
            RaycastHit2D linecastHit = Physics2D.Linecast(launchPointT.position, mousePosition, ignoreLinecast);

            if (linecastHit)
            {
                ShootGrapple();

                // Debugging tools below. Prints when funtion runs. Prints name of object hit and distance between it and the player
                    //Debug.Log("In da way");
                    //hitObject = linecastHit.collider.gameObject.name;
                    //Debug.Log(hitObject);
                    //Debug.Log(linecastHit.distance); 
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }


    public void ShootGrapple()
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

    void StopGrapple()
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
        hookScript.enemy.GetComponent<Rigidbody2D>().AddRelativeForce(yeehaw); //need to make it so when you let go of player it shoots them
    }

    void Grapple(RaycastHit2D hit)
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

        //lr.positionCount = 2;
        
    }

    private void OnDrawGizmos() // Draws a circle to show the radius the player can grapple in
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(launchPointT.position, maxDistance);

        // Draws a line to show where the player will grapple
        Vector2 distanceVector = playerCamera.ScreenToWorldPoint(Input.mousePosition) - launchPointT.position;
        Debug.DrawLine(launchPointT.position, distanceVector, Color.red);
    }

    void EnemyStuck()
    {
        HookScript hookScript = GetComponentInChildren<HookScript>();
        hookScript.enemy.transform.position = hook.transform.position;
        hookScript.enemy.GetComponent<Collider2D>().enabled = false;
    }

    void HookControl()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = launchPointT.position.z - Camera.main.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        distanceV3 = mousePosition - launchPointT.position;

        distanceV3 = Vector3.ClampMagnitude(distanceV3, maxDistance - rangeReduction);

        hook.position = launchPointT.position + distanceV3;
        
        //hook.position = Vector3.Lerp(launchPointT.position, hook.position, hookSpeed);

    }         //Makes the hook stay on the mouse while button is held

    void StickHook()         //Makes hook stick to the grapple point
    {
        hook.position = grapplePoint;
        hookCollider.enabled = false;
    }
}
