using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLocalization : MonoBehaviour
{
    public GameObject Title;          // Reference to the English title GameObject
    public GameObject JapaneseTitle; // Reference to the Japanese title GameObject

    public void OnButtonClicked()
    {
        // Check the active state of the English title and toggle accordingly
        if (Title.activeSelf)
        {
            Title.SetActive(false);           // Deactivate the English title
            JapaneseTitle.SetActive(true);    // Activate the Japanese title
        }
        else
        {
            Title.SetActive(true);            // Activate the English title
            JapaneseTitle.SetActive(false);   // Deactivate the Japanese title
        }
    }
}
