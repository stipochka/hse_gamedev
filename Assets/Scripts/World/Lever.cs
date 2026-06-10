using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Lever : MonoBehaviour, IInteractable
{
    public bool isActivated = false;

    [Tooltip("Перетащи сюда компонент Door — рычаг переключает её напрямую.")]
    public Door linkedDoor;

    private SpriteRenderer _sprite;

    private static readonly Color ColorOff = Color.gray;
    private static readonly Color ColorOn  = Color.green;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        RefreshVisual();
    }

    public void Interact(GameObject interactor)
    {
        isActivated = !isActivated;
        RefreshVisual();
        linkedDoor?.Toggle();
    }

    public string GetInteractionHint() => "Use lever";

    private void RefreshVisual()
    {
        if (_sprite != null)
            _sprite.color = isActivated ? ColorOn : ColorOff;
    }
}
