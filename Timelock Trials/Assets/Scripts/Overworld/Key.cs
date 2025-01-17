using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Key : MonoBehaviour
{
    public int keyID;    // ID used by Game manager

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.AddItemToInventory("Key");
            GameManager.Instance.RegisterKeyDestruction(this);
        }
    }
}
