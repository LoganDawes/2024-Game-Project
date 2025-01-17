using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Sprite closedDoorSprite;         // Sprite for the closed door
    public Sprite openDoorSprite;           // Sprite for the open door

    private SpriteRenderer spriteRenderer;  // Sprite Renderer Component

    private Collider2D triggerCollider;     // The trigger collider to detect player input
    private Collider2D physicalCollider;    // The physical collider to block movement when door is closed

    public string doorID;                   // ID used by GameManager

    private bool isNearDoor = false;        // track if the player is near the door
    public bool isLocked = false;           // track if door is locked
    public bool eventDoor = false;          // track if door is opened by event

    public GameObject lockObject;           // lock sprite

    public AudioClip doorSound;             // Sound the door makes when opened
    public AudioClip lockedSound;           // Sound the door makes when locked
    private AudioSource audioSource;        // AudioSource component

    private void Awake()
    {
        // Get sprite renderer compoennt
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get collision components
        Collider2D[] colliders = GetComponents<Collider2D>();

        // Get audio source component
        audioSource = GetComponent<AudioSource>();

        // Set colliders appropriately
        triggerCollider = colliders[0];
        physicalCollider = colliders[1];
    }

    private void Start()
    {
        // Check if the door should be open on start
        if (GameManager.Instance.IsDoorOpen(doorID))
        {
            lockObject.SetActive(false);
            OpenDoor();
        }
        else if (isLocked)
        {
            lockObject.SetActive(true); // Display lock if the door is locked
        }
        else
        {
            lockObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if player is near the door and presses 'E' to interact
        if (isNearDoor && Input.GetKeyDown(KeyCode.E))
        {
            if (isLocked)
            {
                TryUnlockDoor();
            }
            else if (!GameManager.Instance.IsDoorOpen(doorID))
            {
                audioSource.PlayOneShot(doorSound);
                OpenDoor();
            }
        }
    }

    public void TryUnlockDoor()
    {
        // Check if the player has a key in the inventory
        if (GameManager.Instance.InventoryContains("Key") && !eventDoor)
        {
            // Remove one key from the inventory
            GameManager.Instance.RemoveItemFromInventory("Key");
            isLocked = false;
            StartCoroutine(UnlockAndOpenDoor());
        }
        else
        {
            // Play locked sound and jiggle lock if no key, or event door
            audioSource.PlayOneShot(lockedSound);
            StartCoroutine(JiggleLock());
        }
    }

    public void OpenEventDoor()
    {
        StartCoroutine(UnlockAndOpenDoor());
    }

    private IEnumerator UnlockAndOpenDoor()
    {
        // Animate the lock falling off
        lockObject.transform.Translate(0, -0.5f, 0); // Move the lock downwards
        yield return new WaitForSeconds(0.3f);       // Wait for the animation
        lockObject.SetActive(false);                 // Hide lock object

        audioSource.PlayOneShot(doorSound);
        OpenDoor(); // Open the door after unlocking
    }

    private IEnumerator JiggleLock()
    {
        Vector3 originalPosition = lockObject.transform.localPosition;

        for (int i = 0; i < 5; i++)
        {
            lockObject.transform.localPosition = originalPosition + new Vector3(0.05f, 0, 0);
            yield return new WaitForSeconds(0.01f);
            lockObject.transform.localPosition = originalPosition - new Vector3(0.05f, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }

        lockObject.transform.localPosition = originalPosition;
    }

    public void OpenDoor()
    {
        // Open the door: Disable physical collider to allow passage
        spriteRenderer.sprite = openDoorSprite;
        physicalCollider.enabled = false;  // Disable the collider to allow passage

        // Register door is open with GameManager
        GameManager.Instance.RegisterOpenDoor(doorID);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detects if player is close to door for interaction
        if (collision.CompareTag("Player"))
        {
            isNearDoor = true;
            Debug.Log("Player near door.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Detects if player is close to door for interaction
        if (collision.CompareTag("Player"))
        {
            isNearDoor = false;
        }
    }
}



