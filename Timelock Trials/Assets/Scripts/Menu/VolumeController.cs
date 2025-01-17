using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // Include this for Slider component

public class VolumeController : MonoBehaviour
{
    public AudioClip testNoise;
    public AudioMixer audioMixer;
    private AudioSource audioSource;
    public Slider volumeSlider; // Reference to the Slider component

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        volumeSlider.value = GameManager.Instance.volumeLevel;
    }

    public void SetVolume()
    {
        // Get the slider value (ranging from 0 to 1)
        float volume = volumeSlider.value;

        // Debugging: check the slider value
        Debug.Log("Slider Volume: " + volume);

        // Ensure the value is not zero to avoid log10(0) error
        volume = Mathf.Clamp(volume, 0.0001f, 1f);

        // Convert the volume to decibels and set the AudioMixer
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);

        audioSource.PlayOneShot(testNoise);
        GameManager.Instance.volumeLevel = volume;
    }
}
