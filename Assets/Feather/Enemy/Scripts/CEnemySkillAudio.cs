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
        _enemy.OnAttackPresentationCompleted += Recover;
        _enemy.OnAttackPresentationInterrupted += Interrupt;
        _enemy.OnActionCompleted += End;
        _enemy.OnActionInterrupted += Interrupt;
    }

    private void Begin(CEntity actor)
    {
        End(actor);
        _skill = _enemy.ActionSkill;
        CActorCombatSound actorSound = _sounds.GetActor(_enemy.EntityData);
        _skillSound = actorSound != null ? actorSound.GetSkill(_skill) : null;
        _prepared = false;
        _used = false;
        _recovered = false;
        _hasMiss = false;
        _hasHit = false;
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
            _used = true;
            _preparationSource.Stop();
            Play(_actionSource, EHeroSkillSoundPhase.Use,
                _combo && _skillSound.ComboUse != null ? _skillSound.ComboUse : _skillSound.Use);
            OnUseStarted?.Invoke();
        }
    }

    private void HandleResult(CHealthEffectResult result)
    {
        if (result.Source != _enemy || result.IsPeriodic) return;
        if (result.IsMiss) _hasMiss = true;
        else _hasHit = true;
    }

    private void Recover(CEntity actor)
    {
        if (_skillSound == null || _recovered) return;
        _recovered = true;
        _preparationSource.Stop();
        bool missed = _hasMiss && !_hasHit;
        AudioClip clip = missed ? _skillSound.Miss : _skillSound.Return;
        if (_combo)
        {
            AudioClip comboClip = missed ? _skillSound.ComboMiss : _skillSound.ComboReturn;
            if (comboClip != null) clip = comboClip;
        }
        Play(_actionSource, missed ? EHeroSkillSoundPhase.Miss : EHeroSkillSoundPhase.Return, clip);
        if (!_enemy.IsActionActive) End(actor);
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
            _enemy.OnAttackPresentationCompleted -= Recover;
            _enemy.OnAttackPresentationInterrupted -= Interrupt;
            _enemy.OnActionCompleted -= End;
            _enemy.OnActionInterrupted -= Interrupt;
        }
        Interrupt(_enemy);
    }
}
