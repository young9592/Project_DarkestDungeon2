using UnityEngine;
using UnityEngine.UI;

public class CHeroPreparationModel : MonoBehaviour
{
    [Header("Model Preview")]
    [SerializeField] private RawImage _image;
    [SerializeField, Range(128, 1024)] private int _resolution = 512;
    [SerializeField] private CHeroDataSO[] _heroes;
    [SerializeField] private GameObject[] _visualPrefabs;
    private CPlayer _prefab;
    private GameObject _stage;
    private Camera _camera;
    private RenderTexture _texture;

    public void Show(CPlayer prefab, Quaternion rotation)
    {
        if (prefab == _prefab && _stage != null) return;
        Clear();
        if (prefab == null || _image == null) return;
        GameObject visual = FindVisual(prefab.HeroData);
        if (visual == null) return;

        _prefab = prefab;
        _stage = new GameObject("Hero Model Preview");
        _stage.transform.position = new Vector3(1000.0f, 1000.0f, 1000.0f);
        _stage.transform.rotation = rotation;
        GameObject model = Instantiate(visual, _stage.transform, false);
        model.SetActive(true);
        Transform[] parts = model.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].gameObject.layer = 31;
        }

        Animator animator = model.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.applyRootMotion = false;
            animator.Update(0.0f);
        }

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Clear();
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        GameObject cameraRoot = new GameObject("Hero Preview Camera");
        cameraRoot.transform.SetParent(_stage.transform, false);
        _camera = cameraRoot.AddComponent<Camera>();
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.055f, 0.075f, 0.085f, 1.0f);
        _camera.cullingMask = 1 << 31;
        _camera.orthographic = true;
        float aspect = Mathf.Max(0.1f, _image.rectTransform.rect.width / Mathf.Max(1.0f, _image.rectTransform.rect.height));
        _camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect) * 1.2f;
        _camera.nearClipPlane = 0.01f;
        _camera.farClipPlane = 100.0f;
        Camera battleCamera = Camera.main;
        _camera.transform.rotation = battleCamera != null ? battleCamera.transform.rotation : Quaternion.identity;
        _camera.transform.position = bounds.center - _camera.transform.forward * 10.0f;
        _texture = new RenderTexture(_resolution, Mathf.RoundToInt(_resolution / aspect), 16);
        _texture.Create();
        _camera.targetTexture = _texture;
        _image.texture = _texture;
    }

    private GameObject FindVisual(CHeroDataSO data)
    {
        if (_heroes == null || _visualPrefabs == null) return null;
        int count = Mathf.Min(_heroes.Length, _visualPrefabs.Length);
        for (int i = 0; i < count; i++)
        {
            if (_heroes[i] == data) return _visualPrefabs[i];
        }

        return null;
    }

    public void Clear()
    {
        if (_camera != null) _camera.targetTexture = null;
        if (_image != null) _image.texture = null;
        if (_stage != null)
        {
            _stage.SetActive(false);
            Destroy(_stage);
        }

        if (_texture != null)
        {
            _texture.Release();
            Destroy(_texture);
        }

        _prefab = null;
        _stage = null;
        _camera = null;
        _texture = null;
    }

    private void OnDisable()
    {
        Clear();
    }
}
