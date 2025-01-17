using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public GameObject optionsMenuUI;    // Options Menu object

    public void OnCloseOptionsMenu()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string mainMenuSceneName = "MainMenu";

        if (currentSceneName != mainMenuSceneName)
        {
            // Load the main menu scene
            GameManager.Instance.OpenMainMenu(mainMenuSceneName);
        }
        else
        {
            Debug.Log("Already in the main menu.");
        }
        GameManager.Instance.ResumeGame();
    }
}
