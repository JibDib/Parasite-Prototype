using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookScript : MonoBehaviour
{
    //public GameObject player;     // Could be used to make a system that lets the player swap hosts
    public GameObject enemy;        // A variable to store the enemy hit





    /*private void Start()
    {
        player = GetComponentInParent<GameObject>();
    }*/

    private void OnCollisionEnter2D(Collision2D collision)      // Detects when the hook object collides with something
    {
        GrapplingScript grappleScript = GetComponentInParent<GrapplingScript>();


        // When the hook collides with a grappleable surface, start the grapple function
        if (collision.gameObject.layer == 9 && grappleScript.isGrappled == false && grappleScript.enemyStuck == false)      // If it collides with something on the 'Grapple' layer and you're not already grappled
        {
            grappleScript.ShootGrapple();                       // Start the shoot grapple command;
        }


        // When the hook collides with an enemy, set the enemy stuck bool to true. This calls a function in the Grappling Script
        if (collision.gameObject.layer == 12 && grappleScript.isGrappled == false && grappleScript.enemyStuck == false)
        {
            enemy = collision.gameObject;
            grappleScript.enemyStuck = true;
        }


        //Potentially add something that lets you swap hosts on contact

            /* if (collision.gameObject.layer == 12 && grappleScript.isGrappled == false)
             {
                 //enemy = collision.gameObject;
                 player.transform.position = enemy.transform.position;
                 //enemy.transform.GetChild(0).gameObject.SetActive(true);
                 enemy.SetActive(true);
                 player.SetActive(false);
                 Debug.Log("Woop");
             }*/

    }
}
