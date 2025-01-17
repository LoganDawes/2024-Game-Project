using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzle : MonoBehaviour
{
    [Header("References")]
    public GameObject[] buttons; // Array to store button objects
    public Collider2D activationZone; // Invisible trigger area
    public Door door;               // Door opened by puzzle
    public AudioClip successSound; // Sound for success
    public AudioClip errorSound;   // Sound for failure
    private AudioSource audioSource;

    [Header("Puzzle Settings")]
    private int currentPhase = 1; // Tracks the current phase (1-3)
    private List<int> playerSequence = new List<int>(); // Player input sequence
    private List<int> targetSequence = new List<int>(); // Correct sequence

    private bool puzzleActive = false; // Indicates if the puzzle is active
    private bool acceptingInput = false; // Indicates if player input is allowed
    public bool puzzleCompleted = false; // Prevents the puzzle from being retriggered

    public int puzzleID = 0;

    private void Start()
    {
        // Assign the AudioSource component
        audioSource = GetComponent<AudioSource>();
        AssignButtonIndices();
    }

    private void AssignButtonIndices()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            // Check if the button already has a ButtonScript component
            ButtonScript buttonScript = buttons[i].GetComponent<ButtonScript>();

            if (buttonScript == null)
            {
                // If not, log a warning and skip this button
                Debug.LogWarning($"Button {buttons[i].name} does not have a ButtonScript attached!");
                continue;
            }

            // Initialize the existing ButtonScript component
            buttonScript.Initialize(this, i);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puzzleCompleted && !puzzleActive && collision.CompareTag("Player"))
        {
            puzzleActive = true;
            StartCoroutine(StartPuzzle());
        }
    }

    private IEnumerator StartPuzzle()
    {
        for (int phase = 1; phase <= 3; phase++)
        {
            currentPhase = phase;
            GenerateSequence();
            yield return ShowSequence();
            acceptingInput = true;

            while (!CheckPlayerInput())
            {
                yield return null; // Wait until the player succeeds or fails
            }
        }

        CompletePuzzle();
    }

    private void GenerateSequence()
    {
        targetSequence.Clear();
        int buttonCount = buttons.Length;

        for (int i = 0; i < GetPhaseLength(); i++)
        {
            int nextButton = (currentPhase == 3) ? Random.Range(0, buttonCount) : i;
            targetSequence.Add(nextButton);
        }
    }

    private int GetPhaseLength()
    {
        switch (currentPhase)
        {
            case 1: return 3;
            case 2: return 4;
            case 3: return 6;
            default: return 0;
        }
    }

    private IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(1f);
        acceptingInput = false;

        foreach (int buttonIndex in targetSequence)
        {
            // Change the button sprite color to yellow
            SpriteRenderer renderer = buttons[buttonIndex].GetComponent<SpriteRenderer>();
            renderer.color = Color.yellow;
            buttons[buttonIndex].GetComponent<ButtonScript>().PlayButtonSound();

            // Wait for 0.5 seconds before turning it off
            yield return new WaitForSeconds(0.5f);
            renderer.color = Color.white;

            yield return new WaitForSeconds(0.2f); // Short delay between button highlights
        }

        acceptingInput = true;
        playerSequence.Clear();
    }

    public void ButtonPressed(int buttonIndex)
    {
        if (!acceptingInput) return;

        playerSequence.Add(buttonIndex);

        // Feedback for button press
        SpriteRenderer renderer = buttons[buttonIndex].GetComponent<SpriteRenderer>();
        renderer.color = Color.green;

        StartCoroutine(ResetButtonColor(buttonIndex));

        // Check player input if the sequence is complete
        if (playerSequence.Count == targetSequence.Count)
        {
            CheckPlayerInput();
        }
    }

    private IEnumerator ResetButtonColor(int buttonIndex)
    {
        yield return new WaitForSeconds(0.2f);
        buttons[buttonIndex].GetComponent<SpriteRenderer>().color = Color.white;
    }

    private bool CheckPlayerInput()
    {
        if (playerSequence.Count < targetSequence.Count) return false;

        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != targetSequence[i])
            {
                // Play error sound and restart the phase
                audioSource.PlayOneShot(errorSound);
                playerSequence.Clear();
                StartCoroutine(ShowSequence());
                return false;
            }
        }

        // If the sequence is correct, move to the next phase or complete the puzzle
        audioSource.PlayOneShot(successSound);
        return true;
    }

    private void CompletePuzzle()
    {
        Debug.Log("Puzzle Complete!");
        door.OpenEventDoor();
        puzzleActive = false;
        GameManager.Instance.RegisterCompletedPuzzle(this);
    }
}
