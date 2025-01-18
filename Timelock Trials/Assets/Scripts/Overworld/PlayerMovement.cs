using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;    // Player speed
    private Vector2 moveDirection;  // Direction vector
    private Rigidbody2D rb;         // Rigidbody component

    private void Awake()
    {
        // Register this player instance with the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayer(gameObject);
        }
        else
        {
            Debug.LogWarning("GameManager instance is null.");
        }
    }

    void Start()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody2D>();

        // Ensure freeze rotation
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Movement Direction Calculations
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.y = Input.GetAxisRaw("Vertical");
        moveDirection.Normalize();
    }

    private void FixedUpdate()
    {
        // Movement
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Collisison with Wall
        if (collision.collider.CompareTag("Wall"))
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Collisison with Wall
        if (collision.collider.CompareTag("Wall"))
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

