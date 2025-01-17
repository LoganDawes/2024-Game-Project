using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyTurnManager : MonoBehaviour
{
    private List<Enemy> aliveEnemies = new List<Enemy>();
    private bool isProcessing = false;

    public static EnemyTurnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called at the start of the enemy turn
    public void StartEnemyTurn()
    {
        if (isProcessing) return; // Prevent double execution
        isProcessing = true;

        // Get all initialized enemies
        aliveEnemies.Clear();
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy != null && enemy.isInitialized())
            {
                aliveEnemies.Add(enemy);
            }
        }

        // Start processing the enemy turn
        StartCoroutine(ProcessEnemyTurn());
    }

    private IEnumerator ProcessEnemyTurn()
    {
        foreach (Enemy enemy in aliveEnemies)
        {
            // Simulate enemy action delay
            yield return new WaitForSeconds(1.0f);

            // Randomly choose an action (currently only Attack)
            EnemyAction(enemy);
        }

        // End enemy turn and return control to player
        EndEnemyTurn();
    }

    private void EnemyAction(Enemy enemy)
    {
        string selectedAction = "";

        // Determine enemy action
        if (enemy.GetName() == "Lurker" && Random.value <= 0.05f)
        {
            selectedAction = "Miss";
        }
        else if ((enemy.GetName() == "Insect" || enemy.GetName() == "Bat") && Random.value <= 0.03f)
        {
            selectedAction = "Miss";
        }
        else if (enemy.GetName() == "Bat" && Random.value <= 0.10f)
        {
            selectedAction = "Lifesteal";
        }
        else if (enemy.GetName() == "Insect" && Random.value <= 0.10f)
        {
            selectedAction = "Spawn";
        }
        else
        {
            selectedAction = "Attack";
        }

        switch (selectedAction)
        {
            case "Attack":
                {
                    // Perform attack: damage the player
                    int damage = enemy.GetAttack();
                    damage -= GameManager.Instance.GetDefense();
                    damage = Mathf.Max(0, damage); // Ensure damage is not negative

                    GameManager.Instance.DamagePlayer(damage);
                    Debug.Log($"{enemy.name} attacked the player for {damage} damage.");
                    break;
                }

            case "Miss":
                { 
                    // Perform miss: no damage
                    GameManager.Instance.MissPlayer();
                    Debug.Log($"{enemy.name} missed the attack.");
                    break;
                }

            case "Lifesteal":
                {
                    // Perform lifesteal: damage the player and heal the enemy
                    int damage = enemy.GetAttack();
                    damage -= GameManager.Instance.GetDefense();
                    damage = Mathf.Max(0, damage); // Ensure damage is not negative

                    GameManager.Instance.Lifesteal(damage);
                    enemy.Heal(damage);

                    Debug.Log($"{enemy.GetName()} performed a lifesteal attack, dealing {damage} damage and healing {damage} HP.");
                    break;
                }
            case "Spawn":
                {
                    if (GameManager.Instance.Spawn())
                    {
                        break;
                    }
                    else // Enemy could not spawn
                    {
                        // Perform attack: damage the player
                        int damage = enemy.GetAttack();
                        damage -= GameManager.Instance.GetDefense();
                        damage = Mathf.Max(0, damage); // Ensure damage is not negative

                        GameManager.Instance.DamagePlayer(damage);
                        Debug.Log($"{enemy.name} attacked the player for {damage} damage.");
                        break;
                    }
                }
        }
    }

    private void EndEnemyTurn()
    {
        isProcessing = false;
        BattleUIController.Instance.StartPlayerTurn(); // Switch back to the player's turn
    }
}
