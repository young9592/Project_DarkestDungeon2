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
    private bool _useQueued;
    private bool _returnRequested;
    private readonly CSkillAudioPlayback _playback = new CSkillAudioPlayback();

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
        _enemy.OnHitPresentationStarted += Interrupt;
    }

    private void Begin(CEntity actor)
    {
        End(actor);
        _playback.CancelAll();
        _useQueued = false;
        _returnRequested = false;
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
            PlayPreparation(
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
        if (_enemy.IsDead) { Interrupt(_enemy); return; }
        if (_useSourceTiming && !_used && _animation.IsAttacking && _animator.speed > 0.0f)
        {
            // 여러 대상의 판정이 모인 뒤 명중음이나 빗나감음을 고름
            PlayUse();
            if (_used && _animation.TryGetAttackRecovery(out _, out _)) Recover(_enemy);
        }
        _playback.Tick(Time.deltaTime);
    }

    private void PlayPreparation(AudioClip clip)
    {
        CEnemySkillSound sound = _skillSound;
        CSkillDataSO skill = _skill;
        float delay = sound.AudioTiming != null ? sound.AudioTiming.GetDelay(EHeroSkillSoundPhase.Preparation) : 0.0f;
        _playback.Schedule(EHeroSkillSoundPhase.Preparation, delay,
            () => Play(_preparationSource, skill, EHeroSkillSoundPhase.Preparation, clip, sound.Volume));
    }

    private void PlayUse()
    {
        if (_used || _useQueued || _skillSound == null) return;
        if (_expectsDamage && !_hasHit && !_hasMiss) return;
        bool missed = _hasMiss && !_hasHit;
        EHeroSkillSoundPhase phase = missed ? EHeroSkillSoundPhase.Miss : EHeroSkillSoundPhase.Use;
        CEnemySkillSound sound = _skillSound;
        CSkillDataSO skill = _skill;
        AudioClip clip = missed ? sound.Miss : sound.Use;
        if (_combo)
        {
            AudioClip comboClip = missed ? sound.ComboMiss : sound.ComboUse;
            if (comboClip != null) clip = comboClip;
        }
        float delay = sound.AudioTiming != null ? sound.AudioTiming.GetDelay(phase) : 0.0f;
        _useQueued = true;
        _playback.Cancel(EHeroSkillSoundPhase.Preparation);
        _preparationSource.Stop();
        _playback.Schedule(phase, delay, () =>
        {
            _useQueued = false;
            if (_skillSound != sound || !isActiveAndEnabled || _enemy.IsDead) return;
            _used = true;
            Play(_actionSource, skill, phase, clip, sound.Volume);
            OnUseStarted?.Invoke();
            if (_returnRequested) Recover(_enemy);
        });
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
        _returnRequested = true;
        if (_useSourceTiming && _expectsDamage && !_hasMiss && !_hasHit) return;
        if (!_used) { PlayUse(); return; }
        _recovered = true;
        CEnemySkillSound sound = _skillSound;
        CSkillDataSO skill = _skill;
        AudioClip clip = _combo && sound.ComboReturn != null ? sound.ComboReturn : sound.Return;
        float delay = sound.AudioTiming != null ? sound.AudioTiming.GetDelay(EHeroSkillSoundPhase.Return) : 0.0f;
        _playback.Schedule(EHeroSkillSoundPhase.Return, delay,
            () => Play(_actionSource, skill, EHeroSkillSoundPhase.Return, clip, sound.Volume));
    }

    private void Play(AudioSource source, CSkillDataSO skill, EHeroSkillSoundPhase phase, AudioClip clip, float volume)
    {
        if (clip == null || source == null || !source.isActiveAndEnabled || _enemy.IsDead) return;
        source.PlayOneShot(clip, _sounds.Volume * volume);
        OnSoundPlayed?.Invoke(skill, phase, clip);
    }

    private void End(CEntity actor)
    {
        if (_skillSound != null && !_used) OnUseCancelled?.Invoke();
        _playback.Cancel(EHeroSkillSoundPhase.Preparation);
        _playback.Cancel(EHeroSkillSoundPhase.Use);
        _playback.Cancel(EHeroSkillSoundPhase.Miss);
        _useQueued = false;
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
        _playback.CancelAll();
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
            _enemy.OnHitPresentationStarted -= Interrupt;
        }
        Interrupt(_enemy);
    }
}
