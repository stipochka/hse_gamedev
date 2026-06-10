using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
public class NoiseRadiusIndicator : MonoBehaviour
{
    [SerializeField] private int segments = 32;
    [SerializeField] private Color color = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private float lineWidth = 0.05f;

    private NoiseEmitter _noiseEmitter;
    private LineRenderer _line;

    private void Awake()
    {
        _noiseEmitter = GetComponent<NoiseEmitter>();

        _line = gameObject.AddComponent<LineRenderer>();
        _line.loop = true;
        _line.positionCount = segments;
        _line.useWorldSpace = false;
        _line.startWidth = lineWidth;
        _line.endWidth = lineWidth;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = color;
        _line.endColor = color;
        _line.sortingOrder = 10;
    }

    private void Update()
    {
        float radius = _noiseEmitter.GetCurrentNoiseRadius();
        _line.enabled = radius > 0f;
        if (radius <= 0f) return;

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            _line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }
}
