using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private bool _isOpen          = false;
    private bool _unlockedByLever = false;
    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private Coroutine _moveCoroutine;

    private const float OpenOffset   = 0.5f;
    private const float AnimDuration = 0.3f;

    private void Awake()
    {
        _closedPosition = transform.position;
        _openPosition   = _closedPosition + Vector3.up * OpenOffset;
    }

    // Дверь не открывается напрямую игроком — только показывает подсказку.
    public void Interact(GameObject interactor) { }

    public string GetInteractionHint() =>
        _unlockedByLever ? null : "Activate the lever";

    public bool IsOpen => _isOpen;

    // Вызывается только рычагом.
    public void Toggle()
    {
        _unlockedByLever = true;
        _isOpen = !_isOpen;
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveTo(_isOpen ? _openPosition : _closedPosition));
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start   = transform.position;
        float   elapsed = 0f;

        while (elapsed < AnimDuration)
        {
            elapsed            += Time.deltaTime;
            transform.position  = Vector3.Lerp(start, target, elapsed / AnimDuration);
            yield return null;
        }

        transform.position = target;
    }
}
