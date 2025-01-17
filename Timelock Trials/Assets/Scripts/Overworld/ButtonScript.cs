using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    private ButtonPuzzle puzzleController;
    private int buttonIndex;
    public AudioClip buttonSound; // Unique sound for this button
    private AudioSource audioSource;

    public void Initialize(ButtonPuzzle puzzleController, int index)
    {
        this.puzzleController = puzzleController;
        this.buttonIndex = index;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayButtonSound()
    {
        if (buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayButtonSound();
            puzzleController.ButtonPressed(buttonIndex);
        }
    }
}
