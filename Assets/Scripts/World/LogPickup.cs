using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class LogPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private LogEntry logEntry;

    public LogEntry LogEntry => logEntry;

    private SpriteRenderer _sprite;
    private bool _collected;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void Interact(GameObject interactor)
    {
        if (_collected || logEntry == null) return;

        _collected = true;
        logEntry.isFound = true;

        if (_sprite != null)
            _sprite.color = Color.gray;

        GameManager.Instance.CollectLog(logEntry);
    }

    public string GetInteractionHint() => _collected ? null : "Read note";
}
