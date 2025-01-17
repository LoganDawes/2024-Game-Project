using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Hashset of destroyed enemies
    private HashSet<int> destroyedEnemyIDs = new HashSet<int>();

    // Hashset of disabled enemies
    private HashSet<int> disabledEnemyIDs = new HashSet<int>();

    // Hashset of opened doors
    private HashSet<string> openDoors = new HashSet<string>();

    // Hashset of collected keys
    private HashSet<int> destroyedKeyIDs = new HashSet<int>();

    // Hashset of emptied barrels
    private HashSet<int> emptyBarrels = new HashSet<int>();

    // Hashset of read signs
    private HashSet<int> readSigns = new HashSet<int>();

    // Hashset of completed puzzles
    private HashSet<int> completePuzzles = new HashSet<int>();

    // Store the player's GameObject
    private GameObject player;

    // Store camera anchor GameObject
    private GameObject cameraAnchor;

    // Reference to the pause menu UI
    public GameObject pauseMenuPrefab;
    public GameObject pauseMenuUI;

    // Reference to the sign UI
    public GameObject signMenuPrefab;
    private GameObject signMenuUI;
    private TextMeshProUGUI signMessageText;

    // Reference to the clock UI
    public GameObject clockMenuPrefab;
    private GameObject clockMenuUI;

    // Currently active clock
    public Clock activeClock;

    public int storedHour1, storedMinute1;
    public int storedHour2, storedMinute2;
    public int storedHour3, storedMinute3;
    public int storedHour4, storedMinute4;

    // Reference to the Game Over menu UI
    public GameObject gameOverMenuPrefab;
    private GameObject gameOverUI;

    public AudioClip pauseSound;
    public AudioClip keySound;
    public AudioClip buttonSound;
    public AudioClip damageSound;
    public AudioClip missSound;
    public AudioClip healSound;
    public AudioClip lifestealSound;
    public AudioClip spawnSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    public float volumeLevel = 1f;  // Default volume level

    public bool IsPaused { get; private set; } = false;
    public bool isSignOpen = false;
    public bool isClockOpen = false;

    // Player prefab and camera controller
    public GameObject playerPrefab; // Reference to the player prefab to instantiate
    private CameraController cameraController; // Camera controller component

    // Player Stats
    private int maxHealth = 100;
    private int health = 100;
    private int maxSpecial = 25;
    private int special = 25;
    private int attack = 5;
    private int defense = 0;
    private int tempdefense = 0;

    // Player inventory
    private List<string> inventory = new List<string>();

    public Vector3 startingPlayerPosition = new Vector3(0, 1.6f, 0); // Player's initial start position
    private string lastLoadedScene = "Level 1";                 // Player's current loaded scene

    public enum SceneLoadType
    {
        MainMenu,
        GameScene,
        TransferPlayer,
        BattleScene,
        Restart
    }

    public enum BattleType
    {
        TestBattle,
        Lurker,
        Insect,
        Bat,
        Swarm,
        Combo1,
        Combo2
    }

    // Stored battle type
    private BattleType battleType;

    public int CurrentEnemyID { get; set; } // Stored overworld enemy ID during battle

    public List<Enemy> enemies;        // List to store references to enemies during battle
    public List<Sprite> enemySprites;   // List of enemy sprites for loading enemies

    private void Awake()
    {
        // Ensure there's only one instance of GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive
            Debug.Log("GameManager instance created");
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }

        // Get camera controller component
        cameraController = FindObjectOfType<CameraController>();

        // Get audio source component
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Instantiate the pause menu UI
        pauseMenuUI = Instantiate(pauseMenuPrefab);
        pauseMenuUI.SetActive(false); // Start with the pause menu hidden

        // Instantiate the sign menu UI
        signMenuUI = Instantiate(signMenuPrefab);
        signMenuUI.SetActive(false); // Start with the sign menu hidden

        // Instantiate the clock menu UI
        clockMenuUI = Instantiate(clockMenuPrefab);
        clockMenuUI.SetActive(false); // Start with the sign menu hidden

        inventory.Add("Sword");
    }

    private void Update()
    {
        // Check for pause input (space bar)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsPaused)
            {
                if (isSignOpen && pauseMenuUI.activeSelf)
                {
                    // Switch back to the sign menu while keeping the game paused
                    HidePauseMenuWhileSignOpen();
                }
                else if (isSignOpen)
                {
                    // Show the pause menu while the sign menu is open
                    ShowPauseMenuWhileSignOpen();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Method to pause the game
    public void PauseGame()
    {
        // pause sound
        audioSource.PlayOneShot(pauseSound);

        // show UI
        pauseMenuUI.SetActive(true);

        // Update player stats text fields
        DisplayPlayerStats();

        // Pause time
        Time.timeScale = 0f;
        IsPaused = true;

        Debug.Log("Game Paused");
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    // Method to resume the game
    public void ResumeGame()
    {
        // pause sound
        audioSource.PlayOneShot(pauseSound);

        if (isSignOpen)
        {
            CloseSignMenu();
            CloseClockMenu();
        }

        Time.timeScale = 1f;
        IsPaused = false;
        pauseMenuUI.SetActive(false);

        Debug.Log("Game Resumed");
    }

    // Open the sign menu
    public void OpenSignMenu(string message)
    {
        if (IsPaused) return; // Prevent sign opening while the game is paused

        // Display the sign menu UI
        signMenuUI.SetActive(true);

        // Get reference to the sign menu text component
        signMessageText = signMenuUI.transform.Find("Canvas/MessageText").GetComponent<TextMeshProUGUI>();

        signMessageText.text = message;

        // Pause the game
        Time.timeScale = 0f;
        IsPaused = true;
        isSignOpen = true;

        Debug.Log("Sign Menu Opened");
    }

    public void OpenClockMenu(Clock clock)
    {
        if (IsPaused || isClockOpen) return; // Prevent clock opening while the game is paused

        activeClock = clock;

        // Display the clock menu UI
        clockMenuUI.SetActive(true);

        player.GetComponent<PlayerMovement>().enabled = false;
        isSignOpen = true;
        isClockOpen = true;

        Debug.Log("Clock Menu Opened");
    }

    // Close the sign menu
    public void CloseSignMenu()
    {
        signMenuUI.SetActive(false);
        isSignOpen = false;
        Debug.Log("Sign Menu Closed");
    }

    public void CloseClockMenu()
    {
        player.GetComponent<PlayerMovement>().enabled = true;
        clockMenuUI.SetActive(false);

        isSignOpen = false;
        isClockOpen = false;

        activeClock = null;

        Debug.Log("Clock Menu Closed");
    }

    // Show the pause menu while the sign menu is open
    private void ShowPauseMenuWhileSignOpen()
    {
        // Hide the sign menu but keep the game paused
        signMenuUI.SetActive(false);
        clockMenuUI.SetActive(false);

        // Show the pause menu
        PauseGame();

        Debug.Log("Pause Menu Opened While Sign/Clock Menu Active");
    }

    private void HidePauseMenuWhileSignOpen()
    {
        // Hide the pause menu
        pauseMenuUI.SetActive(false);

        if (isClockOpen)
        { 
            clockMenuUI.SetActive(true);
            Debug.Log("Clock Menu Shown While Pause Menu Hidden");
        }
        else
        {
            // Show the sign menu again
            signMenuUI.SetActive(true);
            Debug.Log("Sign Menu Shown While Pause Menu Hidden");
        }
    }

    // Method to start the game and load a scene
    public void StartGame()
    {
        audioSource.PlayOneShot(buttonSound);
        StartCoroutine(LoadScene(SceneLoadType.GameScene, lastLoadedScene));
    }

    // Method to go back to main menu
    public void OpenMainMenu(string menuSceneName)
    {
        StartCoroutine(LoadScene(SceneLoadType.MainMenu, menuSceneName));
    }

    // Loading scene called by StaircaseTeleport
    public void LoadStaircaseScene(string targetScene, string targetStaircaseID, Vector2 spawnOffset)
    {
        StartCoroutine(LoadScene(SceneLoadType.TransferPlayer, targetScene, targetStaircaseID, spawnOffset));
    }

    public void JumpToScene(string targetScene, Vector2 spawnLocation)
    {
        lastLoadedScene = targetScene;
        startingPlayerPosition = spawnLocation;
        StartCoroutine(LoadScene(SceneLoadType.GameScene, lastLoadedScene));
    }

    // Loading battle scene called by an enemy
    public void LoadBattleScene(string battleSceneName, string enemyType)
    {
        // Parse enemyType to set battleType
        if (Enum.TryParse(enemyType, out BattleType parsedType))
        {
            battleType = parsedType;
        }
        else
        {
            Debug.LogWarning($"Enemy type '{enemyType}' not recognized");
            battleType = BattleType.TestBattle;
        }

        StartCoroutine(LoadScene(SceneLoadType.BattleScene, battleSceneName));
    }

    // Load scene Coroutine
    private IEnumerator LoadScene(SceneLoadType loadType, string sceneName, string targetStaircaseID = null, Vector2? spawnOffset = null)
    {
        // Save the player's current position and scene name if loading the main menu or battle
        if (loadType == SceneLoadType.MainMenu || loadType == SceneLoadType.BattleScene)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                startingPlayerPosition = player.transform.position;
                lastLoadedScene = SceneManager.GetActiveScene().name;
            }
        }

        Debug.Log($"Loading scene: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Display loading progress
        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null;
        }

        // Pause and sign menu not loaded in battle scene
        if (loadType != SceneLoadType.BattleScene)
        {
            // Instantiate the pause menu UI
            pauseMenuUI = Instantiate(pauseMenuPrefab);
            pauseMenuUI.SetActive(false); // Start with the pause menu hidden

            // Instantiate the sign menu UI
            signMenuUI = Instantiate(signMenuPrefab);
            signMenuUI.SetActive(false);

            clockMenuUI = Instantiate(clockMenuPrefab);
            clockMenuUI.SetActive(false);
        }

        switch (loadType)
        {
            case SceneLoadType.MainMenu:
                // No additional setup needed for MainMenu
                Debug.Log("Main Menu scene loaded.");
                break;

            case SceneLoadType.Restart:
                // No additional setup needed for Restart
                Debug.Log("Main Menu scene loaded for restart.");
                break;

            case SceneLoadType.GameScene:
                Debug.Log("Game scene loaded, spawning player...");

                // Instantiate the player at the saved starting position
                GameObject player = Instantiate(playerPrefab, startingPlayerPosition, Quaternion.identity);
                SetPlayer(player); // Register the new player

                Time.timeScale = 1f;
                HandleGameState();
                break;

            case SceneLoadType.BattleScene:
                Debug.Log("Battle scene loaded.");

                cameraAnchor = GameObject.FindGameObjectWithTag("Anchor");

                SetCameraFollowPlayer(cameraAnchor);

                Time.timeScale = 1f;
                InitiateBattleSequence(battleType);
                break;

            case SceneLoadType.TransferPlayer:
                Debug.Log("Scene loaded, finding staircase...");

                // Detect target staircase
                StaircaseTarget staircase = FindStaircaseByID(targetStaircaseID);
                if (staircase != null)
                {
                    Debug.Log($"Staircase position: {staircase.transform.position}");

                    // Calculate spawn position with optional offset
                    Vector3 spawnPosition = staircase.transform.position + (Vector3)spawnOffset.GetValueOrDefault(Vector2.zero);
                    Debug.Log($"Attempting to spawn player at position: {spawnPosition}");

                    // Get instance of player from GameManager
                    GameObject existingPlayer = GetPlayer();

                    if (existingPlayer == null)
                    {
                        // Instantiate a new player if none exists
                        existingPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                        SetPlayer(existingPlayer); // Register the new player
                    }
                    else
                    {
                        // Move existing player to the new position
                        existingPlayer.transform.position = spawnPosition;
                        SceneManager.MoveGameObjectToScene(existingPlayer, SceneManager.GetActiveScene());
                    }

                    Time.timeScale = 1f;
                    HandleGameState();
                }
                else
                {
                    Debug.LogError("Target staircase not found!");
                }
                break;
        }
    }


    private StaircaseTarget FindStaircaseByID(string staircaseID)
    {
        StaircaseTarget[] staircases = FindObjectsOfType<StaircaseTarget>();
        foreach (StaircaseTarget staircase in staircases)
        {
            if (staircase.staircaseID == staircaseID)
            {
                return staircase;
            }
        }
        return null;
    }

    private void HandleGameState()
    {
        // Keep state of enemies
        HandleEnemyStates();

        // Keep state of doors
        HandleDoorStates();

        // Keep state of keys
        HandleKeyStates();

        // Keep state of barrels
        HandleBarrelStates();

        // Keep state of signs
        HandleSignStates();

        // Keep state of puzzles
        HandlePuzzleStates();

        // Set target of camera to player
        SetCameraFollowPlayer(player);
    }

    private void HandleEnemyStates()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyAI enemyComponent = enemy.GetComponent<EnemyAI>();
            if (enemyComponent != null)
            {
                // Check if the enemy is destroyed
                if (IsEnemyDestroyed(enemyComponent.enemyID))
                {
                    Destroy(enemy); // Destroy the enemy if it has been marked as destroyed
                }
                else if (IsEnemyDisabled(enemyComponent.enemyID))
                {
                    enemyComponent.DisableForDuration(3);
                    enemyComponent.SetPlayerReference(GetPlayer());
                }
                else
                {
                    // Set the enemy's player reference to the current player
                    enemyComponent.SetPlayerReference(GetPlayer());
                }
            }
        }
    }

    private void HandleDoorStates()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door door in doors)
        {
            if (IsDoorOpen(door.doorID))
            {
                door.OpenDoor();
            }
        }
    }

    private void HandleKeyStates()
    {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (GameObject key in keys)
        {
            Key keyComponent = key.GetComponent<Key>();
            if (keyComponent != null)
            {
                // Check if the key is destroyed
                if (IsKeyDestroyed(keyComponent.keyID))
                {
                    Destroy(key); // Destroy the key if it has been marked as destroyed
                }
            }
        }
    }

    private void HandleBarrelStates()
    {
        Barrel[] barrels = FindObjectsOfType<Barrel>();
        foreach (Barrel barrel in barrels)
        {
            if (IsBarrelEmpty(barrel.barrelID))
            {
                barrel.EmptyBarrel();
            }
        }
    }

    private void HandleSignStates()
    {
        Sign[] signs = FindObjectsOfType<Sign>();
        foreach (Sign sign in signs)
        {
            if (IsSignRead(sign.signID))
            {
                sign.StopFlickering();
            }
        }
    }

    private void HandlePuzzleStates()
    {
        ButtonPuzzle[] buttonPuzzles = FindObjectsOfType<ButtonPuzzle>();
        foreach (ButtonPuzzle buttonPuzzle in buttonPuzzles)
        {
            if (IsPuzzleComplete(buttonPuzzle.puzzleID))
            {
                buttonPuzzle.puzzleCompleted = true;
            }
        }

        ClockPuzzle[] clockPuzzles = FindObjectsOfType<ClockPuzzle>();
        foreach (ClockPuzzle clockPuzzle in clockPuzzles)
        {
            if (IsPuzzleComplete(clockPuzzle.puzzleID))
            {
                clockPuzzle.puzzleCompleted = true;
            }
            clockPuzzle.clock1.SetTime(storedHour1, storedMinute1);
            clockPuzzle.clock2.SetTime(storedHour2, storedMinute2);
            clockPuzzle.clock3.SetTime(storedHour3, storedMinute3);
            clockPuzzle.clock4.SetTime(storedHour4, storedMinute4);
        }
    }

    private void SetCameraFollowPlayer(GameObject player)
    {
        if (cameraController != null)
        {
            cameraController.SetTarget(player.transform);
        }
    }

    // Registers enemy to destroyed enemies
    public void RegisterEnemyDestruction(int enemyID)
    {
        Debug.Log("Registered Destruction of enemy ID: " + enemyID);
        destroyedEnemyIDs.Add(enemyID);
    }

    // Registers enemy as disabled
    public void RegisterEnemyDisabled(int enemyID)
    {
        Debug.Log("Registered Disabling of ID: " + enemyID);
        disabledEnemyIDs.Add(enemyID);
    }

    // Registers enemy as disabled
    public void DeregisterEnemyDisabled(int enemyID)
    {
        Debug.Log("Deregistered Disabling of ID: " + enemyID);
        disabledEnemyIDs.Remove(enemyID);
    }

    // Registers key to destroyed keys and destroys
    public void RegisterKeyDestruction(Key key)
    {
        // key sound
        audioSource.PlayOneShot(keySound);
        destroyedKeyIDs.Add(key.keyID);
        Destroy(key.gameObject);
    }

    public void RegisterEmptyBarrel(Barrel barrel)
    {
        emptyBarrels.Add(barrel.barrelID);
        barrel.EmptyBarrel();
    }

    public void RegisterReadSign(Sign sign)
    {
        readSigns.Add(sign.signID);
        sign.StopFlickering();
    }

    public void RegisterCompletedPuzzle(ButtonPuzzle buttonPuzzle)
    {
        completePuzzles.Add(buttonPuzzle.puzzleID);
        buttonPuzzle.puzzleCompleted = true;
    }

    public void RegisterCompletedPuzzle(ClockPuzzle clockPuzzle)
    {
        completePuzzles.Add(clockPuzzle.puzzleID);
        clockPuzzle.puzzleCompleted = true;
    }

    // Check if provided key ID is destroyed
    public bool IsKeyDestroyed(int keyID)
    {
        return destroyedKeyIDs.Contains(keyID);
    }

    // Check if provided enemy ID is destroyed
    public bool IsEnemyDestroyed(int enemyID)
    {
        return destroyedEnemyIDs.Contains(enemyID);
    }

    // Check if provided enemy ID is disabled
    public bool IsEnemyDisabled(int enemyID)
    {
        return disabledEnemyIDs.Contains(enemyID);
    }

    // Registers an open door
    public void RegisterOpenDoor(string doorID)
    {
        openDoors.Add(doorID);
    }

    // Check if a provided door ID is open
    public bool IsDoorOpen(string doorID)
    {
        return openDoors.Contains(doorID);
    }

    public bool IsBarrelEmpty(int barrelID)
    {
        return emptyBarrels.Contains(barrelID);
    }

    public bool IsSignRead(int signID)
    {
        return readSigns.Contains(signID);
    }

    public bool IsPuzzleComplete(int puzzleID)
    {
        return completePuzzles.Contains(puzzleID);
    }

    // Method to set the player object
    public void SetPlayer(GameObject playerObject)
    {
        player = playerObject;
    }

    // Method to get the player object
    public GameObject GetPlayer()
    {
        return player;
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetSpecialPoints()
    {
        return special;
    }

    public int GetMaxSpecialPoints()
    {
        return maxSpecial;
    }

    public int GetAttack()
    {
        return attack;
    }

    public int GetDefense()
    {
        return defense + tempdefense;
    }

    public void TemporaryDefense(int amount)
    {
        tempdefense += amount;
    }

    public void ResetTemporaryDefense()
    {
        tempdefense = 0;
    }

    // Method to add items to the inventory
    public void AddItemToInventory(string item)
    {
        inventory.Add(item);
        if (IsPaused)
        {
            DisplayPlayerStats();
        }
    }

    public void RemoveItemFromInventory(string item)
    {
        inventory.Remove(item);
        if (IsPaused)
        {
            DisplayPlayerStats();
        }
    }

    public bool InventoryContains(string item)
    {
        return inventory.Contains(item);
    }

    private void DisplayPlayerStats()
    {
        if (pauseMenuUI != null)
        {
            // Find and update health display
            TextMeshProUGUI healthDisplay = pauseMenuUI.transform.Find("Health Display").GetComponent<TextMeshProUGUI>();
            healthDisplay.text = $"HP: {GetHealth()}";

            // Find and update special points display
            TextMeshProUGUI specialDisplay = pauseMenuUI.transform.Find("Special Display").GetComponent<TextMeshProUGUI>();
            specialDisplay.text = $"SP: {GetSpecialPoints()}";

            // Find and update attack display
            TextMeshProUGUI attackDisplay = pauseMenuUI.transform.Find("Attack Display").GetComponent<TextMeshProUGUI>();
            attackDisplay.text = $"ATK: {GetAttack()}";

            // Find and update defense display
            TextMeshProUGUI defenseDisplay = pauseMenuUI.transform.Find("Defense Display").GetComponent<TextMeshProUGUI>();
            defenseDisplay.text = $"DEF: {GetDefense()}";

            // Find and update inventory display
            TextMeshProUGUI inventoryDisplay = pauseMenuUI.transform.Find("Inventory Display").GetComponent<TextMeshProUGUI>();
            inventoryDisplay.text = "Inventory:\n";

            // Create a dictionary to store item counts
            Dictionary<string, int> itemCounts = new Dictionary<string, int>();

            // Populate the dictionary with item counts
            foreach (string item in inventory)
            {
                if (itemCounts.ContainsKey(item))
                {
                    itemCounts[item]++;
                }
                else
                {
                    itemCounts[item] = 1;
                }
            }

            // Display each item with its count in the inventory UI
            foreach (var kvp in itemCounts)
            {
                string itemName = kvp.Key;
                int count = kvp.Value;

                if (count > 1)
                {
                    inventoryDisplay.text += $"- {itemName} x{count}\n";
                }
                else
                {
                    inventoryDisplay.text += $"- {itemName}\n";
                }
            }

        }
    }

    private void InitiateBattleSequence(BattleType battleType)
    {
        enemies = new List<Enemy>(FindObjectsByType<Enemy>((FindObjectsSortMode)FindObjectsInactive.Exclude));

        enemies[0] = GameObject.Find("Enemy01")?.GetComponent<Enemy>();
        enemies[1] = GameObject.Find("Enemy02")?.GetComponent<Enemy>();
        enemies[2] = GameObject.Find("Enemy03")?.GetComponent<Enemy>();

        switch (battleType)
        {
            case BattleType.TestBattle:
                Debug.Log("Test battle initiated!");
                enemies[0].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                enemies[1].InitializeEnemy("Big Lurker", 10, 100, 0, enemySprites[0]);
                enemies[2].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                break;
            case BattleType.Lurker:
                enemies[0].UninitializeEnemy();
                enemies[1].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                enemies[2].UninitializeEnemy();
                break;
            case BattleType.Insect:
                enemies[0].UninitializeEnemy();
                enemies[1].InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                enemies[2].UninitializeEnemy();
                break;
            case BattleType.Bat:
                enemies[0].UninitializeEnemy();
                enemies[1].InitializeEnemy("Bat", 7, 2, 0, enemySprites[2]);
                enemies[2].UninitializeEnemy();
                break;
            case BattleType.Swarm:
                enemies[0].InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                enemies[1].InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                enemies[2].InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                break;
            case BattleType.Combo1:
                enemies[0].InitializeEnemy("Bat", 7, 2, 0, enemySprites[2]);
                enemies[1].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                enemies[2].InitializeEnemy("Bat", 7, 2, 0, enemySprites[2]);
                break;
            case BattleType.Combo2:
                enemies[0].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                enemies[1].InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                enemies[2].InitializeEnemy("Lurker", 10, 3, 0, enemySprites[0]);
                break;
        }
    }

    public void DamagePlayer(int damage)
    {
        audioSource.PlayOneShot(damageSound);

        BattleUIController.Instance.ShowPlayerDamageText(damage);

        health -= damage;

        BattleUIController.Instance.UpdatePlayerHPBar(health);

        if (health <= 0)
        {
            GameOver();
        }
    }

    public void HealPlayer(int heal)
    {
        audioSource.PlayOneShot(healSound);

        BattleUIController.Instance.ShowPlayerHealText(heal);

        health += heal;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        BattleUIController.Instance.UpdatePlayerHPBar(health);
    }

    public void MissPlayer()
    {
        audioSource.PlayOneShot(missSound);
        BattleUIController.Instance.ShowPlayerDamageText(0);
    }

    public void DepleteSP(int depletion)
    {
        BattleUIController.Instance.ShowSPDepletionText(depletion);

        special -= depletion;

        BattleUIController.Instance.UpdatePlayerSPBar(special);
    }

    public void RestoreSP(int restoration)
    {
        audioSource.PlayOneShot(healSound);

        BattleUIController.Instance.ShowPlayerRestoreText(restoration);

        special += restoration;

        if (special > maxSpecial)
        {
            special = maxSpecial;
        }

        BattleUIController.Instance.UpdatePlayerSPBar(special);
    }

    public void Lifesteal(int damage)
    {
        audioSource.PlayOneShot(damageSound);
        audioSource.PlayOneShot(lifestealSound);

        BattleUIController.Instance.ShowPlayerDamageText(damage);

        health -= damage;

        BattleUIController.Instance.UpdatePlayerHPBar(health);

        if (health <= 0)
        {
            GameOver();
        }
    }

    public bool Spawn()
    {
        foreach (Enemy potentialEnemy in enemies)
        {
            if (!potentialEnemy.isInitialized())
            {
                potentialEnemy.InitializeEnemy("Insect", 5, 3, 0, enemySprites[1]);
                Debug.Log("An Insect enemy spawned another Insect!");
                audioSource.PlayOneShot(spawnSound);
                return true;
            }
        }
        Debug.Log("Failed spawn attempt");
        return false;
    }

    public void EnemyDeathSound()
    {
        audioSource.PlayOneShot(deathSound);
    }

    private void GameOver()
    {
        BattleUIController.Instance.Defeat();

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        Time.timeScale = 0f;
        gameOverUI = Instantiate(gameOverMenuPrefab);
        gameOverUI.SetActive(true);
    }

    public void ResetGame()
    {
        audioSource.PlayOneShot(buttonSound);

        // Reset destroyed objects
        destroyedEnemyIDs.Clear();
        destroyedKeyIDs.Clear();
        openDoors.Clear();
        emptyBarrels.Clear();
        readSigns.Clear();
        completePuzzles.Clear();

        storedHour1 = 12;
        storedMinute1 = 0;
        storedHour2 = 12;
        storedMinute2 = 0;
        storedHour3 = 12;
        storedMinute3 = 0;
        storedHour4 = 12;
        storedMinute4 = 0;

        // Reset stats
        health = maxHealth;
        special = maxSpecial;
        attack = 5;
        defense = 0;
        tempdefense = 0;

        // Reset player inventory
        inventory.Clear();
        inventory.Add("Sword");

        // Reset player position and scene
        startingPlayerPosition = new Vector3(0, 1.6f, 0); // Default starting position
        lastLoadedScene = "Level 1"; // Default starting scene

        StartCoroutine(LoadScene(SceneLoadType.Restart, "MainMenu"));
    }
}
