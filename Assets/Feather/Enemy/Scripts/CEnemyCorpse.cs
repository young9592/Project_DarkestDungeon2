using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CEnemyCorpse : CEntity
{
    private CEnemy _owner;
    private SpriteRenderer _sprite;
    private Renderer[] _livingRenderers;
    private bool[] _rendererStates;
    private Camera _camera;
    private int _health;
    private bool _hasSpawned;

    public CEnemy Owner { get { return _owner; } }
    public int CurrentHp { get { return _health; } }
    public int FormationSize { get { return _owner != null ? _owner.EnemyData.FormationSize : 1; } }
    public bool IsAvailable { get { return _hasSpawned && !_isDead && _health > 0 && isActiveAndEnabled; } }
    public bool IsRemoved { get { return _hasSpawned && _isDead; } }
    public override CEntityDataSO EntityData { get { return _owner != null ? _owner.EntityData : null; } }
    public override EEntityType EntityType { get { return EEntityType.Enemy; } }
    public override ERank CurrentRank { get { return _owner != null ? _owner.CurrentRank : ERank.None; } }

    public event Action<CEnemyCorpse> OnRemoved;
    public event Action<CEnemyCorpse> OnDamaged;

    protected override void Awake()
    {
        base.Awake();
        _isDead = true;
    }

    public void Configure(CEnemy owner)
    {
        if (_owner != null || owner == null) return;
        _owner = owner;
        _livingRenderers = owner.GetComponentsInChildren<Renderer>(true);
        _rendererStates = new bool[_livingRenderers.Length];
        _camera = Camera.main;
        _sprite = gameObject.AddComponent<SpriteRenderer>();
        _sprite.sprite = owner.EnemyData.CorpseSprite;
        _sprite.enabled = false;
        _sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _sprite.receiveShadows = false;
        owner.DeathPresentation.OnDeathPresentationCompleted += HandleDeathCompleted;
    }

    private void HandleDeathCompleted(CEntity entity)
    {
        if (_hasSpawned || _owner == null || entity != _owner || !_owner.IsDead
            || !_owner.EnemyData.LeavesCorpse || _sprite.sprite == null) return;

        _hasSpawned = true;
        _isDead = false;
        _health = _owner.EnemyData.CorpseHealth;
        TokenManager.ClearAllTokens();
        for (int i = 0; i < _livingRenderers.Length; i++)
        {
            if (_livingRenderers[i] == null) continue;
            _rendererStates[i] = _livingRenderers[i].enabled;
            _livingRenderers[i].enabled = false;
        }
        float width = _owner.EnemyData.CorpseWidth;
        float scale = width / Mathf.Max(0.001f, _sprite.sprite.bounds.size.x);
        transform.localScale = Vector3.one * scale;
        _sprite.enabled = true;
        FaceCamera();
    }

    private void LateUpdate()
    {
        if (!_hasSpawned && _owner != null && _owner.IsDead
            && _owner.IsDeathPresented && !_owner.DeathPresentation.IsBusy) HandleDeathCompleted(_owner);
        if (IsAvailable) FaceCamera();
    }

    private void FaceCamera()
    {
        if (_camera == null) _camera = Camera.main;
        if (_camera != null) transform.rotation = _camera.transform.rotation;
    }

    protected override bool CanBeginAction()
    {
        return false;
    }

    public override void TakeDamage(int finalDamage)
    {
        if (!IsAvailable || finalDamage <= 0) return;
        _health = Mathf.Max(0, _health - finalDamage);
        OnDamaged?.Invoke(this);
        if (_health == 0) TryRemove();
    }

    public bool TryRemove()
    {
        if (!_hasSpawned || _isDead) return false;
        _health = 0;
        _isDead = true;
        _sprite.enabled = false;
        TokenManager.ClearAllTokens();
        // 효과 완료 콜백까지 대상 객체는 유지합니다.
        OnRemoved?.Invoke(this);
        return true;
    }

    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        // 시체의 지속 피해 및 라운드 진행은 전투 라운드 연결 단계에서 처리합니다.
    }

    public override void ResetEntityState()
    {
        if (_hasSpawned && _livingRenderers != null)
        {
            for (int i = 0; i < _livingRenderers.Length; i++)
            {
                if (_livingRenderers[i] != null) _livingRenderers[i].enabled = _rendererStates[i];
            }
        }
        _hasSpawned = false;
        _isDead = true;
        _health = 0;
        TokenManager.ClearAllTokens();
        if (_sprite != null) _sprite.enabled = false;
    }

    private void OnDestroy()
    {
        if (_owner != null && _owner.DeathPresentation != null)
            _owner.DeathPresentation.OnDeathPresentationCompleted -= HandleDeathCompleted;
    }
}
