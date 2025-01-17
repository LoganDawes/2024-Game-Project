using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private string enemyName;
    private int maxHealth;
    private int currentHealth;
    private int attack;
    private int defense;
    private SpriteRenderer spriteRenderer;

    private bool initialized = false;

    public Slider hpBarSlider;
    public TextMeshProUGUI damageText;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        hpBarSlider.gameObject.SetActive(false);
        damageText.gameObject.SetActive(false);
    }

    // Method to initialize enemy attributes
    public void InitializeEnemy(string name, int health, int attack, int defense, Sprite sprite)
    {
        this.enemyName = name;
        this.maxHealth = health;
        this.currentHealth = health;
        this.attack = attack;
        this.defense = defense;
        spriteRenderer.sprite = sprite;
        initialized = true;

        hpBarSlider.gameObject.SetActive(true);  // Show the HP bar when initialized
        hpBarSlider.maxValue = maxHealth;  // Set max value to maxHealth
        UpdateHPBar();
    }

    public void UninitializeEnemy()
    {
        this.enemyName= null;
        this.maxHealth = 0;
        this.attack = 0;
        this.defense = 0;
        this.spriteRenderer.sprite = null;
        initialized = false;

        hpBarSlider.gameObject.SetActive(false);
    }

    // Method to check if the enemy is initialized
    public bool isInitialized()
    {
        return initialized;
    }

    public string GetName() { return this.enemyName; }

    public int GetAttack()
    {
        return attack;
    }

    public int GetDefense() { return defense; }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{enemyName} took {damage} damage. Health remaining: {currentHealth}");

        ShowDamageText(damage);
        UpdateHPBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int heal)
    {
        currentHealth += heal;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"{enemyName} healed {heal} HP. Health remaining: {currentHealth}");

        ShowHealText(heal);
        UpdateHPBar();
    }

    private void ShowDamageText(int damage)
    {
        damageText.color = Color.red;
        damageText.text = $"-{damage}";
        damageText.gameObject.SetActive(true);

        StartCoroutine(HideDamageText());
    }

    private void ShowHealText(int heal)
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

    private void Die()
    {
        Debug.Log($"{enemyName} has been defeated!");
        GameManager.Instance.EnemyDeathSound();
        UninitializeEnemy();
    }

    private void UpdateHPBar()
    {
        if (hpBarSlider != null)
        {
            hpBarSlider.value = currentHealth;  // Set the slider's value to current health
        }
    }

    // Method to apply a glow effect to the enemy
    public void ApplyGlowEffect(bool enable)
    {
        if (enable)
        {
            // Apply a glow effect by changing the material's shader to something that supports glowing
            spriteRenderer.color = Color.yellow;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}
