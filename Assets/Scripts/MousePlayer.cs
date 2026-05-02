using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MousePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookRotationOffset = 0f;
    [SerializeField] private float lookRotationLerpSpeed = 12f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainPerSecond = 5f;
    [SerializeField] private float staminaRestorePerPotato = 20f;
    [SerializeField] private float damageCooldownSeconds = 1f;

    [Header("Damage visual")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip eatPotatoSound;

    [Header("World references")]
    [SerializeField] private WorldManager worldManager;

    [Header("World bounds (visible game area)")]
    [SerializeField] private float playerRadius = 0.5f;

    private Camera mainCamera;
    private float currentStamina;
    private float nextDamageAllowedTime;
    private float damageFlashTimer;
    private Color baseSpriteColor = Color.white;
    private bool gameOverTriggered;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercent => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        currentStamina = maxStamina;
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (playerSpriteRenderer != null)
        {
            baseSpriteColor = playerSpriteRenderer.color;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (worldManager == null)
        {
            worldManager = FindAnyObjectByType<WorldManager>();
        }
    }

    private void Update()
    {
        DrainStamina();
        UpdateDamageVisual();

        if (currentStamina <= 0f)
        {
            TriggerGameOver();
            return;
        }

        if (mainCamera == null)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (worldManager == null)
        {
            return;
        }

        MoveTowardsPointer();
        CenterCameraOnPlayer();
    }

    private void DrainStamina()
    {
        if (staminaDrainPerSecond <= 0f)
        {
            return;
        }

        currentStamina = Mathf.Max(0f, currentStamina - staminaDrainPerSecond * Time.deltaTime);
    }

    private void RestoreStamina()
    {
        if (staminaRestorePerPotato <= 0f)
        {
            return;
        }

        currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRestorePerPotato);
    }

    public bool TryTakeDamage(float staminaDamage)
    {
        if (Time.time < nextDamageAllowedTime)
        {
            return false;
        }

        if (staminaDamage <= 0f)
        {
            return false;
        }

        currentStamina = Mathf.Max(0f, currentStamina - staminaDamage);
        nextDamageAllowedTime = Time.time + damageCooldownSeconds;
        damageFlashTimer = damageFlashDuration;
        PlaySound(damageSound);
        return true;
    }

    private void UpdateDamageVisual()
    {
        if (playerSpriteRenderer == null)
        {
            return;
        }

        if (damageFlashTimer <= 0f || damageFlashDuration <= 0f)
        {
            playerSpriteRenderer.color = baseSpriteColor;
            return;
        }

        damageFlashTimer = Mathf.Max(0f, damageFlashTimer - Time.deltaTime);
        float progress = 1f - (damageFlashTimer / damageFlashDuration);
        float intensity = Mathf.Sin(progress * Mathf.PI);
        playerSpriteRenderer.color = Color.Lerp(baseSpriteColor, damageColor, intensity);
    }

    private void MoveTowardsPointer()
    {
        Vector3 pointerScreenPos = Mouse.current.position.ReadValue();
        pointerScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 pointerWorldPos = mainCamera.ScreenToWorldPoint(pointerScreenPos);

        Vector2 toPointer = (pointerWorldPos - transform.position);
        if (toPointer.sqrMagnitude > 0.0001f)
        {
            Vector2 direction = toPointer.normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            RotateTowards(direction);
        }

        float worldMinY = worldManager.WorldMinY;
        float dirtTopY = worldManager.DirtTopY;
        float worldMinX = worldManager.WorldMinX;
        float worldMaxX = worldManager.WorldMaxX;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, worldMinX + playerRadius, worldMaxX - playerRadius);
        clampedPos.y = Mathf.Clamp(clampedPos.y, worldMinY + playerRadius, dirtTopY - playerRadius);
        transform.position = clampedPos;
    }

    private void RotateTowards(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + lookRotationOffset;
        float currentAngle = transform.eulerAngles.z;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, lookRotationLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothedAngle);
    }

    private void CenterCameraOnPlayer()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = transform.position.x;
        mainCamera.transform.position = cameraPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectPotato(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollectPotato(collision.gameObject);
    }

    private void TryCollectPotato(GameObject otherObject)
    {
        if (otherObject == null || worldManager == null)
        {
            return;
        }

        Potato potato = otherObject.GetComponent<Potato>();
        if (potato == null)
        {
            return;
        }

        if (!potato.TryMarkCollected())
        {
            return;
        }

        RestoreStamina();
        PlaySound(eatPotatoSound);
        worldManager.CollectPotato(otherObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private void TriggerGameOver()
    {
        if (gameOverTriggered)
        {
            return;
        }

        gameOverTriggered = true;

        if (worldManager != null)
        {
            worldManager.TriggerGameOver();
            return;
        }
    }
}
