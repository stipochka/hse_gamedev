using UnityEngine;
using Unity.Cinemachine;

// Повесь на любой GameObject в сцене, назначь Player Transform в Inspector.
// Камера создаётся автоматически при старте — сохранять сцену вручную не нужно.
public class CinemachineSetup : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [Tooltip("Опционально: Collider2D, ограничивающий движение камеры.")]
    [SerializeField] private Collider2D confinerBounds;

    private void Awake()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[CinemachineSetup] Назначь Player Transform в Inspector.", this);
            return;
        }

        EnsureCinemachineBrain();

        var vcamGO = new GameObject("CM PlayerCamera");
        SetupVirtualCamera(vcamGO);
        SetupPositionComposer(vcamGO);

        if (confinerBounds != null)
            SetupConfiner(vcamGO);
    }

    private static void EnsureCinemachineBrain()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[CinemachineSetup] Камера с тегом MainCamera не найдена.");
            return;
        }

        if (cam.GetComponent<CinemachineBrain>() == null)
            cam.gameObject.AddComponent<CinemachineBrain>();
    }

    private void SetupVirtualCamera(GameObject vcamGO)
    {
        var vcam = vcamGO.AddComponent<CinemachineCamera>();
        vcam.Follow = playerTransform;
    }

    private static void SetupPositionComposer(GameObject vcamGO)
    {
        var composer = vcamGO.AddComponent<CinemachinePositionComposer>();

        composer.Composition = new ScreenComposerSettings
        {
            DeadZone = new ScreenComposerSettings.DeadZoneSettings
            {
                Enabled = true,
                Size    = new Vector2(0.1f, 0.1f)
            },
            HardLimits = ScreenComposerSettings.Default.HardLimits
        };

        // X = горизонтальный damping, Y = вертикальный, Z = глубина (не используется в 2D).
        composer.Damping = new Vector3(0.5f, 0.5f, 0f);
    }

    private void SetupConfiner(GameObject vcamGO)
    {
        var confiner = vcamGO.AddComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = confinerBounds;
    }
}
