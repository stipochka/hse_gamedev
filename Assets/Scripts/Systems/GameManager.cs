using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<LogEntry> allLogs = new();

    private readonly List<LogEntry> _collectedLogs = new();

    private LogEntry _displayedLog;
    private float    _popupTimer;
    private const float PopupDuration = 5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in allLogs)
            if (entry != null) entry.isFound = false;
    }

    private void Update()
    {
        if (_popupTimer > 0f)
        {
            _popupTimer -= Time.deltaTime;
            if (_popupTimer <= 0f)
                _displayedLog = null;
        }
    }

    public void CollectLog(LogEntry entry)
    {
        if (entry == null || _collectedLogs.Contains(entry)) return;
        _collectedLogs.Add(entry);

        _displayedLog = entry;
        _popupTimer   = PopupDuration;
    }

    public int GetCollectedCount() => _collectedLogs.Count;
    public int GetTotalCount()     => allLogs.Count;

    private void OnGUI()
    {
        DrawCounter();

        if (_displayedLog != null)
            DrawPopup(_displayedLog);
    }

    private void DrawCounter()
    {
        if (allLogs.Count == 0) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperRight
        };

        string text = $"Notes: {_collectedLogs.Count}/{allLogs.Count}";
        float w = 160f, h = 30f;
        float x = Screen.width - w - 12f;
        float y = 12f;

        style.normal.textColor = Color.black;
        GUI.Label(new Rect(x + 1, y + 1, w, h), text, style);
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y, w, h), text, style);
    }

    private void DrawPopup(LogEntry entry)
    {
        float pw = 480f, ph = 90f;
        float px = (Screen.width - pw) * 0.5f;
        float py = 12f;

        // Полупрозрачная плашка сверху, не перекрывает игровую сцену
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float pad = 12f;

        // Заголовок
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
            wordWrap  = true
        };
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
        GUI.Label(new Rect(px + pad, py + pad, pw - pad * 2, 22f), entry.title, titleStyle);

        // Контент
        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            wordWrap  = true,
            alignment = TextAnchor.UpperLeft
        };
        bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        GUI.Label(new Rect(px + pad, py + pad + 24f, pw - pad * 2, ph - pad * 2 - 24f), entry.content, bodyStyle);

        // Закрытие по E
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.E)
        {
            _displayedLog = null;
            _popupTimer   = 0f;
            Event.current.Use();
        }
    }
}
