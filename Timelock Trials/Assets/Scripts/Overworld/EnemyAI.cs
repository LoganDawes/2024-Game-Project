using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Transform player;           // Player's Transform
    public float roamSpeed = 200f;       // Roaming speed
    public float chaseSpeed = 400f;      // Speed when chasing the player
    public float detectionRange = 5f;  // Range within which the enemy will start chasing the player

    public Vector2 roamArea;           // Area within which the enemy will roam
    private Vector2 roamTarget;        // Target point within roam area

    private Rigidbody2D rb;            // Rigidbody component

    private bool isChasing = false;    // To track if the enemy is currently chasing the player
    private bool disabledTemporarily = false;   // tracks if enemy is disabled temporarily

    public float minRoamDelay = 1f;    // Minimum delay between roaming
    public float maxRoamDelay = 3f;    // Maximum delay between roaming

    public int enemyID;                 // ID used by GameManager
    public string enemyType;            // Used to define the initiated battle

    private Coroutine roamCoroutine;

    private void Start()
    {
        // Get Rigidbody Component
        rb = GetComponent<Rigidbody2D>();
        SetNewRoamTarget();                 // Set initial roam target
        roamCoroutine = StartCoroutine(RoamWithDelay());    // Start roaming behavior
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If player is within detection range, chase the player
        if (distanceToPlayer < detectionRange)
        {
            if (!isChasing)
            {
                isChasing = true;
                StopCoroutine(roamCoroutine); // Stop any roaming coroutines if chasing player
            }
            if (!disabledTemporarily)
            {
                ChasePlayer();
            }
        }

        // If the player leaves the detection range, resume roaming
        else if (isChasing)
        {
            isChasing = false;
            rb.velocity = Vector2.zero;         // Stop current velocity
            roamCoroutine = StartCoroutine(RoamWithDelay());    // Resume roaming behavior
        }
    }

    public void SetPlayerReference(GameObject playerObject)
    {
        this.player = playerObject.transform;
    }


    private IEnumerator RoamWithDelay()
    {
        // Wait for a random amount of time before moving
        float roamDelay = Random.Range(minRoamDelay, maxRoamDelay);
        yield return new WaitForSeconds(roamDelay);

        // Move towards the roam target
        while (Vector2.Distance(transform.position, roamTarget) > 0.2f)
        {
            Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
            rb.velocity = direction * roamSpeed * Time.fixedDeltaTime;
            yield return null; // Wait for the next frame
        }

        // After reaching the roam target, pick a new one
        SetNewRoamTarget();
        roamCoroutine = StartCoroutine(RoamWithDelay()); // Continue roaming
    }

    private void SetNewRoamTarget()
    {
        // Pick a random point within the roam area
        roamTarget = new Vector2(
            Random.Range(transform.position.x - roamArea.x, transform.position.x + roamArea.x),
            Random.Range(transform.position.y - roamArea.y, transform.position.y + roamArea.y)
        );
    }

    private void ChasePlayer()
    {
        StopCoroutine(roamCoroutine);
        // Move towards the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the enemy collides with the wall, pick a new roam target
        if (collision.gameObject.CompareTag("Wall"))
        {
            StopCoroutine(roamCoroutine); // Stop current roam and pick a new target
            SetNewRoamTarget();
            roamCoroutine = StartCoroutine(RoamWithDelay()); // Start roaming again
        }

        // If the enemy collides with the player, trigger combat
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance.isClockOpen)
            {
                GameManager.Instance.CloseClockMenu();
            }

            GameManager.Instance.CurrentEnemyID = this.enemyID;
            Debug.Log("Current enemy: "+GameManager.Instance.CurrentEnemyID);
            GameManager.Instance.LoadBattleScene("BattleScene", enemyType);
            Debug.Log("Combat started!");
        }
    }

    public void DisableForDuration(float duration)
    {
        StartCoroutine(DisableTemporarily(duration));
    }

    private IEnumerator DisableTemporarily(float duration)
    {
        if (disabledTemporarily) yield break; // Prevent multiple disable calls
        disabledTemporarily = true;

        // Disable chase behavior
        isChasing = false;
        rb.velocity = Vector2.zero; // Stop movement
        StopCoroutine(roamCoroutine); // Stop any ongoing coroutines

        // Temporarily disable collision by turning off the Collider2D
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Get the SpriteRenderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on enemy!");
            yield break;
        }

        // Start flickering effect
        float flickerInterval = 0.1f; // Time between flickers
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle sprite visibility
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += flickerInterval;
        }

        // Ensure the sprite is visible at the end of the effect
        spriteRenderer.enabled = true;

        // Re-enable collision
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Deregister as disabled and Resume roaming
        disabledTemporarily = false;
        GameManager.Instance.DeregisterEnemyDisabled(this.enemyID);
        roamCoroutine = StartCoroutine(RoamWithDelay());
    }


}

