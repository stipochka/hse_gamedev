using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool instaKill = false;

    private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.gameObject);

    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other.gameObject);

    private void TryDamage(GameObject obj)
    {
        var health = obj.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        if (instaKill) health.Kill();
        else health.TakeDamage(damage);
    }
}
