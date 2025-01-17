using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    private Collider2D triggerCollider;     // The trigger collider to detect player input
    private Collider2D physicalCollider;    // The physical collider to block movement

    private bool isNearBarrel;

    public AudioClip searchSound;           // Sound the barrel makes when searched
    public AudioClip collectSound;           // Sound the barrel makes when searched
    private AudioSource audioSource;        // AudioSource component

    public int barrelID;    // ID used by GameManager

    public string contents;

    void Awake()
    {
        // Get collision components
        Collider2D[] colliders = GetComponents<Collider2D>();

        // Get audio source component
        audioSource = GetComponent<AudioSource>();

        // Set colliders appropriately
        triggerCollider = colliders[0];
        physicalCollider = colliders[1];
    }

    void Start()
    {
        // Check if the barrel is empty
        if (GameManager.Instance.IsBarrelEmpty(barrelID))
        {
            EmptyBarrel();
        }
    }

    public void EmptyBarrel()
    {
        this.contents = string.Empty;
    }

    private void Update()
    {
        // Check if player is near the door and presses 'E' to interact
        if (isNearBarrel && Input.GetKeyDown(KeyCode.E))
        {
            audioSource.PlayOneShot(searchSound);
            OpenBarrel();
        }
    }

    private void OpenBarrel()
    {
        StartCoroutine(Jiggle());

        if (contents != string.Empty)
        {
            audioSource.PlayOneShot(collectSound);
            GameManager.Instance.AddItemToInventory(contents);
            GameManager.Instance.RegisterEmptyBarrel(this);
        }
    }

    private IEnumerator Jiggle()
    {
        Vector3 originalPosition = transform.localPosition;

        for (int i = 0; i < 5; i++)
        {
            transform.localPosition = originalPosition + new Vector3(0.05f, 0, 0);
            yield return new WaitForSeconds(0.01f);
            transform.localPosition = originalPosition - new Vector3(0.05f, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }

        transform.localPosition = originalPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detects if player is close to door for interaction
        if (collision.CompareTag("Player"))
        {
            isNearBarrel = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Detects if player is close to door for interaction
        if (collision.CompareTag("Player"))
        {
            isNearBarrel = false;
        }
    }
}
