using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NoiseEmitter : MonoBehaviour
{
    [SerializeField] public float walkNoiseRadius   = 3f;
    [SerializeField] public float runNoiseRadius    = 6f;
    [SerializeField] public float jumpNoiseRadius   = 4f;
    [SerializeField] public float crouchNoiseRadius = 1f;

    private Rigidbody2D _rb;
    private float _currentNoiseRadius;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _currentNoiseRadius = CalculateNoiseRadius();
    }

    private float CalculateNoiseRadius()
    {
        if (_rb.linearVelocity.magnitude < 0.1f)
            return 0f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            return crouchNoiseRadius;

        return walkNoiseRadius;
    }

    public float GetCurrentNoiseRadius() => _currentNoiseRadius;

    public void DrawGizmo()
    {
        if (_currentNoiseRadius <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _currentNoiseRadius);
    }

    private void OnDrawGizmos()
    {
        // В Edit-режиме показываем walkNoiseRadius как ориентир.
        // В Play-режиме — актуальный радиус (0 когда стоим, >0 когда двигаемся).
        float radius = Application.isPlaying ? _currentNoiseRadius : walkNoiseRadius;
        if (radius <= 0f) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
