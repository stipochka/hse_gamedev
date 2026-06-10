using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1.5f;

    private const int BlinkCount = 3;
    private const float DeathReloadDelay = 1f;

    private int _currentHealth;
    private bool _isInvincible;
    private bool _isDead;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || _isInvincible || amount <= 0) return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
            return;
        }

        _isInvincible = true;
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkAndRecover());
    }

    // Мгновенная смерть, минуя HP и неуязвимость (например, падение за границы уровня).
    public void Kill()
    {
        if (_isDead) return;
        _currentHealth = 0;
        Die();
    }

    public void Heal(int amount)
    {
        if (_isDead || amount <= 0) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
    }

    private IEnumerator BlinkAndRecover()
    {
        float halfInterval = invincibilityDuration / (BlinkCount * 2f);

        for (int i = 0; i < BlinkCount; i++)
        {
            SetSpriteVisible(false);
            yield return new WaitForSeconds(halfInterval);

            SetSpriteVisible(true);
            yield return new WaitForSeconds(halfInterval);
        }

        _isInvincible = false;
    }

    private void SetSpriteVisible(bool visible)
    {
        if (_sprite != null)
            _sprite.enabled = visible;
    }

    private void Die()
    {
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Static;

        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(DeathReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };

        float x = 12f, y = 12f, w = 30f, h = 30f;

        for (int i = 0; i < maxHealth; i++)
        {
            string heart = i < _currentHealth ? "♥" : "♡";
            style.normal.textColor = i < _currentHealth ? Color.red : Color.gray;
            GUI.Label(new Rect(x + i * w, y, w, h), heart, style);
        }
    }
}
