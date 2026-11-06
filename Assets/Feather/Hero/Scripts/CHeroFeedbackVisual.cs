using UnityEngine;

public class CHeroFeedbackVisual : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer[] _renderers;
    [SerializeField, Min(0.05f)] private float _lifetime = 0.25f;
    [SerializeField] private Vector3 _size = new Vector3(0.006f, 0.006f, 0.006f);
    [SerializeField] private bool _billboard = true;
    [SerializeField] private bool _alignToFlight;
    [SerializeField] private Vector2Int _sheet = Vector2Int.one;
    private Camera _camera;
    private MaterialPropertyBlock _properties;
    private Vector3 _start;
    private Vector3 _end;
    private float _age;
    private float _duration;
    private float _arc;
    private bool _isProjectile;
    private bool _isPlaying;
    private static readonly int _alphaId = Shader.PropertyToID("_Fade");
    private static readonly int _uvId = Shader.PropertyToID("_Frame");

    private void Awake()
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        _properties = new MaterialPropertyBlock();
    }

    public void Play(Camera camera, Vector3 start, Vector3 end, float flightDuration, float arc, bool projectile)
    {
        _camera = camera;
        _start = start;
        _end = end;
        _age = 0.0f;
        _arc = arc;
        _isProjectile = projectile;
        _duration = projectile ? Mathf.Max(0.05f, flightDuration) : _lifetime;
        _isPlaying = true;
        transform.position = start;
        transform.localScale = _size;
        UpdateVisual();
    }

    private void Update()
    {
        if (!_isPlaying) return;
        _age += Time.deltaTime;
        if (_age >= _duration)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        float progress = Mathf.Clamp01(_age / _duration);
        if (_isProjectile)
        {
            transform.position = Vector3.Lerp(_start, _end, progress) + Vector3.up * (4.0f * _arc * progress * (1.0f - progress));
        }

        if (_billboard && _camera != null)
        {
            transform.rotation = _camera.transform.rotation;
            if (_alignToFlight && _isProjectile)
            {
                Vector3 direction = _camera.transform.InverseTransformDirection(_end - _start);
                transform.rotation *= Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }
        else if (_isProjectile)
        {
            transform.Rotate(Vector3.forward, Time.deltaTime * 320.0f, Space.Self);
        }

        if (!_isProjectile)
        {
            transform.localScale = _size * Mathf.Lerp(0.75f, 1.15f, progress);
        }

        _properties.SetFloat(_alphaId, Mathf.Clamp01((1.0f - progress) * 4.0f));
        int columns = Mathf.Max(1, _sheet.x);
        int rows = Mathf.Max(1, _sheet.y);
        int frame = Mathf.Min(columns * rows - 1, (int)(progress * columns * rows));
        _properties.SetVector(_uvId, new Vector4(1.0f / columns, 1.0f / rows,
            (float)(frame % columns) / columns, 1.0f - (float)(frame / columns + 1) / rows));
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null) _renderers[i].SetPropertyBlock(_properties);
        }
    }
}
