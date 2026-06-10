using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NoiseHunterEnemy : MonoBehaviour
{
    public enum State { Idle, Chase, Return }

    [Header("Detection")]
    public Transform player;
    public NoiseEmitter playerNoise;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float returnSpeed = 1.5f;

    [Header("Bounds")]
    public Transform boundaryLeft;
    public Transform boundaryRight;

    [Header("Attack")]
    public float attackRange = 1.2f;

    private Rigidbody2D _rb;
    private PlayerHealth _playerHealth;
    private State _currentState;
    private Vector2 _startPosition;
    private Vector3 _originalScale;

    private const float ReachThreshold = 0.1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalScale = transform.localScale;
    }

    private void Start()
    {
        if (player != null)
            _playerHealth = player.GetComponentInParent<PlayerHealth>();

        _startPosition = _rb.position;
        EnterState(State.Idle);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Chase:  UpdateChase();  break;
            case State.Return: UpdateReturn(); break;
        }
    }

    // ── Idle ──────────────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        if (PlayerHeard())
            EnterState(State.Chase);
    }

    // ── Chase ─────────────────────────────────────────────────────────────────

    private void UpdateChase()
    {
        // Сам враг достиг границы — не выходим за неё.
        if (!IsWithinBounds(_rb.position))
        {
            EnterState(State.Return);
            return;
        }

        MoveTowards(player.position.x, chaseSpeed);
        FaceTowards(player.position.x);

        if (Vector2.Distance(transform.position, player.position) < attackRange)
            _playerHealth?.TakeDamage(1);

        // Игрок убежал за границу патруля — прекращаем погоню.
        if (!IsWithinBounds(player.position))
            EnterState(State.Return);
    }

    // ── Return ────────────────────────────────────────────────────────────────

    private void UpdateReturn()
    {
        MoveTowards(_startPosition.x, returnSpeed);
        FaceTowards(_startPosition.x);

        if (Mathf.Abs(_rb.position.x - _startPosition.x) <= ReachThreshold)
            EnterState(State.Idle);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void MoveTowards(float targetX, float speed)
    {
        float dx = targetX - _rb.position.x;
        float vx = Mathf.Abs(dx) > ReachThreshold ? Mathf.Sign(dx) * speed : 0f;

        if (boundaryLeft != null && boundaryRight != null)
        {
            float minX = Mathf.Min(boundaryLeft.position.x, boundaryRight.position.x);
            float maxX = Mathf.Max(boundaryLeft.position.x, boundaryRight.position.x);
            if (vx < 0f && _rb.position.x <= minX) vx = 0f;
            if (vx > 0f && _rb.position.x >= maxX) vx = 0f;
        }

        _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);
    }

    private void FaceTowards(float targetX)
    {
        float dx = targetX - _rb.position.x;
        if (Mathf.Abs(dx) <= ReachThreshold) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(_originalScale.x) * (dx < 0f ? -1f : 1f);
        transform.localScale = scale;
    }

    private bool PlayerHeard()
    {
        if (playerNoise == null || player == null) return false;
        float noiseRadius = playerNoise.GetCurrentNoiseRadius();
        if (noiseRadius <= 0f) return false;
        return Vector2.Distance(transform.position, player.position) <= noiseRadius;
    }

    private bool IsWithinBounds(Vector2 pos)
    {
        if (boundaryLeft == null || boundaryRight == null) return true;
        float minX = Mathf.Min(boundaryLeft.position.x, boundaryRight.position.x);
        float maxX = Mathf.Max(boundaryLeft.position.x, boundaryRight.position.x);
        return pos.x >= minX && pos.x <= maxX;
    }

    private void EnterState(State next)
    {
        _currentState = next;
    }

    private void OnDrawGizmosSelected()
    {
        if (boundaryLeft != null && boundaryRight != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(boundaryLeft.position, boundaryRight.position);
        }
    }
}
