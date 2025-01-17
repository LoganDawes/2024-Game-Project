using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    private Collider2D triggerCollider;     // The trigger collider to detect player input
    private bool isNearSign;               // Tracks if the player is near the sign
    private bool isSignRead = false;       // Tracks if the sign has been read

    public string signMessage = "Placeholder message"; // Message displayed when reading the sign
    private AudioSource audioSource;        // AudioSource component for playing sound effects
    public AudioClip readSound;            // Sound played when the sign is read

    public GameObject glowObject;          // Reference to the Glow object inside the Sign prefab
    private SpriteRenderer glowSpriteRenderer; // The SpriteRenderer for the Glow sprite

    public float flickerInterval = 0.5f;   // Time between flickering on/off

    private Coroutine flickerCoroutine;     // Coroutine reference for flickering

    public int signID;

    void Awake()
    {
        // Get the trigger collider
        triggerCollider = GetComponent<Collider2D>();

        // Get the audio source component
        audioSource = GetComponent<AudioSource>();

        // Get the SpriteRenderer component of the Glow object
        glowSpriteRenderer = glowObject.GetComponent<SpriteRenderer>();

        // Start flickering the sign sprite immediately on awake
        if (!isSignRead)
        {
            StartFlickering();
        }
    }

    private void Update()
    {
        // Check if player is near the sign and presses 'E' to interact
        if (isNearSign && Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance.isSignOpen)
            {
                if (!GameManager.Instance.pauseMenuUI.activeSelf)
                {
                    // Close the sign menu when the player presses 'E' again
                    GameManager.Instance.ResumeGame();
                }
            }
            else
            {
                // Read the sign and open the sign menu
                ReadSign();
            }
        }
    }

    private void ReadSign()
    {
        if (readSound != null)
        {
            audioSource.PlayOneShot(readSound);
        }

        // Display the message
        GameManager.Instance.OpenSignMenu(signMessage);

        // Register the sign as read with GameManager
        GameManager.Instance.RegisterReadSign(this);
    }

    private void StartFlickering()
    {
        // Start flickering the sign sprite until the sign is read
        flickerCoroutine = StartCoroutine(FlickerSprite());
    }

    public void StopFlickering()
    {
        isSignRead = true;
        glowSpriteRenderer.enabled = false;
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
    }

    private IEnumerator FlickerSprite()
    {
        while (!isSignRead)
        {
            // Toggle the sprite visibility on and off
            glowSpriteRenderer.enabled = !glowSpriteRenderer.enabled;

            // Wait for the next interval
            yield return new WaitForSeconds(flickerInterval);
        }

        // Make sure the sprite stays off after being read
        glowSpriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detects if player is near the sign for interaction
        if (collision.CompareTag("Player"))
        {
            isNearSign = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Detects if player exits the range of the sign
        if (collision.CompareTag("Player"))
        {
            isNearSign = false;
        }
    }
}
