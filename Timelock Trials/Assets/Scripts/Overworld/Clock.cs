using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    private Collider2D triggerCollider;     // The trigger collider to detect player input
    private bool isNearClock;               // Tracks if the player is near the clock

    private AudioSource audioSource;        // AudioSource component for playing sound effects
    public AudioClip readSound;            // Sound played when the clock is read
    public AudioClip tickSound;            // Sound played when clock hand is moved

    public ClockPuzzle clockPuzzle;

    [Header("Clock Hands")]
    private int hour = 12;
    private int minute = 0;
    public Transform hourHand;              // Reference to the hour hand
    public Transform minuteHand;            // Reference to the minute hand

    private const float HourStep = -30f;  // Rotation step for hour hand
    private const float MinuteStep = -30f; // Rotation step for minute hand

    void Awake()
    {
        // Get the trigger collider
        triggerCollider = GetComponent<Collider2D>();

        // Get the audio source component
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Check if player is near the clock and presses 'E' to interact
        if (isNearClock && Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager.Instance.isSignOpen)
            {
                if (!GameManager.Instance.pauseMenuUI.activeSelf)
                {
                    // Close the clock menu when the player presses 'E' again
                    GameManager.Instance.CloseClockMenu();
                }
            }
            else
            {
                // Read the clock and open the clock menu
                ReadClock();
            }
        }
    }

    private void ReadClock()
    {
        if (readSound != null)
        {
            audioSource.PlayOneShot(readSound);
        }

        // Display the message
        GameManager.Instance.OpenClockMenu(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detects if player is near the clock for interaction
        if (collision.CompareTag("Player"))
        {
            isNearClock = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Detects if player exits the range of the clock
        if (collision.CompareTag("Player"))
        {
            isNearClock = false;
        }
    }

    // Increment the hour hand
    public void IncrementHourHand()
    {
        audioSource.PlayOneShot(tickSound);
        hour++;
        if (hour > 12)
        {
            hour = 1; // Loop back to 1
        }

        if (hourHand != null)
        {
            hourHand.Rotate(0, 0, HourStep);
        }
        Debug.Log(hour + ":" + minute);
        clockPuzzle.CheckPuzzleSolved();
    }

    // Decrement the hour hand
    public void DecrementHourHand()
    {
        audioSource.PlayOneShot(tickSound);
        hour--;
        if (hour < 1)
        {
            hour = 12; // Loop back to 12
        }

        if (hourHand != null)
        {
            hourHand.Rotate(0, 0, -HourStep);
        }
        Debug.Log(hour + ":" + minute);
        clockPuzzle.CheckPuzzleSolved();
    }

    // Increment the minute hand
    public void IncrementMinuteHand()
    {
        audioSource.PlayOneShot(tickSound);
        minute += 5;
        if (minute >= 60)
        {
            minute = 0; // Loop back to 0
        }

        if (minuteHand != null)
        {
            minuteHand.Rotate(0, 0, MinuteStep);
        }
        Debug.Log(hour + ":" + minute);
        clockPuzzle.CheckPuzzleSolved();
    }

    // Decrement the minute hand
    public void DecrementMinuteHand()
    {
        audioSource.PlayOneShot(tickSound);
        minute -= 5;
        if (minute < 0)
        {
            minute = 55; // Loop back to 55
        }

        if (minuteHand != null)
        {
            minuteHand.Rotate(0, 0, -MinuteStep);
        }
        Debug.Log(hour + ":" + minute);
        clockPuzzle.CheckPuzzleSolved();
    }

    public int GetHour()
    {
        return hour;
    }

    public int GetMinute()
    { 
        return minute;
    }

    public bool CheckTime(int hour, int minute)
    {
        return hour == this.hour && minute == this.minute;
    }

    public void SetTime(int hour, int minute)
    {
        // Ensure the hour is between 1 and 12
        if (hour < 1)
        {
            hour = 12; // Default to 12 if the hour is less than 1
        }
        else if (hour > 12)
        {
            hour = hour % 12 == 0 ? 12 : hour % 12; // Wrap around if greater than 12
        }

        // Ensure the minute is between 0 and 59
        if (minute < 0 || minute >= 60)
        {
            minute = 0; // Default to 0 if invalid
        }

        this.hour = hour;
        this.minute = minute;

        // Calculate the rotation for the hour hand
        if (hourHand != null)
        {
            float hourRotation = (hour % 12) * HourStep; // Snap the hour hand in 30-degree increments
            hourHand.localRotation = Quaternion.Euler(0, 0, hourRotation);
        }

        // Calculate the rotation for the minute hand
        if (minuteHand != null)
        {
            float minuteRotation = (minute / 5) * MinuteStep; // Snap the minute hand in 30-degree increments
            minuteHand.localRotation = Quaternion.Euler(0, 0, minuteRotation);
        }
    }
}
