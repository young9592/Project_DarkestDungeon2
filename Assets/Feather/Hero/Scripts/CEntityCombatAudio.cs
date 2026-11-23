using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CEntityCombatAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private CCombatAudioSO _sounds;
    [SerializeField] private AudioSource _source;
    [SerializeField, Range(0.0f, 1.0f)] private float _volumeScale = 1.0f;

    private CEntity _entity;
    private CPlayer _hero;
    private CEnemy _enemy;
    private CEntityDeathPresentation _deathPresentation;
    private bool _bound;
    private bool _deathPlayed;
    private bool _wasDeathDoor;
    [Header("Riposte")]
    [SerializeField] private string _riposteActionState;
    [SerializeField] private string _riposteRecoveryState;
    private Animator _animator;
    private CHeroAnimation _heroAnimation;
    private CHeroStressResolution _stressResolution;
    private readonly List<CEntity> _riposteTargets = new List<CEntity>();
    private bool _riposteHasHit;
    private bool _riposteHasMiss;
    private bool _riposteSoundPlayed;
    private int _lastStateHash;
    private int _typedTickFrame = -1;
    private bool _pendingRecovery;
    private int _lastDeathArmor;
    private CEntity _hurtSource;
    private CHeroSkillAudio _hurtHeroAudio;
    private CEnemySkillAudio _hurtEnemyAudio;


    public event Action<string, AudioClip> OnSoundPlayed;

    private void Awake()
    {
        _entity = GetComponent<CEntity>();
        _hero = GetComponent<CPlayer>();
        _enemy = GetComponent<CEnemy>();
        _deathPresentation = GetComponent<CEntityDeathPresentation>();
        _animator = GetComponentInChildren<Animator>(true);
        _heroAnimation = GetComponent<CHeroAnimation>();
        _stressResolution = GetComponent<CHeroStressResolution>();
        if (_entity == null || _sounds == null || _source == null) enabled = false;
    }

    private void Start()
    {
        Bind();
    }

    private void OnEnable()
    {
        if (_entity != null && _entity.TokenManager != null) Bind();
    }

    private void Bind()
    {
        if (_bound || _entity == null || _entity.TokenManager == null) return;
        _bound = true;
        _lastDeathArmor = _enemy != null ? _enemy.RemainingDeathArmor : 0;
        CApplyStatusStrategy.OnStatusEffectResisted += StatusResisted;
        _entity.OnHealthEffectResolved += HealthResolved;
        _entity.TokenManager.OnTokenAdded += TokenAdded;
        if (_hero != null)
        {
            if (_heroAnimation != null) _heroAnimation.OnRiposteQueued += BeginRiposteAudio;
            _hero.OnStressEffectResolved += StressResolved;
            _hero.OnStatusEffectApplied += StatusApplied;
            _hero.OnStatusEffectsCured += StatusCured;
            _hero.OnDeathblowResisted += DeathblowResisted;
            _hero.OnStatusEffectTicked += StatusTicked;
            _hero.OnHitPresentationCompleted += HitCompleted;
            if (_stressResolution != null) _stressResolution.OnResultStarted += StressResultStarted;

        }
        if (_enemy != null)
        {
            _enemy.OnHitPresentationCompleted += HitCompleted;
            _enemy.OnStatusEffectApplied += StatusApplied;
            _enemy.OnStatusEffectsCured += StatusCured;
            _enemy.OnStatusEffectTicked += StatusTicked;
        }
        if (_deathPresentation != null) _deathPresentation.OnDeathPresentationStarted += DeathStarted;
    }

    private CActorCombatSound GetActor()
    {
        return _sounds.GetActor(_entity.EntityData);
    }

    private void HealthResolved(CHealthEffectResult result)
    {
        if (result.IsMiss) return;
        if (_enemy != null && _enemy.RemainingDeathArmor < _lastDeathArmor) Play("armorbreak");
        if (_enemy != null) _lastDeathArmor = _enemy.RemainingDeathArmor;
        if (result.IsHealing && result.AppliedAmount > 0)
        {
            string side = _hero != null ? "ally" : "enemy";
            Play(result.IsPeriodic ? "status/hotDOT_" + side
                : "status/heal_" + side + (result.IsCritical ? "_crit" : ""));
        }
        else if (result.IsPeriodic && result.AppliedAmount > 0)
        {
            if (_typedTickFrame != Time.frameCount) Play("status/hp_loss");
        }
        else if (!_entity.IsDead && result.AppliedAmount > 0)
        {
            QueueHurt(result.Source);
        }
        if (result.IsCritical && !result.IsHealing) Play(_hero != null ? "crit_hero" : "crit_enemy");
        if (_entity.IsDeathsDoor && !_wasDeathDoor)
        {
            Play("deaths_door");
            CActorCombatSound actor = GetActor();
            if (actor != null) PlayClip("death-door", actor.DeathDoor);
        }
        _wasDeathDoor = _entity.IsDeathsDoor;
    }

    private void QueueHurt(CEntity source)
    {
        ClearPendingHurt();
        _pendingRecovery = true;
        if (source != null)
        {
            _hurtHeroAudio = source.GetComponent<CHeroSkillAudio>();
            _hurtEnemyAudio = source.GetComponent<CEnemySkillAudio>();
            bool waiting = _hurtHeroAudio != null && _hurtHeroAudio.isActiveAndEnabled && _hurtHeroAudio.IsWaitingForUse
                || _hurtEnemyAudio != null && _hurtEnemyAudio.isActiveAndEnabled && _hurtEnemyAudio.IsWaitingForUse;
            if (waiting)
            {
                // 판정은 그대로 두고 피격음만 공격 사용 시점까지 기다림
                _hurtSource = source;
                if (_hurtHeroAudio != null)
                {
                    _hurtHeroAudio.OnUseStarted += PlayPendingHurt;
                    _hurtHeroAudio.OnUseCancelled += CancelPendingUse;
                }
                if (_hurtEnemyAudio != null)
                {
                    _hurtEnemyAudio.OnUseStarted += PlayPendingHurt;
                    _hurtEnemyAudio.OnUseCancelled += CancelPendingUse;
                }
                source.OnActionInterrupted += CancelPendingHurt;
                source.OnActionCompleted += CancelPendingHurt;
                return;
            }
        }
        PlayPendingHurt();
    }

    private void PlayPendingHurt()
    {
        ClearPendingHurt();
        if (!isActiveAndEnabled || _entity.IsDead) return;
        CActorCombatSound actor = GetActor();
        if (actor != null) PlayClip("hurt", actor.Hurt);
    }

    private void CancelPendingHurt(CEntity actor)
    {
        CancelPendingUse();
    }

    private void CancelPendingUse()
    {
        ClearPendingHurt();
        _pendingRecovery = false;
    }

    private void ClearPendingHurt()
    {
        if (_hurtHeroAudio != null)
        {
            _hurtHeroAudio.OnUseStarted -= PlayPendingHurt;
            _hurtHeroAudio.OnUseCancelled -= CancelPendingUse;
        }
        if (_hurtEnemyAudio != null)
        {
            _hurtEnemyAudio.OnUseStarted -= PlayPendingHurt;
            _hurtEnemyAudio.OnUseCancelled -= CancelPendingUse;
        }
        if (_hurtSource != null)
        {
            _hurtSource.OnActionInterrupted -= CancelPendingHurt;
            _hurtSource.OnActionCompleted -= CancelPendingHurt;
        }
        _hurtSource = null;
        _hurtHeroAudio = null;
        _hurtEnemyAudio = null;
    }

    public void PlayTokenConsumed(EEffectType type)
    {
        if (!isActiveAndEnabled || _entity.IsDead) return;
        switch (type)
        {
            case EEffectType.Daze: Play("token/daze_consume"); break;
            case EEffectType.Stun: Play("token/stun_consume"); break;
            case EEffectType.Speed: Play("token/speed_consume"); break;
        }
    }

    private void BeginRiposteAudio()
    {
        ClearRiposteAudio();
        CEntity[] targets = FindObjectsOfType<CEntity>();
        for (int i = 0; targets != null && i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            _riposteTargets.Add(targets[i]);
            targets[i].OnHealthEffectResolved += RiposteResolved;
        }
    }

    private void RiposteResolved(CHealthEffectResult result)
    {
        if (_heroAnimation == null || !_heroAnimation.IsRiposting
            || result.Source != _hero || result.IsPeriodic || result.IsHealing) return;
        if (result.IsMiss) _riposteHasMiss = true;
        else _riposteHasHit = true;
    }

    private void ClearRiposteAudio()
    {
        for (int i = 0; i < _riposteTargets.Count; i++)
        {
            if (_riposteTargets[i] != null) _riposteTargets[i].OnHealthEffectResolved -= RiposteResolved;
        }
        _riposteTargets.Clear();
        _riposteHasHit = false;
        _riposteHasMiss = false;
        _riposteSoundPlayed = false;
        _lastStateHash = 0;
    }

    private void Update()
    {
        if (_hero == null || _animator == null || _heroAnimation == null) return;
        if (!_heroAnimation.IsRiposting)
        {
            if (_riposteTargets.Count > 0) ClearRiposteAudio();
            return;
        }
        if (_animator.speed <= 0.0f || string.IsNullOrEmpty(_riposteActionState)
            || _animator.IsInTransition(0)) return;
        int hash = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if (hash == Animator.StringToHash(_riposteActionState))
        {
            if (_riposteSoundPlayed || (!_riposteHasHit && !_riposteHasMiss)) return;
            _riposteSoundPlayed = true;
            Play(_riposteHasMiss && !_riposteHasHit ? "riposte_miss" : "riposte");
        }
        else if (hash == Animator.StringToHash(_riposteRecoveryState) && hash != _lastStateHash)
        {
            CActorCombatSound actor = GetActor();
            if (actor != null) PlayClip("riposte-return", actor.RiposteReturn);
        }
        _lastStateHash = hash;
    }

    private void StatusResisted(CEntity target, EEffectType type)
    {
        if (target != _entity || _entity.IsDead) return;
        Play("status/resist");
    }

    private void StatusCured()
    {
        Play("status/cure");
    }

    private void DeathblowResisted()
    {
        Play("status/resist");
    }

    private void StatusApplied(EEffectType type)
    {
        switch (type)
        {
            case EEffectType.Bleed: Play("status/bleedonset"); break;
            case EEffectType.Blight: Play("status/blightonset"); break;
            case EEffectType.Burn: Play("status/burnonset"); break;
            case EEffectType.Horror: Play("status/horroronset"); break;
            case EEffectType.Regen: Play(_hero != null ? "status/hotonset_ally" : "status/hotonset_enemy"); break;
        }
    }

    private void StatusTicked(EEffectType type)
    {
        switch (type)
        {
            case EEffectType.Bleed: PlayDetached("status/bleedDOT", _sounds.GetSound("status/bleedDOT")); break;
            case EEffectType.Blight: PlayDetached("status/blightDOT", _sounds.GetSound("status/blightDOT")); break;
            case EEffectType.Burn: PlayDetached("status/burnDOT", _sounds.GetSound("status/burnDOT")); break;
            default: return;
        }
        _typedTickFrame = Time.frameCount;
    }

    private void HitCompleted(CEntity actor)
    {
        if (!_pendingRecovery || _entity.IsDead) return;
        _pendingRecovery = false;
        CActorCombatSound sound = GetActor();
        if (sound != null) PlayClip("hit-recovery", sound.Recovery);
    }

    private void StressResolved(CPlayer hero, int appliedAmount, bool isHorror)
    {
        if (appliedAmount == 0) return;
        Play(isHorror && appliedAmount > 0 ? "status/horrorDOT"
            : appliedAmount > 0 ? "status/stressup" : "status/stressdown");
    }

    private void TokenAdded(EEffectType type)
    {
        if (_entity.IsDead) return;
        string key = null;
        switch (type)
        {
            case EEffectType.Block: key = "block"; break;
            case EEffectType.BlockPlus: key = "block_plus"; break;
            case EEffectType.Dodge: key = "dodge"; break;
            case EEffectType.DodgePlus: key = "dodge_plus"; break;
            case EEffectType.Crit: key = "crit"; break;
            case EEffectType.Guard: key = "guard"; break;
            case EEffectType.Blind: key = "blind"; break;
            case EEffectType.Speed: key = "speed"; break;
            case EEffectType.Strength: key = "strength"; break;
            case EEffectType.Stealth: key = "stealth"; break;
            case EEffectType.Daze: key = "daze"; break;
            case EEffectType.Stun: key = "stun"; break;
            case EEffectType.Taunt: key = "taunt"; break;
            case EEffectType.Immobilize: key = "immobilize"; break;
            case EEffectType.Combo: key = "combo"; break;
            case EEffectType.DeathArmor: key = "deathsdoor_armor"; break;
            case EEffectType.Weak:
            case EEffectType.Vulnerable: key = "general_debuff"; break;
            case EEffectType.Riposte: Play("status/riposte"); return;
            case EEffectType.DenialOfArms:
            case EEffectType.DenialOfReach:
            case EEffectType.DenialOfFortitude:
            case EEffectType.DenialOfReason: key = "healing_block"; break;
        }
        if (key != null) Play("token/" + key);
    }

    private void DeathStarted(CEntity actor)
    {
        if (_deathPlayed) return;
        _deathPlayed = true;
        ClearPendingHurt();
        _pendingRecovery = false;
        _source.Stop();
        CActorCombatSound sound = GetActor();
        PlayDetached("deathblow", _sounds.GetSound("deathblow"));
        if (sound == null) return;
        PlayDetached("death", sound.Death);
        PlayDetached("death-foley", sound.DeathFoley);
    }

    private void PlayDetached(string key, AudioClip clip)
    {
        if (clip == null) return;
        // 사망음과 틱 소리는 시체로 바뀌어도 끝까지 재생함
        GameObject sound = new GameObject("Combat Audio Tail");
        AudioSource source = sound.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0.0f;
        source.clip = clip;
        source.volume = _sounds.Volume * _volumeScale;
        source.Play();
        Destroy(sound, clip.length + 0.1f);
        OnSoundPlayed?.Invoke(key, clip);
    }

    private void StressResultStarted(CPlayer hero, EHeroStressResult result)
    {
        Play(result == EHeroStressResult.Meltdown ? "status/meltdown" : "status/resolute");
    }

    private void Play(string key)
    {
        PlayClip(key, _sounds.GetSound(key));
    }

    private void PlayClip(string key, AudioClip clip)
    {
        if (clip == null || _source == null || !_source.isActiveAndEnabled) return;
        _source.PlayOneShot(clip, _sounds.Volume * _volumeScale);
        OnSoundPlayed?.Invoke(key, clip);
    }

    private void OnDisable()
    {
        ClearPendingHurt();
        ClearRiposteAudio();
        if (_bound)
        {
            CApplyStatusStrategy.OnStatusEffectResisted -= StatusResisted;
            _entity.OnHealthEffectResolved -= HealthResolved;
            _entity.TokenManager.OnTokenAdded -= TokenAdded;
            if (_hero != null)
            {
                if (_heroAnimation != null) _heroAnimation.OnRiposteQueued -= BeginRiposteAudio;
                _hero.OnStressEffectResolved -= StressResolved;
                _hero.OnStatusEffectApplied -= StatusApplied;
                _hero.OnStatusEffectsCured -= StatusCured;
                _hero.OnDeathblowResisted -= DeathblowResisted;
                _hero.OnStatusEffectTicked -= StatusTicked;
                _hero.OnHitPresentationCompleted -= HitCompleted;
                if (_stressResolution != null) _stressResolution.OnResultStarted -= StressResultStarted;

            }
            if (_enemy != null)
            {
                _enemy.OnHitPresentationCompleted -= HitCompleted;
                _enemy.OnStatusEffectApplied -= StatusApplied;
                _enemy.OnStatusEffectsCured -= StatusCured;
                _enemy.OnStatusEffectTicked -= StatusTicked;
            }
            if (_deathPresentation != null) _deathPresentation.OnDeathPresentationStarted -= DeathStarted;
        }
        _bound = false;
        if (_source != null) _source.Stop();
        _pendingRecovery = false;
    }
}
