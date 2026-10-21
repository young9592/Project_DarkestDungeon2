using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroSkillFeedback : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource _audio;
    [SerializeField] private Renderer _rangedOrigin;
    [Header("Projectile Origin")]
    [SerializeField] private Transform _projectileOrigin;
    private CPlayer _hero;
    private CHeroAnimation _animation;
    private Camera _camera;
    private CHeroSkillFeedbackSO _feedback;
    private CSkillDataSO _skill;
    private readonly List<CEntity> _targets = new List<CEntity>();
    private readonly List<CEntity> _impacted = new List<CEntity>();
    private readonly List<CHeroFeedbackVisual> _visuals = new List<CHeroFeedbackVisual>();
    private bool _hasDamage;
    private bool _hasHeal;
    private bool _impactSoundPlayed;
    private int _shots;
    private int _feedbackVersion;

    public event Action<CSkillDataSO, CEntity> OnProjectileLaunched;
    public event Action<CSkillDataSO, CEntity, Vector3> OnImpactPlayed;

    public bool IsBusy
    {
        get
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                if (_visuals[i] != null && _visuals[i].gameObject.activeSelf) return true;
            }

            return _audio != null && _audio.isPlaying;
        }
    }

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _animation = GetComponent<CHeroAnimation>();
        _camera = Camera.main;
        if (_audio == null) _audio = GetComponent<AudioSource>();
        if (_hero == null || _animation == null)
        {
            Debug.LogWarning("Hero feedback needs a Hero and animation component.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_hero == null) return;
        _hero.OnAttackPresentationStarted += Begin;
        _hero.OnAttackPresentationCompleted += AttackCompleted;
        _hero.OnActionCompleted += End;
        _hero.OnActionInterrupted += End;
        _hero.OnAttackPresentationInterrupted += End;
    }

    private void Begin(CEntity actor)
    {
        Clear();
        CHeroSkillSO skill = _hero.ActionSkill as CHeroSkillSO;
        if (skill == null || skill.Feedback == null) return;
        _skill = skill;
        _feedback = skill.Feedback;
        if (_camera == null) _camera = Camera.main;
        if (skill.EffectList != null)
        {
            for (int i = 0; i < skill.EffectList.Count; i++)
            {
                CEffectSO effect = skill.EffectList[i];
                if (effect == null) continue;
                _hasDamage |= effect.EffectType == EEffectType.Damage;
                _hasHeal |= effect.EffectType == EEffectType.Heal;
            }
        }

        IReadOnlyList<CEntity> targets = _hero.ActionTargets;
        if (targets == null) return;
        for (int i = 0; i < targets.Count; i++)
        {
            CEntity target = targets[i];
            if (target == null || _targets.Contains(target)) continue;
            _targets.Add(target);
            Bind(target, true);
        }
    }

    private void Update()
    {
        _visuals.RemoveAll(visual => visual == null);
        if (_feedback == null) return;
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] == null || !_targets[i].isActiveAndEnabled)
            {
                _hero.CancelAction();
                return;
            }
        }

        if (_animation.TryGetAttackProgress(out float progress, out float remaining)
            && _shots < _feedback.ShotCount
            && progress >= _feedback.ReleaseTime + _shots * _feedback.ShotSpacing)
        {
            Launch(remaining);
        }
    }

    private void Launch(float remaining)
    {
        if (_feedback == null) return;
        int version = _feedbackVersion;
        _shots++;
        Vector3 origin = Center(_hero);
        if (_feedback.Projectile != null && _projectileOrigin != null)
        {
            origin = _projectileOrigin.position;
        }
        else if (_feedback.Projectile != null && _rangedOrigin != null)
        {
            Bounds bounds = _rangedOrigin.bounds;
            origin = bounds.center;
            if (_targets.Count > 0 && _targets[0] != null)
            {
                // 무기 중심 대신 대상을 향한 총구 쪽에서 발사
                Vector3 direction = (Center(_targets[0]) - origin).normalized;
                Vector3 absolute = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
                origin += direction * Vector3.Dot(bounds.extents, absolute) * 0.8f;
            }
        }
        else if (_camera != null)
        {
            origin += _camera.transform.right * 0.003f;
        }
        Spawn(_feedback.Launch, origin, origin, 0, false);
        PlaySound(_feedback.LaunchSound);
        for (int i = 0; i < _targets.Count; i++)
        {
            CEntity target = _targets[i];
            if (target == null) continue;
            if (_feedback.Projectile != null)
            {
                Spawn(_feedback.Projectile, origin, Center(target), Mathf.Clamp(remaining * 0.75f, 0.05f, 0.35f), true);
                OnProjectileLaunched?.Invoke(_skill, target);
                if (version != _feedbackVersion) return;
            }
        }
    }

    private void AttackCompleted(CEntity actor)
    {
        if (_feedback == null) return;
        int version = _feedbackVersion;
        while (_shots < _feedback.ShotCount)
        {
            Launch(0.1f);
            if (version != _feedbackVersion) return;
        }

        if (_hasDamage || _hasHeal) return;
        // 아직 효과가 없는 보조 스킬도 선택한 대상에 연출만 재생
        for (int i = 0; i < _targets.Count; i++)
        {
            Impact(_targets[i]);
            if (version != _feedbackVersion) return;
        }
    }

    private void HitStarted(CEntity target)
    {
        if (_hasDamage) Impact(target);
    }

    private void HealthChanged(CPlayer target, int previous, int current)
    {
        if (_hasHeal && current > previous) Impact(target);
    }

    private void Impact(CEntity target)
    {
        if (_feedback == null || target == null || !_targets.Contains(target) || _impacted.Contains(target)) return;
        _impacted.Add(target);
        Vector3 point = Center(target);
        Spawn(_feedback.Impact, point, point, 0, false);
        if (!_impactSoundPlayed)
        {
            PlaySound(_feedback.ImpactSound);
            _impactSoundPlayed = true;
        }

        OnImpactPlayed?.Invoke(_skill, target, point);
    }

    private void Spawn(CHeroFeedbackVisual prefab, Vector3 start, Vector3 end, float duration, bool projectile)
    {
        if (prefab == null || _camera == null) return;
        CHeroFeedbackVisual visual = Instantiate(prefab);
        // 모델의 크기 계산에 이펙트가 섞이지 않도록 월드에 생성
        Vector3 offset = -_camera.transform.forward * 0.004f;
        visual.Play(_camera, start + offset, end + offset, duration, _feedback.ArcHeight, projectile);
        _visuals.Add(visual);
    }

    public static Vector3 Center(CEntity entity)
    {
        Renderer[] renderers = entity.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(entity.transform.position + Vector3.up * 0.008f, Vector3.zero);
        bool found = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled) continue;
            if (!found) bounds = renderers[i].bounds;
            else bounds.Encapsulate(renderers[i].bounds);
            found = true;
        }

        return bounds.center;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audio != null && _audio.isActiveAndEnabled)
        {
            _audio.PlayOneShot(clip, _feedback.Volume);
        }
    }

    private void Bind(CEntity target, bool add)
    {
        CPlayer hero = target as CPlayer;
        CEnemy enemy = target as CEnemy;
        if (hero != null)
        {
            if (add)
            {
                hero.OnHitPresentationStarted += HitStarted;
                hero.OnHealthChanged += HealthChanged;
            }
            else
            {
                hero.OnHitPresentationStarted -= HitStarted;
                hero.OnHealthChanged -= HealthChanged;
            }
        }

        if (enemy != null)
        {
            if (add) enemy.OnHitPresentationStarted += HitStarted;
            else enemy.OnHitPresentationStarted -= HitStarted;
        }
    }

    private void End(CEntity actor)
    {
        Clear();
    }

    private void Clear()
    {
        _feedbackVersion++;
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] != null) Bind(_targets[i], false);
        }

        for (int i = 0; i < _visuals.Count; i++)
        {
            if (_visuals[i] == null) continue;
            _visuals[i].gameObject.SetActive(false);
            Destroy(_visuals[i].gameObject);
        }

        if (_audio != null) _audio.Stop();
        _visuals.Clear();
        _targets.Clear();
        _impacted.Clear();
        _skill = null;
        _feedback = null;
        _hasDamage = false;
        _hasHeal = false;
        _impactSoundPlayed = false;
        _shots = 0;
    }

    private void OnDisable()
    {
        if (_hero != null)
        {
            _hero.OnAttackPresentationStarted -= Begin;
            _hero.OnAttackPresentationCompleted -= AttackCompleted;
            _hero.OnActionCompleted -= End;
            _hero.OnActionInterrupted -= End;
            _hero.OnAttackPresentationInterrupted -= End;
        }

        Clear();
    }
}
