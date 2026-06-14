using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip musicTrack;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        _source.clip = musicTrack;
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = volume;
    }

    private void Start()
    {
        if (musicTrack != null && !_source.isPlaying)
            _source.Play();
    }
}
