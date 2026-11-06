using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CEnemySkillFeedback : MonoBehaviour
{
    [Header("Launch Origins")]
    [SerializeField] private Transform _handOrigin;
    [SerializeField] private Transform _mouthOrigin;

    private CEnemy _enemy;
    private CEnemyAttackAnimation _animation;
    private Camera _camera;
    private CEnemySkillFeedbackSO _feedback;
    private CSkillDataSO _skill;
    private CEntity _target;
    private readonly List<CHeroFeedbackVisual> _visuals = new List<CHeroFeedbackVisual>();
    private bool _hasDamage;
    private bool _hasImpacted;
    private int _shots;
    private int _version;

    public event Action<CSkillDataSO, CEntity> OnProjectileLaunched;
    public event Action<CSkillDataSO, CEntity, Vector3> OnImpactPlayed;

    public bool IsBusy
    {
        get
        {
            for (int i = 0; i < _visuals.Count; i++)
            {
                if (_visuals[i] != null && _visuals[i].gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private void Awake()
    {
        _enemy = GetComponent<CEnemy>();
        _animation = GetComponent<CEnemyAttackAnimation>();
        _camera = Camera.main;
        if (_enemy == null || _animation == null)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_enemy == null) return;
        _enemy.OnAttackPresentationStarted += Begin;
        _enemy.OnAttackPresentationCompleted += AttackCompleted;
        _enemy.OnAttackPresentationInterrupted += End;
        _enemy.OnActionCompleted += End;
        _enemy.OnActionInterrupted += End;
    }

    private void Begin(CEntity actor)
    {
        Clear();
        _skill = _enemy.ActionSkill;
        _target = _enemy.ActionTarget;
        _feedback = _animation.GetFeedback(_skill);
        if (_feedback == null || _target == null)
        {
            Clear();
            return;
        }

        if (_camera == null) _camera = Camera.main;
        if (_skill.EffectList != null)
        {
            for (int i = 0; i < _skill.EffectList.Count; i++)
            {
                CEffectSO effect = _skill.EffectList[i];
                if (effect != null && effect.EffectType == EEffectType.Damage)
                {
                    _hasDamage = true;
                }
            }
        }

        BindTarget(true);
    }

    private void Update()
    {
        _visuals.RemoveAll(visual => visual == null);
        if (_feedback == null) return;
        if (_target == null || !_target.isActiveAndEnabled)
        {
            _enemy.CancelAction();
            return;
        }

        if (_animation.TryGetAttackRecovery(out float progress, out float remaining)
            && _shots < _feedback.ShotCount
            && progress >= _feedback.ReleaseTime + _shots * _feedback.ShotSpacing)
        {
            Launch(remaining);
        }
    }

    private void Launch(float remaining)
    {
        if (_feedback == null || _target == null) return;
        int version = _version;
        _shots++;
        Vector3 origin = GetOrigin();
        Spawn(_feedback.Launch, origin, origin, 0.0f, false);
        if (_feedback.Projectile == null) return;
        Spawn(_feedback.Projectile, origin, CHeroSkillFeedback.Center(_target),
            Mathf.Clamp(remaining * 0.75f, 0.08f, 0.4f), true);
        OnProjectileLaunched?.Invoke(_skill, _target);
        if (version != _version) return;
    }

    private Vector3 GetOrigin()
    {
        if (_feedback.Origin == EEnemyLaunchOrigin.Hand && _handOrigin != null)
        {
            return _handOrigin.position;
        }

        if (_feedback.Origin == EEnemyLaunchOrigin.Mouth && _mouthOrigin != null)
        {
            return _mouthOrigin.position;
        }

        return CHeroSkillFeedback.Center(_enemy);
    }

    private void AttackCompleted(CEntity actor)
    {
        if (_feedback == null) return;
        int version = _version;
        while (_shots < _feedback.ShotCount)
        {
            Launch(0.1f);
            if (version != _version) return;
        }

        // 효과가 없는 스킬은 피해 없이 타격 연출만 재생
        if (!_hasDamage) Impact(_target);
    }

    private void HitStarted(CEntity target)
    {
        if (_hasDamage) Impact(target);
    }

    private void Impact(CEntity target)
    {
        if (_feedback == null || target == null || target != _target || _hasImpacted) return;
        _hasImpacted = true;
        Vector3 point = CHeroSkillFeedback.Center(target);
        Spawn(_feedback.Impact, point, point, 0.0f, false);
        OnImpactPlayed?.Invoke(_skill, target, point);
    }

    private void Spawn(CHeroFeedbackVisual prefab, Vector3 start, Vector3 end, float duration, bool projectile)
    {
        if (prefab == null || _camera == null) return;
        CHeroFeedbackVisual visual = Instantiate(prefab);
        // 이펙트가 모델 중심 계산에 섞이지 않게 월드에 생성
        Vector3 offset = -_camera.transform.forward * 0.004f;
        visual.Play(_camera, start + offset, end + offset, duration, _feedback.ArcHeight, projectile);
        _visuals.Add(visual);
    }

    private void BindTarget(bool add)
    {
        CPlayer hero = _target as CPlayer;
        if (hero != null)
        {
            if (add) hero.OnHitPresentationStarted += HitStarted;
            else hero.OnHitPresentationStarted -= HitStarted;
        }
    }

    private void End(CEntity actor)
    {
        Clear();
    }

    private void Clear()
    {
        _version++;
        BindTarget(false);
        for (int i = 0; i < _visuals.Count; i++)
        {
            if (_visuals[i] == null) continue;
            _visuals[i].gameObject.SetActive(false);
            Destroy(_visuals[i].gameObject);
        }

        _visuals.Clear();
        _skill = null;
        _feedback = null;
        _target = null;
        _hasDamage = false;
        _hasImpacted = false;
        _shots = 0;
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackPresentationStarted -= Begin;
            _enemy.OnAttackPresentationCompleted -= AttackCompleted;
            _enemy.OnAttackPresentationInterrupted -= End;
            _enemy.OnActionCompleted -= End;
            _enemy.OnActionInterrupted -= End;
        }

        Clear();
        if (_enemy != null && _enemy.IsActionActive) _enemy.CancelAction();
    }
}
