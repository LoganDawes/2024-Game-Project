using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockPuzzle : MonoBehaviour
{
    public Clock clock1;  // Reference to the first clock
    public Clock clock2;  // Reference to the second clock
    public Clock clock3;  // Reference to the third clock
    public Clock clock4;  // Reference to the fourth clock
    public Door door;     // Reference to the door that will be unlocked

    private AudioSource audioSource;        // AudioSource component for playing sound effects
    public AudioClip successSound;            // Sound played when the puzzle is completed

    public bool puzzleCompleted = false; // Prevents the puzzle from being retriggered

    // Predefined solution (time on each clock)
    public int solutionHour1 = 2, solutionMinute1 = 15;
    public int solutionHour2 = 12, solutionMinute2 = 30;
    public int solutionHour3 = 11, solutionMinute3 = 55;
    public int solutionHour4 = 8, solutionMinute4 = 10;

    public int puzzleID = 1;

    void Awake()
    {
        // Get the audio source component
        audioSource = GetComponent<AudioSource>();
    }

    // Check if the times on all clocks are correct
    public void CheckPuzzleSolved()
    {
        bool clock1Solved = clock1.CheckTime(solutionHour1, solutionMinute1);
        bool clock2Solved = clock2.CheckTime(solutionHour2, solutionMinute2);
        bool clock3Solved = clock3.CheckTime(solutionHour3, solutionMinute3);
        bool clock4Solved = clock4.CheckTime(solutionHour4, solutionMinute4);

        if (clock1Solved && clock2Solved && clock3Solved && clock4Solved && !puzzleCompleted)
        {
            audioSource.PlayOneShot(successSound);
            GameManager.Instance.RegisterCompletedPuzzle(this);
            Debug.Log("Puzzle Complete!");
            door.OpenEventDoor();
        }
        StoreTime();
    }

    public void StoreTime()
    {
        GameManager.Instance.storedHour1 = clock1.GetHour();
        GameManager.Instance.storedMinute1 = clock1.GetMinute();
        GameManager.Instance.storedHour2 = clock2.GetHour();
        GameManager.Instance.storedMinute2 = clock2.GetMinute();
        GameManager.Instance.storedHour3 = clock3.GetHour();
        GameManager.Instance.storedMinute3 = clock3.GetMinute();
        GameManager.Instance.storedHour4 = clock4.GetHour();
        GameManager.Instance.storedMinute4 = clock4.GetMinute();
    }
}
