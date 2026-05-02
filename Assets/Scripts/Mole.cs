using UnityEngine;

public class Mole : MonoBehaviour
{
    private enum TargetStrategy
    {
        NEAREST_TO_MOLE,
        NEAREST_TO_PLAYER,
        FURTHEST_FROM_PLAYER
    }

    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float speedIncreaseDelay = 5f;
    [SerializeField] private float speedIncreaseDelta = 0f;
    [SerializeField] private float targetRefreshIntervalSeconds = 0.25f;
    [SerializeField] private float lookRotationOffset = 0f;
    [SerializeField] private float lookRotationLerpSpeed = 12f;
    [SerializeField] private float staminaDamageToPlayer = 15f;
    [SerializeField] private TargetStrategy strategy = TargetStrategy.NEAREST_TO_MOLE;
    [SerializeField] private MousePlayer player;

    private Potato currentTarget;
    private float targetRefreshTimer;
    private float speedIncreaseTimer;

    private void Awake()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<MousePlayer>();
        }
    }

    private void Update()
    {
        UpdateMoveSpeedGrowth();

        targetRefreshTimer += Time.deltaTime;
        if (currentTarget == null || targetRefreshTimer >= targetRefreshIntervalSeconds)
        {
            targetRefreshTimer = 0f;
            currentTarget = FindTargetPotato();
        }

        if (currentTarget == null)
        {
            return;
        }

        MoveTowardsTarget(currentTarget.transform.position);
    }

    private void UpdateMoveSpeedGrowth()
    {
        if (speedIncreaseDelay <= 0f || speedIncreaseDelta <= 0f)
        {
            return;
        }

        speedIncreaseTimer += Time.deltaTime;
        if (speedIncreaseTimer >= speedIncreaseDelay)
        {
            speedIncreaseTimer = 0f;
            moveSpeed += speedIncreaseDelta;
        }
    }

    private Potato FindTargetPotato()
    {
        Potato[] potatoes = FindObjectsByType<Potato>(FindObjectsInactive.Exclude);

        switch (strategy)
        {
            case TargetStrategy.NEAREST_TO_PLAYER:
                return SelectPotatoByDistance(potatoes, GetPlayerPositionOrFallback(), true);
            case TargetStrategy.FURTHEST_FROM_PLAYER:
                return SelectPotatoByDistance(potatoes, GetPlayerPositionOrFallback(), false);
            case TargetStrategy.NEAREST_TO_MOLE:
            default:
                return SelectPotatoByDistance(potatoes, transform.position, true);
        }
    }

    private Vector3 GetPlayerPositionOrFallback()
    {
        if (player != null)
        {
            return player.transform.position;
        }

        return transform.position;
    }

    private static Potato SelectPotatoByDistance(Potato[] potatoes, Vector3 referencePosition, bool nearest)
    {
        Potato selected = null;
        float selectedDistanceSq = nearest ? float.MaxValue : float.MinValue;

        for (int i = 0; i < potatoes.Length; i++)
        {
            Potato potato = potatoes[i];
            if (potato == null)
            {
                continue;
            }

            Vector3 delta = potato.transform.position - referencePosition;
            float distanceSq = delta.sqrMagnitude;

            bool isBetterMatch = nearest ? distanceSq < selectedDistanceSq : distanceSq > selectedDistanceSq;
            if (isBetterMatch)
            {
                selectedDistanceSq = distanceSq;
                selected = potato;
            }
        }

        return selected;
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector3 previousPosition = transform.position;
        Vector3 nextPosition = Vector3.MoveTowards(
            previousPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Keep current Z so the mole stays on its gameplay plane.
        nextPosition.z = transform.position.z;
        transform.position = nextPosition;

        Vector2 moveDirection = nextPosition - previousPosition;
        if (moveDirection.sqrMagnitude > 0.000001f)
        {
            RotateTowards(moveDirection.normalized);
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + lookRotationOffset;
        float currentAngle = transform.eulerAngles.z;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, lookRotationLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothedAngle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDestroyPotato(other.gameObject);
        TryDamagePlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDestroyPotato(collision.gameObject);
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDestroyPotato(GameObject otherObject)
    {
        if (otherObject == null)
        {
            return;
        }

        Potato potato = otherObject.GetComponent<Potato>();
        if (potato == null)
        {
            return;
        }

        Destroy(otherObject);

        if (currentTarget != null && currentTarget.gameObject == otherObject)
        {
            currentTarget = null;
        }
    }

    private void TryDamagePlayer(GameObject otherObject)
    {
        if (otherObject == null)
        {
            return;
        }

        MousePlayer hitPlayer = otherObject.GetComponentInParent<MousePlayer>();
        if (hitPlayer == null)
        {
            return;
        }

        hitPlayer.TryTakeDamage(staminaDamageToPlayer);
    }
}
