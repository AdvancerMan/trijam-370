using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WorldManager : MonoBehaviour
{
    [Header("Potato spawning")]
    [SerializeField] private GameObject potatoPrefab;
    [SerializeField] private float spawnIntervalSeconds = 5f;
    [SerializeField] private int maxPotatoCount = 20;

    [Header("World bounds")]
    [SerializeField] private float worldMinX = -8f;
    [SerializeField] private float worldMaxX = 8f;
    [SerializeField] private float worldMinY = -5f;
    [SerializeField] private float worldMaxY = 5f;
    [SerializeField] [Range(0f, 1f)] private float dirtHeightPercent = 0.7f;

    [Header("Spawn depth")]
    [SerializeField] private float spawnZ = 0f;

    [Header("UI")]
    [SerializeField] private TMP_Text potatoCounterText;
    [SerializeField] private GameObject uiObject;

    private Camera mainCamera;
    private float spawnTimer;
    private float elapsedTimeSeconds;
    private int collectedPotatoCount;
    private string gameplaySceneName = string.Empty;
    private bool gameOverTriggered;

    public float WorldMinX => worldMinX;
    public float WorldMaxX => worldMaxX;
    public float WorldMinY => worldMinY;
    public float WorldMaxY => worldMaxY;
    public float DirtHeightPercent => dirtHeightPercent;
    public float DirtTopY => worldMinY + (worldMaxY - worldMinY) * dirtHeightPercent;
    public int CollectedPotatoCount => collectedPotatoCount;
    public float ElapsedTimeSeconds => elapsedTimeSeconds;

    private void Awake()
    {
        mainCamera = Camera.main;
        gameplaySceneName = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        elapsedTimeSeconds += Time.deltaTime;
        UpdateUi();

        if (potatoPrefab == null || mainCamera == null)
        {
            return;
        }

        if (spawnIntervalSeconds <= 0f)
        {
            return;
        }

        if (transform.childCount >= maxPotatoCount)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnIntervalSeconds)
        {
            spawnTimer = 0f;
            SpawnPotato();
        }
    }

    private void SpawnPotato()
    {
        float minX;
        float maxX;

        if (mainCamera.orthographic)
        {
            float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;
            minX = mainCamera.transform.position.x - halfWidth;
            maxX = mainCamera.transform.position.x + halfWidth;
        }
        else
        {
            Vector3 left = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, Mathf.Abs(mainCamera.transform.position.z - spawnZ)));
            Vector3 right = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(mainCamera.transform.position.z - spawnZ)));
            minX = left.x;
            maxX = right.x;
        }

        GameObject potatoInstance = Instantiate(potatoPrefab, new Vector3(0f, 0f, spawnZ), Quaternion.identity, transform);
        Vector2 halfExtents = GetPotatoHalfExtents(potatoInstance);

        float spawnMinX = Mathf.Max(minX, worldMinX) + halfExtents.x;
        float spawnMaxX = Mathf.Min(maxX, worldMaxX) - halfExtents.x;
        float spawnMinY = worldMinY + halfExtents.y;
        float spawnMaxY = DirtTopY - halfExtents.y;

        float randomX = GetRandomInRangeOrMidpoint(spawnMinX, spawnMaxX);
        float randomY = GetRandomInRangeOrMidpoint(spawnMinY, spawnMaxY);
        potatoInstance.transform.position = new Vector3(randomX, randomY, spawnZ);
    }

    private static Vector2 GetPotatoHalfExtents(GameObject potatoInstance)
    {
        Collider2D collider2D = potatoInstance.GetComponentInChildren<Collider2D>();
        if (collider2D != null)
        {
            return collider2D.bounds.extents;
        }

        Renderer renderer = potatoInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.extents;
        }

        return Vector2.zero;
    }

    private static float GetRandomInRangeOrMidpoint(float min, float max)
    {
        if (min <= max)
        {
            return Random.Range(min, max);
        }

        return (min + max) * 0.5f;
    }

    public void CollectPotato(GameObject potato)
    {
        if (potato == null)
        {
            return;
        }

        collectedPotatoCount += 1;
        Destroy(potato);
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered)
        {
            return;
        }

        gameOverTriggered = true;
        GameSessionData.CollectedPotatoes = collectedPotatoCount;
        GameSessionData.TimeSpentSeconds = elapsedTimeSeconds;
        GameSessionData.GameplaySceneName = gameplaySceneName;
        SceneManager.LoadScene("GameOver");
    }

    private void UpdateUi()
    {
        if (potatoCounterText != null)
        {
            potatoCounterText.text = collectedPotatoCount.ToString();
        }

        if (uiObject == null || mainCamera == null)
        {
            return;
        }

        Transform uiTransform = uiObject.transform;
        float distanceToCamera = Mathf.Abs(mainCamera.transform.position.z - uiTransform.position.z);
        Vector3 topRightWorld = mainCamera.ViewportToWorldPoint(new Vector3(0.9f, 0.9f, distanceToCamera));
        uiTransform.position = topRightWorld;
    }
}
