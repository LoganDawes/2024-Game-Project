using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static GameManager;
using System.Collections;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    public TextMeshProUGUI[] optionTexts;  // Array of main option texts
    public GameObject[] submenus;          // Array of submenu panels

    public GameObject victoryUI;            // Victory UI overlay
    private GameObject statSubmenu;          // Stats submenu for display

    private int selectedIndex = 0;         // Tracks the currently selected main option
    private int submenuIndex = 0;          // Tracks the currently selected submenu option

    private bool submenuActive = false;    // Tracks if a submenu is active
    private bool targetingEnemy = false;   // Flag for enemy targeting mode
    private bool playerTurn = true;        // Track if it's the player's turn

    private List<Enemy> enemies = new List<Enemy>();  // List of enemies for target selection
    private Enemy currentTarget;                      // Reference to the currently selected enemy

    public AudioClip selectSound;           // Audio clip for menu option selected
    public AudioClip damageSound;           // Audio clip for damaging an enemy
    public AudioClip failureSound;          // Audio clip for fail to select
    private AudioSource audioSource;        // Audio source component

    public Slider hpBarSlider;  // Battle menu player HP
    public Slider spBarSlider;  // Battle menu player SP
    public TextMeshProUGUI damageText;  // Battle menu player damage text
    public TextMeshProUGUI spdepleteText;  // Battle menu player sp depletion text

    public static BattleUIController Instance { get; private set; }

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

    private void Start()
    {
        // Get audio source component
        audioSource = GetComponent<AudioSource>();

        // Get stat submenu from submenu array
        statSubmenu = submenus[2];

        UpdateUI();
        hpBarSlider.maxValue = GameManager.Instance.GetMaxHealth();
        UpdatePlayerHPBar(GameManager.Instance.GetHealth());

        spBarSlider.maxValue = GameManager.Instance.GetMaxSpecialPoints();
        UpdatePlayerSPBar(GameManager.Instance.GetSpecialPoints());

        damageText.gameObject.SetActive(false);
        spdepleteText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerTurn) return;
        if (!submenuActive && !targetingEnemy)
        {
            // Navigate main options with W/S
            if (Input.GetKeyDown(KeyCode.W))
            {
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : optionTexts.Length - 1;
                UpdateUI();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                selectedIndex = (selectedIndex < optionTexts.Length - 1) ? selectedIndex + 1 : 0;
                UpdateUI();
            }
            // Open submenu with D or E
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E))
            {
                OpenSubmenu();
            }
        }
        else if (submenuActive && !targetingEnemy)
        {
            // Navigate submenu options (only for submenus with multiple options)
            if (Input.GetKeyDown(KeyCode.W))
            {
                submenuIndex = (submenuIndex > 0) ? submenuIndex - 1 : submenus[selectedIndex].transform.childCount - 1;
                UpdateSubmenuUI();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                submenuIndex = (submenuIndex < submenus[selectedIndex].transform.childCount - 1) ? submenuIndex + 1 : 0;
                UpdateSubmenuUI();
            }
            // Execute selected submenu option with E
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ExecuteSubmenuAction();
            }
            // Close submenu with A
            else if (Input.GetKeyDown(KeyCode.A))
            {
                CloseSubmenu();
            }
        }
        else if (targetingEnemy)
        {
            // Navigate between enemies using W and S
            if (Input.GetKeyDown(KeyCode.W))
            {
                int index = enemies.IndexOf(currentTarget);
                index = (index > 0) ? index - 1 : enemies.Count - 1;
                HighlightEnemy(enemies[index]);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                int index = enemies.IndexOf(currentTarget);
                index = (index < enemies.Count - 1) ? index + 1 : 0;
                HighlightEnemy(enemies[index]);
            }
            // Confirm the target with E
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ResetEnemyGlow();
                Debug.Log($"Target selected: {currentTarget.name}");
                targetingEnemy = false;  // Stop targeting mode
                PerformJabAction(currentTarget);
            }
        }
    }

    private void UpdateUI()
    {
        audioSource.PlayOneShot(selectSound);
        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i == selectedIndex)
                optionTexts[i].text = $"> {optionTexts[i].text.TrimStart('>', ' ')}"; // Add ">"
            else
                optionTexts[i].text = optionTexts[i].text.TrimStart('>', ' ');         // Remove ">"
        }
    }

    private void OpenSubmenu()
    {
        audioSource.PlayOneShot(selectSound);
        submenuActive = true;
        submenus[selectedIndex].SetActive(true);
        submenuIndex = 0; // Reset submenu index when opened
        UpdateSubmenuUI();

        // Display stats when stat submenu opened
        if (selectedIndex == 2)
        {
            DisplayPlayerStats();
        }
    }

    private void CloseSubmenu()
    {
        audioSource.PlayOneShot(selectSound);
        submenuActive = false;
        submenus[selectedIndex].SetActive(false);
    }

    private void UpdateSubmenuUI()
    {
        // Iterate through submenu options and update their appearance
        Transform submenuTransform = submenus[selectedIndex].transform;
        for (int i = 0; i < submenuTransform.childCount; i++)
        {
            TextMeshProUGUI optionText = submenuTransform.GetChild(i).GetComponent<TextMeshProUGUI>();
            optionText.text = (i == submenuIndex) ? $"> {optionText.text.TrimStart('>', ' ')}" : optionText.text.TrimStart('>', ' ');
        }
    }

    private void ExecuteSubmenuAction()
    {
        if (selectedIndex == 3) // Run Action
        {
            PerformRunAction();
        }
        else if (selectedIndex == 0) // Attack
        {
            switch (submenuIndex)
            {
                case 0: // Jab Action
                    Debug.Log("Performing Jab action.");
                    PickTarget();           // Switch to enemy selection
                    targetingEnemy = true;  // Enable targeting mode
                    break;
                case 1: // Slash Action
                    if (GameManager.Instance.GetSpecialPoints() >= 3)
                    {
                        Debug.Log("Performing Slash action.");
                        PerformSlashAction();
                    }
                    else
                    {
                        audioSource.PlayOneShot(failureSound);
                        Debug.Log("Insufficient SP Cost.");
                    }
                    break;
            }
        }
        else if (selectedIndex == 1) // Defend
        {
            switch (submenuIndex)
            {
                case 0: // Defend Action
                    Debug.Log("Performing Defend action.");
                    PerformDefendAction();
                    break;

                case 1: // Heal Action
                    if (GameManager.Instance.GetSpecialPoints() >= 10)
                    {
                        Debug.Log("Performing Heal action.");
                        PerformHealAction();
                        break;
                    }
                    else
                    {
                        audioSource.PlayOneShot(failureSound);
                        Debug.Log("Insufficient SP Cost.");
                        break;
                    }
            }
            
        }
    }

    // Display Stats in stat submenu
    private void DisplayPlayerStats()
    {
            // Find and update health display
            TextMeshProUGUI healthDisplay = statSubmenu.transform.Find("Health Display").GetComponent<TextMeshProUGUI>();
            healthDisplay.text = $"HP: {GameManager.Instance.GetHealth()}";

            // Find and update special points display
            TextMeshProUGUI specialDisplay = statSubmenu.transform.Find("Special Display").GetComponent<TextMeshProUGUI>();
            specialDisplay.text = $"SP: {GameManager.Instance.GetSpecialPoints()}";

            // Find and update attack display
            TextMeshProUGUI attackDisplay = statSubmenu.transform.Find("Attack Display").GetComponent<TextMeshProUGUI>();
            attackDisplay.text = $"ATK: {GameManager.Instance.GetAttack()}";

            // Find and update defense display
            TextMeshProUGUI defenseDisplay = statSubmenu.transform.Find("Defense Display").GetComponent<TextMeshProUGUI>();
            defenseDisplay.text = $"DEF: {GameManager.Instance.GetDefense()}";

    }

    public void StartPlayerTurn()
    {
        playerTurn = true;

        GameManager.Instance.ResetTemporaryDefense();

        Debug.Log("Player's turn begins!");
    }

    private void EndTurn()
    {
        // Check if all enemies have been defeated
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy != null && enemy.isInitialized())
            {
                playerTurn = false;
                EnemyTurnManager.Instance.StartEnemyTurn(); // Start the enemy turn
                return;
            }
        }

        // Trigger victory if no enemies are alive
        TriggerVictory();
    }

    private void TriggerVictory()
    {
        GameManager.Instance.ResetTemporaryDefense();

        // Destroy the enemy that triggered the battle
        if (GameManager.Instance.CurrentEnemyID != -1)
        {
            GameManager.Instance.RegisterEnemyDestruction(GameManager.Instance.CurrentEnemyID);
            GameManager.Instance.CurrentEnemyID = -1; // Clear the reference after destruction
        }

        // Disable player input, enable victory UI, load game scene after delay
        this.enabled = false;  // Disables further input handling
        victoryUI.SetActive(true);

        StartCoroutine(VictoryScreen());
    }

    public void Defeat()
    {
        this.enabled = false;
    }

    private IEnumerator VictoryScreen()
    {
        yield return new WaitForSeconds(3);  // Wait for 3 seconds before returning
        GameManager.Instance.StartGame();
    }

    // Initializes the target selection
    private void PickTarget()
    {
        // Get all initialized enemy objects from the scene
        enemies.Clear();
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy != null && enemy.isInitialized())  // Ensure the enemy is not null and is initialized
            {
                enemies.Add(enemy);
            }
        }

        HighlightEnemy(enemies[0]);
    }

    // Highlight the selected enemy
    private void HighlightEnemy(Enemy enemy)
    {
        // Reset previous target glow
        ResetEnemyGlow();

        // Apply a glow effect to the current target
        enemy.ApplyGlowEffect(true);
        currentTarget = enemy;

        Debug.Log("Target: "+ currentTarget);
    }

    // Reset the glow effect
    private void ResetEnemyGlow()
    {
        if (currentTarget != null)
        {
            currentTarget.ApplyGlowEffect(false); // Remove glow effect
        }
    }

    private void PerformJabAction(Enemy target)
    {
        CloseSubmenu();

        if (target != null)
        {
            // Get the player's attack value
            int damage = GameManager.Instance.GetAttack();
            damage -= target.GetDefense();

            // Apply damage to the enemy's current health
            target.TakeDamage(damage);
            audioSource.PlayOneShot(damageSound);

            EndTurn();
        }
    }

    private void PerformSlashAction()
    {
        CloseSubmenu();

        GameManager.Instance.DepleteSP(3);

        // Get the player's attack value divided by 3 (rounded up)
        int damage = Mathf.CeilToInt((float)GameManager.Instance.GetAttack() / 3);

        // Apply damage to each enemy's current health
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy != null && enemy.isInitialized())  // Ensure the enemy is not null and is initialized
            {
                damage -= enemy.GetDefense();
                enemy.TakeDamage(damage);
            }
        }
        audioSource.PlayOneShot(damageSound);

        EndTurn();
    }

    private void PerformDefendAction()
    {
        CloseSubmenu();
        GameManager.Instance.RestoreSP(2);

        GameManager.Instance.TemporaryDefense(2);

        EndTurn();
    }

    private void PerformHealAction()
    {
        CloseSubmenu();
        GameManager.Instance.DepleteSP(10);

        GameManager.Instance.HealPlayer(10);

        EndTurn();
    }

    private void PerformRunAction()
    {
        GameManager.Instance.ResetTemporaryDefense();

        // Disable the enemy that triggered the battle
        if (GameManager.Instance.CurrentEnemyID != -1)
        {
            GameManager.Instance.RegisterEnemyDisabled(GameManager.Instance.CurrentEnemyID);
            GameManager.Instance.CurrentEnemyID = -1;
        }

        GameManager.Instance.StartGame();
    }

    public void UpdatePlayerHPBar(int health)
    {
        if (hpBarSlider != null)
        {
            hpBarSlider.value = health;  // Set the slider's value to current health
        }
    }

    public void ShowPlayerDamageText(int damage)
    {
        damageText.color = Color.red;
        damageText.text = $"-{damage}";
        damageText.gameObject.SetActive(true);

        StartCoroutine(HideDamageText());
    }

    public void ShowPlayerHealText(int heal)
    {
        damageText.color = Color.green;
        damageText.text = $"+{heal}";
        damageText.gameObject.SetActive(true);

        StartCoroutine(HideDamageText());
    }

    private IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(0.5f);
        damageText.gameObject.SetActive(false);
    }

    public void UpdatePlayerSPBar(int special)
    {
        if (hpBarSlider != null)
        {
            spBarSlider.value = special;  // Set the slider's value to current special
        }
    }

    public void ShowSPDepletionText(int depletion)
    {
        spdepleteText.color = Color.red;
        spdepleteText.text = $"-{depletion}";
        spdepleteText.gameObject.SetActive(true);

        StartCoroutine(HideDepletionText());
    }

    public void ShowPlayerRestoreText(int restoration)
    {
        spdepleteText.color = Color.yellow;
        spdepleteText.text = $"+{restoration}";
        spdepleteText.gameObject.SetActive(true);

        StartCoroutine(HideDepletionText());
    }

    private IEnumerator HideDepletionText()
    {
        yield return new WaitForSeconds(0.5f);
        spdepleteText.gameObject.SetActive(false);
    }
}
