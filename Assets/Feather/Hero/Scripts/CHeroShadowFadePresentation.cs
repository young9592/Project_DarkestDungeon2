using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroShadowFadePresentation : MonoBehaviour
{
    private enum EPresentationState
    {
        None,
        Attack,
        Returning
    }

    [Header("Shadow Fade")]
    [SerializeField] private CHeroSkillSO _skill;
    [SerializeField] private Shader _shader;
    [SerializeField] private Color _tint = new Color(0.22f, 0.18f, 0.85f, 1.0f);
    [SerializeField, Range(0.0f, 0.5f)] private float _echoStart = 0.1f;
    [SerializeField, Range(0.5f, 1.0f)] private float _fadeStart = 0.78f;
    [SerializeField, Min(0.05f)] private float _returnDuration = 0.18f;

    private CPlayer _hero;
    private CHeroAnimation _animation;
    private Animator _animator;
    private Camera _camera;
    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private readonly List<Material> _materials = new List<Material>();
    private readonly List<Material> _echoMaterials = new List<Material>();
    private GameObject _echo;
    private Vector3 _echoStartPosition;
    private Vector3 _echoDirection;
    private float _height;
    private float _returnTime;
    private EPresentationState _state;

    public bool IsBusy
    {
        get { return _state != EPresentationState.None; }
    }

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _animation = GetComponent<CHeroAnimation>();
        _animator = GetComponentInChildren<Animator>(true);
        _camera = Camera.main;
        if (_hero == null || _animation == null || _animator == null || _shader == null || _skill == null)
        {
            Debug.LogError(name + ": 그림자 은신 연출 연결을 확인해야 합니다.", this);
            enabled = false;
            return;
        }

        _renderers = _animator.GetComponentsInChildren<Renderer>(true);
    }

    private void OnEnable()
    {
        if (_hero == null) return;
        _hero.OnAttackPresentationStarted += Begin;
        _hero.OnAttackPresentationCompleted += BeginReturn;
        _hero.OnAttackPresentationInterrupted += Cancel;
        _hero.OnActionInterrupted += Cancel;
        _hero.OnHitPresentationStarted += Cancel;
    }

    private void Begin(CEntity actor)
    {
        Restore();
        if (_hero.ActionSkill != _skill || _renderers == null || _renderers.Length == 0) return;

        _originalMaterials = new Material[_renderers.Length][];
        _height = 0.0f;
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].sharedMaterials;
            _renderers[i].sharedMaterials = CreateMaterials(_originalMaterials[i], _materials);
            if (_renderers[i].enabled && _renderers[i].gameObject.activeInHierarchy)
            {
                _height = Mathf.Max(_height, _renderers[i].bounds.size.y);
            }
        }

        _state = EPresentationState.Attack;
        SetAppearance(_materials, 0.0f, 1.0f);
    }

    private Material[] CreateMaterials(Material[] originals, List<Material> materials)
    {
        if (originals == null) return new Material[0];
        Material[] copies = new Material[originals.Length];
        for (int i = 0; i < originals.Length; i++)
        {
            if (originals[i] == null) continue;
            Material copy = new Material(_shader);
            copy.mainTexture = originals[i].mainTexture;
            copy.mainTextureScale = originals[i].mainTextureScale;
            copy.mainTextureOffset = originals[i].mainTextureOffset;
            copy.SetColor("_Tint", _tint);
            copies[i] = copy;
            materials.Add(copy);
        }

        return copies;
    }

    private void LateUpdate()
    {
        if (_state == EPresentationState.None) return;
        if (_state == EPresentationState.Returning)
        {
            // 대기 자세가 실제 평가된 뒤에 다시 보이게 한다.
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")) return;
            _returnTime += Time.deltaTime;
            float progress = Mathf.Clamp01(_returnTime / _returnDuration);
            SetAppearance(_materials, 1.0f - progress, progress);
            if (progress < 1.0f) return;
            Restore();
            _hero.TryCompleteAction();
            return;
        }

        if (!_animation.TryGetAttackProgress(out float attackProgress, out _)) return;
        float tint = Mathf.SmoothStep(0.0f, 1.0f, attackProgress / Mathf.Max(0.01f, _echoStart));
        float opacity = Mathf.Lerp(1.0f, 0.72f, tint);
        if (attackProgress >= _fadeStart)
        {
            opacity *= 1.0f - Mathf.SmoothStep(0.0f, 1.0f,
                Mathf.InverseLerp(_fadeStart, 0.97f, attackProgress));
        }

        SetAppearance(_materials, tint, opacity);
        if (attackProgress >= _echoStart && _echo == null) CreateEcho();
        if (_echo == null) return;
        float echoProgress = Mathf.InverseLerp(_echoStart, 0.8f, attackProgress);
        _echo.transform.position = _echoStartPosition + _echoDirection * (_height * 0.15f * echoProgress);
        SetAppearance(_echoMaterials, 1.0f, (1.0f - echoProgress) * 0.34f);
    }

    private void CreateEcho()
    {
        if (_animator.transform == transform || _originalMaterials == null) return;
        // 모델 자식만 복제해 현재 자세를 고정한다. 영웅 로직은 복제하지 않는다.
        _echo = Instantiate(_animator.gameObject, _animator.transform.position, _animator.transform.rotation);
        _echo.name = "Shadow Fade Echo";
        _echo.transform.localScale = _animator.transform.lossyScale;
        foreach (Animator animator in _echo.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (MonoBehaviour script in _echo.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
        foreach (Collider collider in _echo.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        Transform[] sourceBones = _animator.GetComponentsInChildren<Transform>(true);
        Transform[] echoBones = _echo.GetComponentsInChildren<Transform>(true);
        for (int i = 1; i < sourceBones.Length && i < echoBones.Length; i++)
        {
            echoBones[i].localPosition = sourceBones[i].localPosition;
            echoBones[i].localRotation = sourceBones[i].localRotation;
            echoBones[i].localScale = sourceBones[i].localScale;
        }
        Renderer[] renderers = _echo.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length && i < _originalMaterials.Length; i++)
        {
            renderers[i].sharedMaterials = CreateMaterials(_originalMaterials[i], _echoMaterials);
        }

        if (_camera == null) _camera = Camera.main;
        _echoDirection = _camera == null ? Vector3.left : -_camera.transform.right;
        _echoStartPosition = _echo.transform.position + _echoDirection * (_height * 0.05f);
        _echo.transform.position = _echoStartPosition;
    }

    private static void SetAppearance(List<Material> materials, float tint, float opacity)
    {
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] == null) continue;
            materials[i].SetFloat("_TintStrength", tint);
            materials[i].SetFloat("_Opacity", opacity);
        }
    }

    private void BeginReturn(CEntity actor)
    {
        if (_state != EPresentationState.Attack) return;
        _state = EPresentationState.Returning;
        _returnTime = 0.0f;
        SetAppearance(_materials, 1.0f, 0.0f);
        if (_echo != null) _echo.SetActive(false);
    }

    private void Cancel(CEntity actor)
    {
        Restore();
    }

    private void Restore()
    {
        _state = EPresentationState.None;
        if (_originalMaterials != null && _renderers != null)
        {
            for (int i = 0; i < _renderers.Length && i < _originalMaterials.Length; i++)
            {
                if (_renderers[i] != null) _renderers[i].sharedMaterials = _originalMaterials[i];
            }
        }

        _originalMaterials = null;
        if (_echo != null)
        {
            _echo.SetActive(false);
            Destroy(_echo);
            _echo = null;
        }

        for (int i = 0; i < _materials.Count; i++) Destroy(_materials[i]);
        for (int i = 0; i < _echoMaterials.Count; i++) Destroy(_echoMaterials[i]);
        _materials.Clear();
        _echoMaterials.Clear();
    }

    private void OnDisable()
    {
        if (_hero != null)
        {
            _hero.OnAttackPresentationStarted -= Begin;
            _hero.OnAttackPresentationCompleted -= BeginReturn;
            _hero.OnAttackPresentationInterrupted -= Cancel;
            _hero.OnActionInterrupted -= Cancel;
            _hero.OnHitPresentationStarted -= Cancel;
        }

        Restore();
    }
}
