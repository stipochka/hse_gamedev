using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelEntry
    {
        public string displayName;
        public int buildIndex;
    }

    [SerializeField] private LevelEntry[] levels =
    {
        new LevelEntry { displayName = "Level 1", buildIndex = 1 }
    };

    private bool _showLevelSelect;

    private void OnGUI()
    {
        if (_showLevelSelect)
            DrawLevelSelect();
        else
            DrawMainMenu();
    }

    private void DrawMainMenu()
    {
        float w = Screen.width;
        float bw = 240f, bh = 44f, gap = 12f;
        float startY = Screen.height * 0.5f - bh;

        var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };

        var playRect = new Rect((w - bw) * 0.5f, startY, bw, bh);
        if (GUI.Button(playRect, "Play", buttonStyle))
            _showLevelSelect = true;

        var quitRect = new Rect((w - bw) * 0.5f, startY + bh + gap, bw, bh);
        if (GUI.Button(quitRect, "Quit", buttonStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void DrawLevelSelect()
    {
        float w = Screen.width;
        float bw = 240f, bh = 44f, gap = 12f;
        float totalHeight = levels.Length * (bh + gap) + bh;
        float startY = (Screen.height - totalHeight) * 0.5f;

        var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };

        for (int i = 0; i < levels.Length; i++)
        {
            var rect = new Rect((w - bw) * 0.5f, startY + i * (bh + gap), bw, bh);

            bool unlocked = LevelManager.Instance == null || LevelManager.Instance.IsLevelUnlocked(levels[i].buildIndex);
            string label = unlocked ? levels[i].displayName : $"{levels[i].displayName} 🔒";

            GUI.enabled = unlocked;
            if (GUI.Button(rect, label, buttonStyle) && unlocked)
            {
                if (LevelManager.Instance != null)
                    LevelManager.Instance.LoadLevel(levels[i].buildIndex);
                else
                    Debug.LogError("[MainMenuUI] LevelManager.Instance не найден — добавь GameObject с LevelManager в сцену.");
            }
            GUI.enabled = true;
        }

        var backRect = new Rect((w - bw) * 0.5f, startY + levels.Length * (bh + gap) + gap, bw, bh);
        if (GUI.Button(backRect, "Back", buttonStyle))
            _showLevelSelect = false;
    }
}
