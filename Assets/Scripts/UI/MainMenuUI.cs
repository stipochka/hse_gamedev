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

    [Header("Visuals")]
    [SerializeField] private string gameTitle = "WASTELAND";
    [SerializeField] private Texture2D backgroundImage;

    private bool _showLevelSelect;

    private GUIStyle _buttonStyle;
    private GUIStyle _lockedButtonStyle;
    private GUIStyle _titleStyle;

    private void BuildStyles()
    {
        if (_buttonStyle != null) return;

        var normalTex  = MakeTex(new Color(0.18f, 0.18f, 0.18f, 0.9f));
        var hoverTex   = MakeTex(new Color(0.32f, 0.32f, 0.32f, 0.95f));
        var activeTex  = MakeTex(new Color(0.45f, 0.18f, 0.10f, 1f));
        var lockedTex  = MakeTex(new Color(0.12f, 0.12f, 0.12f, 0.6f));

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            margin    = new RectOffset(0, 0, 0, 0),
            border    = new RectOffset(2, 2, 2, 2)
        };
        _buttonStyle.normal.background  = normalTex;
        _buttonStyle.normal.textColor   = new Color(0.9f, 0.85f, 0.7f);
        _buttonStyle.hover.background   = hoverTex;
        _buttonStyle.hover.textColor    = Color.white;
        _buttonStyle.active.background  = activeTex;
        _buttonStyle.active.textColor   = Color.white;
        _buttonStyle.focused.background = normalTex;

        _lockedButtonStyle = new GUIStyle(_buttonStyle);
        _lockedButtonStyle.normal.background = lockedTex;
        _lockedButtonStyle.normal.textColor  = new Color(0.5f, 0.5f, 0.5f);
        _lockedButtonStyle.hover.background  = lockedTex;
        _lockedButtonStyle.hover.textColor   = new Color(0.5f, 0.5f, 0.5f);

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = new Color(0.85f, 0.75f, 0.55f);
    }

    private static Texture2D MakeTex(Color color)
    {
        var tex = new Texture2D(2, 2);
        var pixels = new Color[4] { color, color, color, color };
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        BuildStyles();
        DrawBackground();

        if (_showLevelSelect)
            DrawLevelSelect();
        else
            DrawMainMenu();
    }

    private void DrawBackground()
    {
        if (backgroundImage != null)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundImage, ScaleMode.ScaleAndCrop);
        else
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
    }

    private void DrawMainMenu()
    {
        float w = Screen.width;
        float bw = 260f, bh = 50f, gap = 16f;
        float startY = Screen.height * 0.5f - bh;

        GUI.Label(new Rect(0, startY - 120f, w, 80f), gameTitle, _titleStyle);

        var playRect = new Rect((w - bw) * 0.5f, startY, bw, bh);
        if (GUI.Button(playRect, "Play", _buttonStyle))
            _showLevelSelect = true;

        var quitRect = new Rect((w - bw) * 0.5f, startY + bh + gap, bw, bh);
        if (GUI.Button(quitRect, "Quit", _buttonStyle))
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
        float bw = 260f, bh = 50f, gap = 16f;
        float totalHeight = levels.Length * (bh + gap) + bh;
        float startY = (Screen.height - totalHeight) * 0.5f;

        for (int i = 0; i < levels.Length; i++)
        {
            var rect = new Rect((w - bw) * 0.5f, startY + i * (bh + gap), bw, bh);

            bool unlocked = LevelManager.Instance == null || LevelManager.Instance.IsLevelUnlocked(levels[i].buildIndex);
            string label = unlocked ? levels[i].displayName : $"{levels[i].displayName}  🔒";
            var style = unlocked ? _buttonStyle : _lockedButtonStyle;

            GUI.enabled = unlocked;
            if (GUI.Button(rect, label, style) && unlocked)
            {
                if (LevelManager.Instance != null)
                    LevelManager.Instance.LoadLevel(levels[i].buildIndex);
                else
                    Debug.LogError("[MainMenuUI] LevelManager.Instance не найден — добавь GameObject с LevelManager в сцену.");
            }
            GUI.enabled = true;
        }

        var backRect = new Rect((w - bw) * 0.5f, startY + levels.Length * (bh + gap) + gap, bw, bh);
        if (GUI.Button(backRect, "Back", _buttonStyle))
            _showLevelSelect = false;
    }
}
