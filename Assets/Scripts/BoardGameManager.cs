using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardGameManager : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 2;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float timeBetweenWaves = 3f;

    private Transform[] waypoints;

    private int currentWave = 0;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;

    private bool waveInProgress;
    private bool gameEnded;

    [Header("Wave Reward")]
    [SerializeField] private int coinsPerWave = 50;

    private EconomyManager economyManager;

    private BaseHealth baseHealth;
    private Button startWaveButton;
    private TMP_Text healthText;
    private TMP_Text waveText;
    private TMP_Text coinsText;

    private GameObject victoryOverlay;
    private GameObject defeatOverlay;

    private void Start()
    {
        waypoints = new Transform[]
        {
            transform.Find("Waypoint_Start"),
            transform.Find("Waypoint_1"),
            transform.Find("Waypoint_2"),
            transform.Find("Waypoint_End")
        };

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogError(
                    $"Missing waypoint at index {i}. " +
                    "Check BoardBase waypoint names."
                );
            }
        }

        economyManager = FindFirstObjectByType<EconomyManager>();
        baseHealth = GetComponent<BaseHealth>();

        startWaveButton = GameObject.Find("StartWaveButton")?.GetComponent<Button>();
        if (startWaveButton != null)
            startWaveButton.onClick.AddListener(StartWave);

        healthText = GameObject.Find("HealthText")?.GetComponent<TMP_Text>();
        waveText = GameObject.Find("WaveText")?.GetComponent<TMP_Text>();
        coinsText = GameObject.Find("CoinsText")?.GetComponent<TMP_Text>();

        CreateResultOverlays();

        if (economyManager == null)
        {
            Debug.LogError("EconomyManager not found.");
        }

        UpdateHud();
    }

    private void Update()
    {
        UpdateHud();
    }

    public void StartWave()
    {
        if (gameEnded || waveInProgress)
            return;

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (gameEnded)
            return;

        if (currentWave >= totalWaves)
        {
            WinGame();
            return;
        }

        currentWave++;
        enemiesSpawned = 0;
        enemiesAlive = 0;
        waveInProgress = true;

        Debug.Log($"WAVE {currentWave} STARTED");

        InvokeRepeating(
            nameof(SpawnEnemy),
            0f,
            spawnInterval
        );
    }

    private void SpawnEnemy()
    {
        if (gameEnded)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned.");
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        if (enemiesSpawned >= enemiesPerWave)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            waypoints[0].position,
            Quaternion.identity
        );

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

        if (movement != null)
            movement.waypoints = waypoints;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (health != null)
            health.SetWaveManager(this);

        enemiesSpawned++;
        enemiesAlive++;

        Debug.Log(
            $"Wave {currentWave}: " +
            $"{enemiesSpawned}/{enemiesPerWave} spawned"
        );
    }

    public void EnemyFinished()
    {
        if (gameEnded)
            return;

        enemiesAlive--;

        if (enemiesAlive < 0)
            enemiesAlive = 0;

        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        if (!waveInProgress)
            return;

        if (enemiesSpawned < enemiesPerWave)
            return;

        if (enemiesAlive > 0)
            return;

        waveInProgress = false;

        Debug.Log($"WAVE {currentWave} COMPLETED");

        if (economyManager != null)
        {
            economyManager.AddCoins(coinsPerWave);

            Debug.Log(
                $"Wave {currentWave} reward: +{coinsPerWave} coins"
            );
        }

        if (currentWave >= totalWaves)
        {
            WinGame();
            return;
        }

    }

    private void UpdateHud()
    {
        if (healthText != null && baseHealth != null)
            healthText.text = $"HEALTH: {baseHealth.CurrentHealth}";

        if (waveText != null)
            waveText.text = $"WAVE {currentWave}/{totalWaves}";

        if (coinsText != null && economyManager != null)
            coinsText.text = $"COINS: {economyManager.CurrentCoins}";
    }

    private void CreateResultOverlays()
    {
        Canvas canvas = GameObject.Find("GameUI")?.GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("GameUI Canvas not found; result overlays cannot be created.");
            return;
        }

        victoryOverlay = CreateResultOverlay(
            canvas.transform,
            "VictoryOverlay",
            "VICTORY!",
            $"All {totalWaves} waves completed!",
            new Color(0.25f, 0.9f, 0.45f, 1f)
        );

        defeatOverlay = CreateResultOverlay(
            canvas.transform,
            "DefeatOverlay",
            "DEFEAT",
            "Your base has fallen.",
            new Color(1f, 0.3f, 0.25f, 1f)
        );

        victoryOverlay.SetActive(false);
        defeatOverlay.SetActive(false);
    }

    private GameObject CreateResultOverlay(
        Transform parent,
        string overlayName,
        string title,
        string message,
        Color accentColor
    )
    {
        GameObject overlay = new GameObject(overlayName);
        overlay.transform.SetParent(parent, false);

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("ResultPanel");
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 360f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.035f, 0.055f, 0.09f, 0.97f);

        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(8f, -8f);

        TMP_Text titleText = CreateResultText(
            panel.transform,
            "ResultTitle",
            title,
            64f,
            accentColor,
            new Vector2(0f, 55f),
            new Vector2(680f, 110f)
        );

        CreateResultText(
            panel.transform,
            "ResultMessage",
            message,
            30f,
            Color.white,
            new Vector2(0f, -55f),
            new Vector2(680f, 100f)
        );

        titleText.fontStyle = FontStyles.Bold;
        overlay.transform.SetAsLastSibling();
        return overlay;
    }

    private TMP_Text CreateResultText(
        Transform parent,
        string objectName,
        string text,
        float fontSize,
        Color color,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = size;
        textRect.anchoredPosition = position;

        TextMeshProUGUI resultText = textObject.AddComponent<TextMeshProUGUI>();
        resultText.text = text;
        resultText.fontSize = fontSize;
        resultText.color = color;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.enableWordWrapping = true;
        resultText.raycastTarget = false;
        return resultText;
    }

    private void ShowResultOverlay(bool victory)
    {
        if (victoryOverlay != null)
            victoryOverlay.SetActive(victory);

        if (defeatOverlay != null)
            defeatOverlay.SetActive(!victory);
    }

    private void WinGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Debug.Log("GAME WON!");
        ShowResultOverlay(true);
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public int CurrentWave => currentWave;

    public void LoseGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        CancelInvoke(nameof(SpawnEnemy));
        CancelInvoke(nameof(StartNextWave));

        Debug.Log("GAME LOST!");
        ShowResultOverlay(false);
    }
}
