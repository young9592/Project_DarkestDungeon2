using System;
using System.Collections.Generic;
using UnityEngine;

public enum EHeroSkillSoundPhase
{
    Preparation,
    Use,
    Miss,
    Return
}

[DisallowMultipleComponent]
public class CHeroSkillAudio : MonoBehaviour
{
    [Header("Skill Audio")]
    [SerializeField] private AudioSource _preparationSource;
    [SerializeField] private AudioSource _preparationLoopSource;
    [SerializeField] private AudioSource _actionSource;

    private CPlayer _hero;
    private CHeroAnimation _animation;
    private Animator _animator;
    private CHeroSkillSO _preparedSkill;
    private CHeroSkillSO _activeSkill;
    private readonly List<CEntity> _targets = new List<CEntity>();
    private bool _preparationPlayed;
    private bool _usePlayed;
    private bool _recoveryPlayed;
    private bool _hasMiss;
    private bool _hasHit;
    private bool _waitForHitResult;

    public event Action<CSkillDataSO, EHeroSkillSoundPhase, AudioClip> OnSoundPlayed;
    public bool IsWaitingForUse { get { return _activeSkill != null && !_usePlayed; } }
    public event Action OnUseStarted;
    public event Action OnUseCancelled;

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _animation = GetComponent<CHeroAnimation>();
        _animator = GetComponentInChildren<Animator>(true);
        if (_hero == null || _animation == null || _animator == null
            || _preparationSource == null || _preparationLoopSource == null || _actionSource == null)
        {
            Debug.LogWarning(name + ": 영웅 스킬 사운드 연결을 확인해야 합니다.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_hero == null) return;
        _hero.OnAttackPresentationStarted += Begin;
        _hero.OnAttackPresentationCompleted += FinishPresentation;
        _hero.OnActionCompleted += End;
        _hero.OnActionInterrupted += Interrupt;
        _hero.OnAttackPresentationInterrupted += Interrupt;
        _hero.OnHitPresentationStarted += Interrupt;
    }

    private static CHeroSkillSO GetSoundSkill(CSkillDataSO skill)
    {
        return CHeroSkillSO.GetBaseSkill(skill) as CHeroSkillSO;
    }

    private void Update()
    {
        if (_hero == null || _animation == null || _animator == null) return;
        if (_activeSkill == null)
        {
            CHeroSkillSO selected = GetSoundSkill(_hero.SelectedSkill);
            if (selected != null && selected.Feedback != null)
            {
                Prepare(selected);
                if (_animation.IsSelecting && _animator.speed > 0.0f) PlayPreparation();
            }
            else if (_preparedSkill != null
                && GetSoundSkill(_hero.ActionSkill) != _preparedSkill)
            {
                ClearPreparation();
            }
            return;
        }

        if (!_animation.IsAttacking || _animator.speed <= 0.0f) return;
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        int startHash = Animator.StringToHash(_activeSkill.StartState);
        if (!_preparationPlayed && state.fullPathHash == startHash) PlayPreparation();

        if (!_usePlayed && _animation.TryGetAttackProgress(out _, out _))
        {
            PlayUse();
        }

        if (_activeSkill.ExecutionStartState != _activeSkill.EndState
            && _animation.TryGetAttackRecovery(out _, out _))
        {
            if (!_usePlayed) PlayUse();
            PlayRecovery();
        }
    }

    private void Prepare(CHeroSkillSO skill)
    {
        if (_preparedSkill == skill) return;
        ClearPreparation();
        _preparedSkill = skill;
    }

    private void PlayPreparation()
    {
        if (_preparationPlayed || _preparedSkill == null || _preparedSkill.Feedback == null) return;
        _preparationPlayed = true;
        CHeroSkillFeedbackSO feedback = _preparedSkill.Feedback;
        AudioClip clip = feedback.PreparationSound;
        if (clip == null) return;

        double start = AudioSettings.dspTime + 0.02;
        _preparationSource.clip = clip;
        _preparationSource.volume = feedback.Volume;
        _preparationSource.loop = false;
        _preparationSource.PlayScheduled(start);
        if (feedback.PreparationLoopSound != null)
        {
            _preparationLoopSource.clip = feedback.PreparationLoopSound;
            _preparationLoopSource.volume = feedback.Volume;
            _preparationLoopSource.loop = true;
            _preparationLoopSource.PlayScheduled(start + clip.length);
        }
        OnSoundPlayed?.Invoke(_preparedSkill, EHeroSkillSoundPhase.Preparation, clip);
    }

    private void Begin(CEntity actor)
    {
        ClearTargets();
        _activeSkill = GetSoundSkill(_hero.ActionSkill);
        _usePlayed = false;
        _recoveryPlayed = false;
        _hasMiss = false;
        _hasHit = false;
        if (_activeSkill == null || _activeSkill.Feedback == null)
        {
            _activeSkill = null;
            ClearPreparation();
            return;
        }

        _waitForHitResult = false;
        if (_activeSkill.EffectList != null)
        {
            for (int i = 0; i < _activeSkill.EffectList.Count; i++)
            {
                CEffectSO effect = _activeSkill.EffectList[i];
                if (effect != null && effect.EffectType == EEffectType.Damage
                    && effect.Strategy is CDamageStrategy && effect.EffectTarget != EEffectTarget.Self) _waitForHitResult = true;
            }
        }
        Prepare(_activeSkill);
        // 보호자가 대신 맞아도 실제 피해 결과를 받음
        CEntity[] targets = FindObjectsOfType<CEntity>();
        for (int i = 0; targets != null && i < targets.Length; i++)
        {
            CEntity target = targets[i];
            if (target == null || _targets.Contains(target)) continue;
            _targets.Add(target);
            target.OnHealthEffectResolved += HandleResult;
        }
    }

    private void HandleResult(CHealthEffectResult result)
    {
        if (_activeSkill == null || result.Source != _hero || result.IsPeriodic || result.IsHealing) return;
        if (result.IsMiss) _hasMiss = true;
        else _hasHit = true;
    }

    private void PlayUse()
    {
        if (_usePlayed || _activeSkill == null) return;
        if (_waitForHitResult && !_hasHit && !_hasMiss) return;
        _usePlayed = true;
        StopPreparation();
        bool missed = _hasMiss && !_hasHit;
        Play(_actionSource, _activeSkill, missed ? EHeroSkillSoundPhase.Miss : EHeroSkillSoundPhase.Use,
            missed ? _activeSkill.Feedback.MissSound : _activeSkill.Feedback.UseSound);
        OnUseStarted?.Invoke();
    }

    private void PlayRecovery()
    {
        if (_recoveryPlayed || _activeSkill == null) return;
        _recoveryPlayed = true;
        Play(_actionSource, _activeSkill, EHeroSkillSoundPhase.Return, _activeSkill.Feedback.ReturnSound);
    }

    private void FinishPresentation(CEntity actor)
    {
        if (_activeSkill == null) return;
        _waitForHitResult = false;
        if (!_usePlayed) PlayUse();
        // 그림자 은신처럼 별도 복귀 상태가 없으면 동작이 끝날 때 재생
        PlayRecovery();
    }

    private void Play(AudioSource source, CHeroSkillSO skill, EHeroSkillSoundPhase phase, AudioClip clip)
    {
        if (source == null || !source.isActiveAndEnabled || clip == null || skill.Feedback == null) return;
        source.PlayOneShot(clip, skill.Feedback.Volume);
        OnSoundPlayed?.Invoke(skill, phase, clip);
    }

    private void ClearTargets()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] != null) _targets[i].OnHealthEffectResolved -= HandleResult;
        }
        _targets.Clear();
    }

    private void ClearPreparation()
    {
        StopPreparation();
        _preparedSkill = null;
        _preparationPlayed = false;
    }

    private void StopPreparation()
    {
        if (_preparationSource != null) _preparationSource.Stop();
        if (_preparationLoopSource != null) _preparationLoopSource.Stop();
    }

    private void End(CEntity actor)
    {
        if (_activeSkill != null && !_usePlayed) OnUseCancelled?.Invoke();
        ClearTargets();
        ClearPreparation();
        _activeSkill = null;
        _waitForHitResult = false;
    }

    private void Interrupt(CEntity actor)
    {
        End(actor);
        if (_actionSource != null) _actionSource.Stop();
    }

    private void OnDisable()
    {
        if (_hero != null)
        {
            _hero.OnAttackPresentationStarted -= Begin;
            _hero.OnAttackPresentationCompleted -= FinishPresentation;
            _hero.OnActionCompleted -= End;
            _hero.OnActionInterrupted -= Interrupt;
            _hero.OnAttackPresentationInterrupted -= Interrupt;
            _hero.OnHitPresentationStarted -= Interrupt;
        }
        Interrupt(_hero);
    }
}
