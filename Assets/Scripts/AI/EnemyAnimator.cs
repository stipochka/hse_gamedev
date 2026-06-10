using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _animator.SetBool("isWalking", Mathf.Abs(_rb.linearVelocity.x) > 0.1f);
    }

    public void PlayAttack()
    {
        _animator.SetTrigger("Attack");
    }
}
