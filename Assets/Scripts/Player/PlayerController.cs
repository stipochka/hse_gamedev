using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 7f;
    [SerializeField] public float crouchSpeedMultiplier = 0.5f;
    [SerializeField] public float jumpForce = 14f;
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2.5f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Level Exit")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Level Bounds")]
    [SerializeField] private bool useLevelBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float deathY = -10f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private float footstepInterval = 0.35f;
    [SerializeField] private float crouchFootstepInterval = 0.55f;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _collider;
    private PlayerHealth _health;
    private AudioSource _audioSource;

    private float _footstepTimer;

    private Vector2 _colliderDefaultSize;
    private Vector2 _colliderDefaultOffset;

    private bool _isGrounded;
    private bool _isCrouching;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;

    private IInteractable _nearestInteractable;
    private string _interactionHint;
    private Vector3 _interactableWorldPos;

    private const float CoyoteTime = 0.15f;
    private const float JumpBufferTime = 0.1f;
    private const float CrouchHeightMultiplier = 0.5f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>();
        _health = GetComponent<PlayerHealth>();
        _audioSource = GetComponent<AudioSource>();

        _colliderDefaultSize = _collider.size;
        _colliderDefaultOffset = _collider.offset;
    }

    private void Update()
    {
        CheckGround();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleJump();
        HandleCrouch();
        HandleInteraction();
        HandleFootsteps();
    }

    private void FixedUpdate()
    {
        if (_rb.bodyType == RigidbodyType2D.Static) return;

        HandleMovement();
        ApplyQueuedJump();
        ClampToLevelBounds();
    }

    // Не даём игроку выйти за границы уровня по X, а при падении ниже deathY — убиваем.
    private void ClampToLevelBounds()
    {
        if (!useLevelBounds) return;

        Vector2 pos = _rb.position;
        float clampedX = Mathf.Clamp(pos.x, minX, maxX);

        if (!Mathf.Approximately(clampedX, pos.x))
        {
            _rb.position = new Vector2(clampedX, pos.y);
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }

        if (pos.y < deathY)
            _health?.Kill();
    }

    // Нижняя точка коллайдера — вычисляется из bounds, не требует ручного GroundCheck-объекта.
    private Vector2 GroundCheckOrigin =>
        (Vector2)transform.position + _collider.offset + Vector2.down * (_collider.size.y * 0.5f);

    private void CheckGround()
    {
        int mask = groundLayer.value != 0 ? groundLayer.value : ~0;
        _isGrounded = Physics2D.OverlapCircle(GroundCheckOrigin, groundCheckRadius, mask);
    }

    private void HandleCoyoteTime()
    {
        if (_isGrounded)
            _coyoteTimeCounter = CoyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;
    }

    // Флаг передаётся из Update в FixedUpdate, чтобы прыжок не перебивал HandleMovement
    private bool _jumpQueued;
    private bool _jumpCutQueued;

    private void HandleJumpBuffer()
    {
        bool jumpPressed = Input.GetButtonDown("Jump")
                        || Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.W);
        if (jumpPressed)
            _jumpBufferCounter = JumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _jumpQueued = true;
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;

            if (jumpClip != null)
                _audioSource.PlayOneShot(jumpClip);
        }

        bool jumpReleased = Input.GetButtonUp("Jump")
                         || Input.GetKeyUp(KeyCode.Space)
                         || Input.GetKeyUp(KeyCode.W);
        if (jumpReleased && _rb.linearVelocity.y > 0f)
            _jumpCutQueued = true;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float speed = _isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        _rb.linearVelocity = new Vector2(horizontal * speed, _rb.linearVelocity.y);
    }

    private void ApplyQueuedJump()
    {
        if (_jumpQueued)
        {
            // Сбрасываем Y перед импульсом, чтобы jumpForce был точным при падении
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _jumpQueued = false;
        }

        if (_jumpCutQueued)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
            _jumpCutQueued = false;
        }
    }

    private void HandleInteraction()
    {
        int mask = interactLayer.value != 0 ? interactLayer.value : ~0;
        var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, mask);

        // OverlapCircleAll + фильтр: пропускаем собственные коллайдеры игрока,
        // чтобы Player на слое Interactable не блокировал детекцию других объектов.
        Collider2D best = null;
        foreach (var h in hits)
        {
            if (h.transform.IsChildOf(transform) || h.gameObject == gameObject) continue;
            if (h.GetComponentInParent<IInteractable>() != null) { best = h; break; }
        }

        _nearestInteractable  = best != null ? best.GetComponentInParent<IInteractable>() : null;
        _interactionHint      = _nearestInteractable?.GetInteractionHint();
        _interactableWorldPos = best != null ? best.transform.position : Vector3.zero;

        if (Input.GetKeyDown(KeyCode.E) && _nearestInteractable != null)
            _nearestInteractable.Interact(gameObject);

        // Выход с уровня: открытая дверь + Shift.
        if (best != null && best.GetComponentInParent<Door>() is Door door && door.IsOpen)
        {
            bool shiftPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            if (shiftPressed)
            {
                LevelManager.Instance?.CompleteLevel(SceneManager.GetActiveScene().buildIndex);
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }

    private void OnGUI()
    {
        if (_nearestInteractable == null || string.IsNullOrEmpty(_interactionHint)) return;
        if (Camera.main == null) return;

        // Переводим мировую позицию объекта в экранные координаты.
        // GUI.Label использует Y от верха экрана, Screen — от низа, поэтому инвертируем Y.
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_interactableWorldPos);
        if (screenPos.z < 0) return; // объект за камерой

        float w = 200f, h = 30f;
        float x = screenPos.x - w * 0.5f;
        float y = (Screen.height - screenPos.y) - h - 40f; // 40px над спрайтом

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        string text = $"[E] {_interactionHint}";

        style.normal.textColor = Color.black;
        GUI.Label(new Rect(x + 1, y + 1, w, h), text, style);
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(x,     y,     w, h), text, style);
    }

    private void HandleCrouch()
    {
        bool crouching = Input.GetAxisRaw("Vertical") < -0.5f;
        _isCrouching = crouching;

        if (crouching)
        {
            // Меняем только высоту, offset не трогаем: сдвиг offset вниз создавал
            // мгновенный оверлап с полом и физика выталкивала тело вверх.
            _collider.size = new Vector2(_colliderDefaultSize.x, _colliderDefaultSize.y * CrouchHeightMultiplier);
        }
        else if (!IsCeilingAbove())
        {
            _collider.size = _colliderDefaultSize;
        }
    }

    // Проигрывает шаги, пока игрок стоит на земле и движется горизонтально.
    private void HandleFootsteps()
    {
        bool moving = _isGrounded && Mathf.Abs(_rb.linearVelocity.x) > 0.1f;

        if (!moving)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f)
        {
            if (footstepClip != null)
                _audioSource.PlayOneShot(footstepClip);

            _footstepTimer = _isCrouching ? crouchFootstepInterval : footstepInterval;
        }
    }

    // Не даём встать, если над головой потолок
    private bool IsCeilingAbove()
    {
        Vector2 topCenter = (Vector2)transform.position + _colliderDefaultOffset +
                            Vector2.up * (_colliderDefaultSize.y * 0.5f);
        return Physics2D.OverlapCircle(topCenter, groundCheckRadius, groundLayer);
    }

    private void OnValidate()
    {
        if (groundLayer.value == 0)
            Debug.LogWarning("[PlayerController] groundLayer не назначен — использую все слои как фолбэк.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (_collider == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GroundCheckOrigin, groundCheckRadius);

        Gizmos.color = Color.yellow;
        Vector2 topCenter = (Vector2)transform.position + _collider.offset +
                            Vector2.up * (_collider.size.y * 0.5f);
        Gizmos.DrawWireSphere(topCenter, groundCheckRadius);

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        if (useLevelBounds)
        {
            Gizmos.color = Color.magenta;
            Vector3 top = transform.position + Vector3.up * 50f;
            Vector3 bottom = transform.position + Vector3.down * 50f;
            Gizmos.DrawLine(new Vector3(minX, top.y, 0f), new Vector3(minX, bottom.y, 0f));
            Gizmos.DrawLine(new Vector3(maxX, top.y, 0f), new Vector3(maxX, bottom.y, 0f));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(minX - 5f, deathY, 0f), new Vector3(maxX + 5f, deathY, 0f));
        }
    }
}
