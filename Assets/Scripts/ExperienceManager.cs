// ExperienceManager.cs
// Add this to your Player GameObject
// Handles experience gain, leveling, and stat progression

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int maxLevel = 100;
    [SerializeField] private float expMultiplier = 1.5f; // How much harder each level gets
    
    [Header("Level Up Rewards")]
    [SerializeField] private float healthPerLevel = 10f;
    [SerializeField] private float damagePerLevel = 2f;
    [SerializeField] private float armorPerLevel = 1f;
    
    [Header("UI References")]
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private GameObject levelUpEffect;
    
    private PlayerStats playerStats;
    private int expToNextLevel;
    
    // Events
    public event System.Action<int> OnLevelUp;
    public event System.Action<int> OnExpGained;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        expToNextLevel = CalculateExpForLevel(currentLevel + 1);
        UpdateUI();
    }
    
    // Calculate experience needed for a specific level
    int CalculateExpForLevel(int level)
    {
        // Exponential curve: 100 * (level^1.5)
        return Mathf.RoundToInt(100 * Mathf.Pow(level, expMultiplier));
    }
    
    // Award experience (call from EnemyStats.Die())
    public void GainExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;
        
        currentExp += amount;
        OnExpGained?.Invoke(amount);
        
        // Show floating exp text
        ShowExpGainText(amount);
        
        // Check for level up
        while (currentExp >= expToNextLevel && currentLevel < maxLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
    }
    
    void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel;
        expToNextLevel = CalculateExpForLevel(currentLevel + 1);
        
        // Apply stat increases (you'll need to add these methods to PlayerStats)
        if (playerStats != null)
        {
            // Heal player to full on level up
            playerStats.Heal(9999f);
        }
        
        // Visual effects
        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }
        
        OnLevelUp?.Invoke(currentLevel);
        
        Debug.Log($"LEVEL UP! Now level {currentLevel}");
        Debug.Log($"New stats: +{healthPerLevel} HP, +{damagePerLevel} DMG, +{armorPerLevel} ARM");
    }
    
    void ShowExpGainText(int amount)
    {
        GameObject textObj = new GameObject("ExpGainText");
        textObj.transform.position = transform.position + Vector3.up * 2.5f;
        
        Canvas canvas = textObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"+{amount} EXP";
        text.fontSize = 24;
        text.color = Color.cyan;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;
        
        // Add animation component
        ExpGainAnimation anim = textObj.AddComponent<ExpGainAnimation>();
        anim.Initialize();
    }
    
    void UpdateUI()
    {
        if (expBar != null)
        {
            float progress = (float)currentExp / expToNextLevel;
            expBar.fillAmount = progress;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
        
        if (expText != null)
        {
            expText.text = $"{currentExp} / {expToNextLevel} EXP";
        }
    }
    
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;
    public float GetLevelProgress() => (float)currentExp / expToNextLevel;
}

// Helper class for exp gain animation
public class ExpGainAnimation : MonoBehaviour
{
    private float lifetime = 2f;
    private float moveSpeed = 1f;
    private TextMeshProUGUI text;
    
    public void Initialize()
    {
        text = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        if (text != null)
        {
            Color color = text.color;
            color.a = Mathf.Lerp(1f, 0f, 1f - (lifetime / 2f));
            text.color = color;
        }
        
        lifetime -= Time.deltaTime;
    }
}