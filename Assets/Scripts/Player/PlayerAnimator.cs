using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 velocity = _rb.linearVelocity;

        _animator.SetBool("isWalking", Mathf.Abs(velocity.x) > 0.1f);

        if (velocity.y > 0.1f)
            _animator.SetTrigger("Jump");

        if (velocity.x < 0f)
            _spriteRenderer.flipX = true;
        else if (velocity.x > 0f)
            _spriteRenderer.flipX = false;
    }
}
