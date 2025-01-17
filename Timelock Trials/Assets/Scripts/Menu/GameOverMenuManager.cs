using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenuManager : MonoBehaviour
{
    public void QuitButton()
    {
        // Quit the application
        Application.Quit();

        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartButton()
    {
        Debug.Log("Restart Button pushed");
        GameManager.Instance.ResetGame();
    }
}
