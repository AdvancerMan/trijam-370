using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    [SerializeField] private TMP_Text staminaText;

    [Header("World references")]
    [SerializeField] private WorldManager worldManager;

    [Header("World bounds (visible game area)")]
    [SerializeField] private float playerRadius = 0.5f;

    private Camera mainCamera;
    private float currentStamina;

    private void Awake()
    {
        mainCamera = Camera.main;
        currentStamina = maxStamina;

        if (worldManager == null)
        {
            worldManager = FindAnyObjectByType<WorldManager>();
        }
    }

    private void Update()
    {
        DrainStamina();
        UpdateStaminaText();

        if (currentStamina <= 0f)
        {
            Destroy(gameObject);
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

    private void UpdateStaminaText()
    {
        if (staminaText == null)
        {
            return;
        }

        staminaText.text = Mathf.CeilToInt(currentStamina).ToString();
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

        Vector3 clampedPos = transform.position;
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
        worldManager.CollectPotato(otherObject);
    }
}
