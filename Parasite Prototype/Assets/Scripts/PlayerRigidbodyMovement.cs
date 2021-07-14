using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerRigidbodyMovement : MonoBehaviour
{

    public Rigidbody2D playerRigidbody;
    public float moveSpeed = 10f;
    public float maxSpeed = 2f;

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyInput();
    }

    void ApplyInput()
    {
        float xInput = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        //float yInput = Input.GetAxis("Vertical");

        Vector2 newForce = new Vector2(xInput, 0);


        playerRigidbody.AddRelativeForce(newForce, ForceMode2D.Impulse);

    }

}
