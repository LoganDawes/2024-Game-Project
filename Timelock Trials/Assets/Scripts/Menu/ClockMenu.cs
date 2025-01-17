using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockMenu : MonoBehaviour
{
    public void IncrementHourButton()
    {
        if (GameManager.Instance.activeClock != null)
        {
            GameManager.Instance.activeClock.IncrementHourHand();
        }
    }

    public void DecrementHourButton()
    {
        if (GameManager.Instance.activeClock != null)
        {
            GameManager.Instance.activeClock.DecrementHourHand();
        }
    }

    public void IncrementMinuteButton()
    {
        if (GameManager.Instance.activeClock != null)
        {
            GameManager.Instance.activeClock.IncrementMinuteHand();
        }
    }

    public void DecrementMinuteButton()
    {
        if (GameManager.Instance.activeClock != null)
        {
            GameManager.Instance.activeClock.DecrementMinuteHand();
        }
    }
}
