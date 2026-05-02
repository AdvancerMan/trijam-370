using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MousePlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainPerSecond = 5f;
    [SerializeField] private float staminaRestorePerPotato = 20f;
    [SerializeField] private TMP_Text staminaText;

    [Header("World references")]
    [SerializeField] private WorldManager worldManager;

    [Header("World bounds (visible game area)")]
    [SerializeField] private float worldMinX = -8f;
    [SerializeField] private float worldMaxX = 8f;
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
        }

        float worldMinY = worldManager.WorldMinY;
        float dirtTopY = worldManager.DirtTopY;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, worldMinX + playerRadius, worldMaxX - playerRadius);
        clampedPos.y = Mathf.Clamp(clampedPos.y, worldMinY + playerRadius, dirtTopY - playerRadius);
        transform.position = clampedPos;
    }

    private void CenterCameraOnPlayer()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.y;
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
