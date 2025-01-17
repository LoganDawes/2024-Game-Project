using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaircaseTeleport : MonoBehaviour
{
    public string targetScene;                          // The scene to transfer the player to
    public string targetStaircaseID;                    // ID of the paired staircase in the target scene
    public Vector2 spawnOffset = new Vector2(1f, 0f);   // Offset for where the player will appear near the target staircase

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Use the GameSceneManager to handle the scene transition
            GameManager.Instance.LoadStaircaseScene(targetScene, targetStaircaseID, spawnOffset);
        }
    }
}
