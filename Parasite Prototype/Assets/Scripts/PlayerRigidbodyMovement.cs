using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerRigidbodyMovement : MonoBehaviour
{
    public Rigidbody2D playerRigidbody;     // The player's rigidbody
    public float moveSpeed = 10f;           // The player's move speed
    public float maxSpeed = 2f;             // The player's max speed

    void FixedUpdate()
    {
        ApplyInput();
    }

    void ApplyInput()
    {
        float xInput = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        Vector2 newForce = new Vector2(xInput, 0);

        playerRigidbody.AddRelativeForce(newForce, ForceMode2D.Impulse);

    }

}
