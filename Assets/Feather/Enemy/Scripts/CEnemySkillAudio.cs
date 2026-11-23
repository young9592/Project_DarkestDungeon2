using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CEnemySkillAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private CCombatAudioSO _sounds;
    [SerializeField] private AudioSource _preparationSource;
    [SerializeField] private AudioSource _actionSource;

    private CEnemy _enemy;
    private CEnemyAttackAnimation _animation;
    private Animator _animator;
    private CEnemySkillSound _skillSound;
    private CEntity[] _battleTargets;
    private CSkillDataSO _skill;
    private readonly List<CEntity> _targets = new List<CEntity>();
    private bool _prepared;
    private bool _used;
    private bool _recovered;
    private bool _hasMiss;
    private bool _hasHit;
    private bool _combo;
    private bool _useSourceTiming;
    private bool _expectsDamage;

    public event Action<CSkillDataSO, EHeroSkillSoundPhase, AudioClip> OnSoundPlayed;
    public bool IsWaitingForUse { get { return _skillSound != null && !_used; } }
    public event Action OnUseStarted;
    public event Action OnUseCancelled;

    private void Awake()
    {
        _enemy = GetComponent<CEnemy>();
        _animation = GetComponent<CEnemyAttackAnimation>();
        _animator = GetComponentInChildren<Animator>(true);
        if (_enemy == null || _animation == null || _animator == null || _sounds == null
            || _preparationSource == null || _actionSource == null) enabled = false;
    }

    private void Start()
    {
        _battleTargets = FindObjectsOfType<CEntity>();
    }

    private void OnEnable()
    {
        if (_enemy == null) return;
        _enemy.OnAttackPresentationStarted += Begin;
        _enemy.OnAttackPresentationCompleted += FinishPresentation;
        _animation.OnAttackRecoveryStarted += RecoverFromAnimation;
        _enemy.OnAttackPresentationInterrupted += Interrupt;
        _enemy.OnActionCompleted += End;
        _enemy.OnActionInterrupted += Interrupt;
    }

    private void Begin(CEntity actor)
    {
        End(actor);
        _skill = _animation.ActiveSkill;
        if (_skill == null) _skill = _enemy.ActionSkill;
        CActorCombatSound actorSound = _sounds.GetActor(_enemy.EntityData);
        _skillSound = actorSound != null ? actorSound.GetSkill(_skill) : null;
        _prepared = false;
        _used = false;
        _recovered = false;
        _hasMiss = false;
        _hasHit = false;
        _useSourceTiming = _animation.UsesSourceTiming;
        _expectsDamage = false;
        if (_skill != null && _skill.EffectList != null)
        {
            for (int i = 0; i < _skill.EffectList.Count; i++)
            {
                CEffectSO effect = _skill.EffectList[i];
                if (effect != null && effect.EffectType == EEffectType.Damage
                    && effect.EffectTarget != EEffectTarget.Self) _expectsDamage = true;
            }
        }
        _combo = _enemy.ActionTarget != null && _enemy.ActionTarget.TokenManager != null
            && _enemy.ActionTarget.TokenManager.HasToken(EEffectType.Combo);
        IReadOnlyList<CEntity> targets = _battleTargets;
        for (int i = 0; targets != null && i < targets.Count; i++)
        {
            CEntity target = targets[i];
            if (target == null || _targets.Contains(target)) continue;
            _targets.Add(target);
            target.OnHealthEffectResolved += HandleResult;
        }

        if (_skillSound != null && _useSourceTiming)
        {
            _prepared = true;
            PlayUse();
            if (_animation.TryGetAttackRecovery(out _, out _)) Recover(actor);
        }
    }

    private void Update()
    {
        if (!_animation.IsAttacking || _animator.speed <= 0.0f) return;
        if (_skillSound == null && !_enemy.IsActionActive && _enemy.RiposteSkill != null)
        {
            _skill = _enemy.RiposteSkill;
            CActorCombatSound actor = _sounds.GetActor(_enemy.EntityData);
            _skillSound = actor != null ? actor.GetSkill(_skill) : null;
        }
        if (_skillSound == null) return;
        if (_useSourceTiming)
        {
            _prepared = true;
            PlayUse();
            if (_animation.TryGetAttackRecovery(out _, out _)) Recover(_enemy);
            return;
        }
        if (!_prepared)
        {
            _prepared = true;
            Play(_preparationSource, EHeroSkillSoundPhase.Preparation,
                _combo && _skillSound.ComboPreparation != null ? _skillSound.ComboPreparation : _skillSound.Preparation);
        }
        CEnemySkillFeedbackSO feedback = _animation.GetFeedback(_skill);
        float releaseTime = feedback != null ? feedback.ReleaseTime : 0.0f;
        if (!_used && _animation.TryGetAttackRecovery(out float progress, out _) && progress >= releaseTime)
        {
            PlayUse();
        }
    }

    private void LateUpdate()
    {
        if (!_useSourceTiming || _used || !_animation.IsAttacking) return;
        // 여러 대상의 판정이 모인 뒤 같은 프레임에 명중음이나 빗나감음을 고름
        PlayUse();
        if (_used && _animation.TryGetAttackRecovery(out _, out _)) Recover(_enemy);
    }

    private void PlayUse()
    {
        if (_used || _skillSound == null) return;
        if (_expectsDamage && !_hasHit && !_hasMiss) return;
        _used = true;
        _preparationSource.Stop();
        bool missed = _hasMiss && !_hasHit;
        AudioClip clip = missed ? _skillSound.Miss : _skillSound.Use;
        if (_combo)
        {
            AudioClip comboClip = missed ? _skillSound.ComboMiss : _skillSound.ComboUse;
            if (comboClip != null) clip = comboClip;
        }
        Play(_actionSource, missed ? EHeroSkillSoundPhase.Miss : EHeroSkillSoundPhase.Use, clip);
        OnUseStarted?.Invoke();
    }

    private void RecoverFromAnimation()
    {
        if (_useSourceTiming) Recover(_enemy);
    }

    private void FinishPresentation(CEntity actor)
    {
        _expectsDamage = false;
        Recover(actor);
        if (!_enemy.IsActionActive) End(actor);
    }

    private void HandleResult(CHealthEffectResult result)
    {
        if (result.Source != _enemy || result.IsPeriodic || result.IsHealing) return;
        if (result.IsMiss) _hasMiss = true;
        else _hasHit = true;
    }

    private void Recover(CEntity actor)
    {
        if (_skillSound == null || _recovered) return;
        // 복귀가 바로 시작되는 스킬도 명중 결과를 받은 뒤 소리를 고름
        if (_useSourceTiming && _expectsDamage && !_hasMiss && !_hasHit) return;
        _recovered = true;
        _preparationSource.Stop();
        if (!_used) PlayUse();
        AudioClip clip = _combo && _skillSound.ComboReturn != null ? _skillSound.ComboReturn : _skillSound.Return;
        Play(_actionSource, EHeroSkillSoundPhase.Return, clip);
    }

    private void Play(AudioSource source, EHeroSkillSoundPhase phase, AudioClip clip)
    {
        if (clip == null || source == null || !source.isActiveAndEnabled) return;
        source.PlayOneShot(clip, _sounds.Volume);
        OnSoundPlayed?.Invoke(_skill, phase, clip);
    }

    private void End(CEntity actor)
    {
        if (_skillSound != null && !_used) OnUseCancelled?.Invoke();
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] != null) _targets[i].OnHealthEffectResolved -= HandleResult;
        }
        _targets.Clear();
        if (_preparationSource != null) _preparationSource.Stop();
        _skillSound = null;
        _skill = null;
        _useSourceTiming = false;
        _expectsDamage = false;
    }

    private void Interrupt(CEntity actor)
    {
        End(actor);
        if (_actionSource != null) _actionSource.Stop();
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackPresentationStarted -= Begin;
            _enemy.OnAttackPresentationCompleted -= FinishPresentation;
            if (_animation != null) _animation.OnAttackRecoveryStarted -= RecoverFromAnimation;
            _enemy.OnAttackPresentationInterrupted -= Interrupt;
            _enemy.OnActionCompleted -= End;
            _enemy.OnActionInterrupted -= Interrupt;
        }
        Interrupt(_enemy);
    }
}
