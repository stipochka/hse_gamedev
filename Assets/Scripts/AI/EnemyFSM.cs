using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFSM : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Search }

    [Header("Detection")]
    public float detectionRange = 5f;
    public float chaseRange = 8f;
    public float alertDuration = 0.1f;
    public float searchDuration = 3f;

    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float chaseSpeed = 1.8f;

    [Header("References")]
    public Transform waypointA;
    public Transform waypointB;
    public Transform player;
    public NoiseEmitter playerNoise;

    [Header("Attack")]
    public float attackRange = 0.6f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private PlayerHealth _playerHealth;
    private Vector3 _originalScale;

    private State _currentState;
    private Transform _currentWaypoint;

    private float _alertTimer;
    private float _searchTimer;
    private Vector2 _lastKnownPlayerPos;

    private const float WaypointReachThreshold = 0.1f;

    private static readonly Color ColorPatrol = Color.white;
    private static readonly Color ColorAlert  = Color.yellow;
    private static readonly Color ColorChase  = Color.red;
    private static readonly Color ColorSearch = Color.blue;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;

        // Враг не падает и не прыгает — Rigidbody2D используется только для горизонтального движения.
        _rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
    }

    private void Start()
    {
        if (player != null)
            _playerHealth = player.GetComponentInParent<PlayerHealth>();

        _currentWaypoint = waypointB;
        EnterState(State.Patrol);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Alert:  UpdateAlert();  break;
            case State.Chase:  UpdateChase();  break;
            case State.Search: UpdateSearch(); break;
        }
    }

    // ── Patrol ────────────────────────────────────────────────────────────────

    private void UpdatePatrol()
    {
        MoveTowards2D(_currentWaypoint.position, patrolSpeed);

        if (ReachedPoint(_currentWaypoint.position))
            _currentWaypoint = _currentWaypoint == waypointA ? waypointB : waypointA;

        if (PlayerInRange(detectionRange) || PlayerHeard())
        {
            _lastKnownPlayerPos = player.position;
            EnterState(State.Alert);
        }
    }

    // ── Alert ─────────────────────────────────────────────────────────────────

    private void UpdateAlert()
    {
        _alertTimer -= Time.deltaTime;

        if (_alertTimer <= 0f)
            EnterState(State.Chase);
    }

    // ── Chase ─────────────────────────────────────────────────────────────────

    private void UpdateChase()
    {
        _lastKnownPlayerPos = player.position;

        // Игрок вышел за границы патрульной зоны — враг не преследует за пределы маршрута.
        if (!IsWithinPatrolBounds(player.position))
        {
            EnterState(State.Search);
            return;
        }

        MoveTowards2D(player.position, chaseSpeed);

        if (Vector2.Distance(transform.position, player.position) < attackRange)
            _playerHealth?.TakeDamage(1);

        if (!PlayerInRange(chaseRange))
            EnterState(State.Search);
    }

    // ── Search ────────────────────────────────────────────────────────────────

    private void UpdateSearch()
    {
        if (!ReachedPoint(_lastKnownPlayerPos))
            MoveTowards2D(_lastKnownPlayerPos, patrolSpeed);

        _searchTimer -= Time.deltaTime;

        if (_searchTimer <= 0f)
            EnterState(State.Patrol);
    }

    // ── State transitions ─────────────────────────────────────────────────────

    private void EnterState(State next)
    {
        _currentState = next;

        switch (next)
        {
            case State.Patrol:
                _currentWaypoint = NearestWaypoint();
                // Если игрок столкнулся и вытолкнул врага за границы маршрута,
                // принудительно возвращаем позицию внутрь — иначе коллайдер игрока
                // может заблокировать MovePosition и враг зависнет снаружи.
                _rb.position = ClampToPatrolBounds(_rb.position);
                break;

            case State.Alert:
                _alertTimer = alertDuration;
                _rb.linearVelocity = Vector2.zero;
                break;

            case State.Chase:
                break;

            case State.Search:
                _searchTimer = searchDuration;
                // Если игрок ушёл за пределы патрульной зоны, последняя известная позиция
                // недостижима — враг упирался в границу и никогда не запускал таймер.
                _lastKnownPlayerPos = ClampToPatrolBounds(_lastKnownPlayerPos);
                break;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void MoveTowards2D(Vector2 target, float speed)
    {
        float dx = target.x - _rb.position.x;
        float vx = Mathf.Abs(dx) > WaypointReachThreshold ? Mathf.Sign(dx) * speed : 0f;

        // Второй уровень защиты от выхода за границы: гасим скорость у самой границы,
        // даже если переход состояния ещё не произошёл в этом кадре.
        if (waypointA != null && waypointB != null)
        {
            float minX = Mathf.Min(waypointA.position.x, waypointB.position.x);
            float maxX = Mathf.Max(waypointA.position.x, waypointB.position.x);
            if (vx < 0f && _rb.position.x <= minX) vx = 0f;
            if (vx > 0f && _rb.position.x >= maxX) vx = 0f;
        }

        _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);

        if (vx < 0f)
            transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
        else if (vx > 0f)
            transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
    }

    private bool ReachedPoint(Vector2 target) =>
        Mathf.Abs(_rb.position.x - target.x) <= WaypointReachThreshold;

    private bool PlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= range;
    }

    private bool PlayerHeard()
    {
        if (playerNoise == null || player == null) return false;
        float noiseRadius = playerNoise.GetCurrentNoiseRadius();
        if (noiseRadius <= 0f) return false;
        return Vector2.Distance(transform.position, player.position) <= noiseRadius;
    }

    private Transform NearestWaypoint()
    {
        if (waypointA == null) return waypointB;
        if (waypointB == null) return waypointA;

        float dA = Vector2.Distance(transform.position, waypointA.position);
        float dB = Vector2.Distance(transform.position, waypointB.position);
        // Возвращаем БЛИЖАЙШУЮ точку: враг сразу идёт к ближайшей границе маршрута,
        // а не через всю карту к дальней (что раньше блокировалось коллайдером игрока).
        return dA <= dB ? waypointA : waypointB;
    }

    private bool IsWithinPatrolBounds(Vector2 pos)
    {
        if (waypointA == null || waypointB == null) return true;
        float minX = Mathf.Min(waypointA.position.x, waypointB.position.x);
        float maxX = Mathf.Max(waypointA.position.x, waypointB.position.x);
        return pos.x >= minX && pos.x <= maxX;
    }

    private Vector2 ClampToPatrolBounds(Vector2 pos)
    {
        if (waypointA == null || waypointB == null) return pos;
        float minX = Mathf.Min(waypointA.position.x, waypointB.position.x);
        float maxX = Mathf.Max(waypointA.position.x, waypointB.position.x);
        return new Vector2(Mathf.Clamp(pos.x, minX, maxX), pos.y);
    }

    private void OnDrawGizmosSelected()
    {
        Color stateColor = _currentState switch
        {
            State.Patrol => ColorPatrol,
            State.Alert  => ColorAlert,
            State.Chase  => ColorChase,
            State.Search => ColorSearch,
            _ => Color.white
        };
        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (waypointA != null && waypointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(waypointA.position, waypointB.position);
            Gizmos.DrawWireSphere(waypointA.position, 0.15f);
            Gizmos.DrawWireSphere(waypointB.position, 0.15f);
        }
    }
}
