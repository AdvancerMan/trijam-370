using UnityEngine;

public class Mole : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float targetRefreshIntervalSeconds = 0.25f;
    [SerializeField] private float lookRotationOffset = 0f;
    [SerializeField] private float lookRotationLerpSpeed = 12f;

    private Potato currentTarget;
    private float targetRefreshTimer;

    private void Update()
    {
        targetRefreshTimer += Time.deltaTime;
        if (currentTarget == null || targetRefreshTimer >= targetRefreshIntervalSeconds)
        {
            targetRefreshTimer = 0f;
            currentTarget = FindNearestPotato();
        }

        if (currentTarget == null)
        {
            return;
        }

        MoveTowardsTarget(currentTarget.transform.position);
    }

    private Potato FindNearestPotato()
    {
        Potato[] potatoes = FindObjectsByType<Potato>(FindObjectsInactive.Exclude);
        Potato nearest = null;
        float nearestDistanceSq = float.MaxValue;

        Vector3 molePosition = transform.position;
        for (int i = 0; i < potatoes.Length; i++)
        {
            Potato potato = potatoes[i];
            if (potato == null)
            {
                continue;
            }

            Vector3 delta = potato.transform.position - molePosition;
            float distanceSq = delta.sqrMagnitude;
            if (distanceSq < nearestDistanceSq)
            {
                nearestDistanceSq = distanceSq;
                nearest = potato;
            }
        }

        return nearest;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDestroyPotato(collision.gameObject);
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
}
