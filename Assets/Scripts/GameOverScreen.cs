using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text collectedPotatoesText;
    [SerializeField] private TMP_Text timeSpentText;
    [SerializeField] private string fallbackGameplaySceneName = "SampleScene";

    private void Start()
    {
        if (collectedPotatoesText != null)
        {
            collectedPotatoesText.text = $"Collected potatos: {GameSessionData.CollectedPotatoes}";
        }

        if (timeSpentText != null)
        {
            timeSpentText.text = $"Time spent: {GameSessionData.TimeSpentSeconds:F1}s";
        }
    }

    public void RestartGame()
    {
        string sceneToLoad = GameSessionData.GameplaySceneName;
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = fallbackGameplaySceneName;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
