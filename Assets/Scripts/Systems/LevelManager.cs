using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadLevel(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private const string UnlockedLevelKey = "UnlockedLevel";

    // Уровень доступен, если его buildIndex не превышает наибольший открытый.
    public bool IsLevelUnlocked(int buildIndex)
    {
        int unlocked = PlayerPrefs.GetInt(UnlockedLevelKey, 1);
        return buildIndex <= unlocked;
    }

    // Открывает следующий уровень после завершения текущего.
    public void CompleteLevel(int buildIndex)
    {
        int unlocked = PlayerPrefs.GetInt(UnlockedLevelKey, 1);
        if (buildIndex + 1 > unlocked)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, buildIndex + 1);
            PlayerPrefs.Save();
        }
    }
}
