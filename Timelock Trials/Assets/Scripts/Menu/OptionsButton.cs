using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public void OnOpenOptionsMenu()
    {
        GameManager.Instance.PauseGame();
    }
}


