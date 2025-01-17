using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;            // Player's transform
    public float smoothSpeed = 2f;      // Smooth follow speed
    public Vector3 offset;              // Camera offset
    public static CameraController Instance { get; private set; }

    // LateUpdate for smooth movement
    private void LateUpdate()
    {
        if (player == null)
        {
            return; // Exit the method if no player is assigned
        }

        Vector3 desiredPosition = player.position + offset;

        float adjustedSpeed = smoothSpeed * Time.deltaTime;

        // Linear interpolation for smooth transition
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, adjustedSpeed);

        transform.position = smoothedPosition;
    }

    private void Awake()
    {
        // Ensure that only one instance of the camera manager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the camera manager across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }

    // Set target for switching between scenes
    public void SetTarget(Transform newPlayer)
    {
        player = newPlayer;
        transform.position = player.position + offset;
    }
}

