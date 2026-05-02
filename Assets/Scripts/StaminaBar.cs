using UnityEngine;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private MousePlayer player;
    [SerializeField] private float scaleLerpSpeed = 10f;
    private float fullWidthAtScaleOne = 1f;

    private Vector3 initialScale;
    private Vector3 initialLocalPosition;

    private void Awake()
    {
        initialScale = transform.localScale;
        initialLocalPosition = transform.localPosition;
        TryCacheWidthFromRenderer();

        if (player == null)
        {
            player = FindAnyObjectByType<MousePlayer>();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float targetPercent = Mathf.Clamp01(player.StaminaPercent);
        Vector3 currentScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x * targetPercent, initialScale.y, initialScale.z);

        transform.localScale = Vector3.Lerp(currentScale, targetScale, scaleLerpSpeed * Time.deltaTime);
        KeepRightEdgeFixed();
    }

    private void KeepRightEdgeFixed()
    {
        float scaleDeltaX = initialScale.x - transform.localScale.x;
        float localOffsetX = scaleDeltaX * fullWidthAtScaleOne * 0.5f;

        Vector3 anchoredPosition = initialLocalPosition;
        anchoredPosition.x += localOffsetX;
        transform.localPosition = anchoredPosition;
    }

    private void TryCacheWidthFromRenderer()
    {
        Renderer visual = GetComponentInChildren<Renderer>();
        if (visual == null)
        {
            return;
        }

        float lossyScaleX = Mathf.Abs(transform.lossyScale.x);
        if (lossyScaleX <= 0.0001f)
        {
            return;
        }

        fullWidthAtScaleOne = visual.bounds.size.x / lossyScaleX;
    }
}
