using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [System.Serializable]
    public class LevelEntry
    {
        public string displayName;
        public string sceneName;
    }

    [SerializeField] private string gameTitle = "WASTELAND";
    [SerializeField] private LevelEntry[] levels;

    private void OnGUI()
    {
        float w = Screen.width;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 60, w, 60), gameTitle, titleStyle);

        var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };

        float bw = 240f, bh = 44f, gap = 12f;
        float startY = 160f;

        for (int i = 0; i < levels.Length; i++)
        {
            var rect = new Rect((w - bw) * 0.5f, startY + i * (bh + gap), bw, bh);
            if (GUI.Button(rect, levels[i].displayName, buttonStyle))
                SceneManager.LoadScene(levels[i].sceneName);
        }

        var quitRect = new Rect((w - bw) * 0.5f, startY + levels.Length * (bh + gap) + gap, bw, bh);
        if (GUI.Button(quitRect, "Quit", buttonStyle))
            Application.Quit();
    }
}
