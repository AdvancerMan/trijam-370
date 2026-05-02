using UnityEngine;

public class Potato : MonoBehaviour
{
    public bool IsCollected { get; private set; }

    public bool TryMarkCollected()
    {
        if (IsCollected)
        {
            return false;
        }

        IsCollected = true;
        return true;
    }
}
