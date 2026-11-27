using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public class CBattleAudio : MonoBehaviour
{
    [Header("Battle")]
    [SerializeField] private CCombatManager _combatManager;
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private CBattleAudioSO _sounds;
    [Header("Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _cueSource;

    private CHeroParty _heroParty;
    private CEnemyParty _enemyParty;
    private readonly List<CEntity> _actors = new List<CEntity>();
    private bool _battleStarted;
    private bool _battleFinished;
    private bool _musicStarted;
    private bool _isBossMusic;
    private int _lastRound;
    private CEntity _lastTurnActor;
    private readonly Dictionary<CEntity, Action<EEffectType, int, int>> _tokenCallbacks
        = new Dictionary<CEntity, Action<EEffectType, int, int>>();
    private int _lastTurnFrame = -1;
    private bool _turnCueAwaitingBegin;
    private CEntity _interruptedActor;
    private int _interruptedFrame = -1;
    private readonly List<CTokenConsumption> _pendingTokens = new List<CTokenConsumption>();

    private class CTokenConsumption
    {
        public CEntity Actor;
        public EEffectType Type;
        public int Round;
        public bool EnemyStun;
    }

    public bool IsBossMusic { get { return _isBossMusic; } }
    public event Action<string, int, CEntity> OnCuePlayed;
    public event Action<AudioClip> OnMusicStarted;

    private void Awake()
    {
        _heroParty = FindObjectOfType<CHeroParty>();
        _enemyParty = FindObjectOfType<CEnemyParty>();
        if (_combatManager == null || _combatUI == null || _sounds == null
            || _heroParty == null || _enemyParty == null
            || _musicSource == null || _cueSource == null)
        {
            Debug.LogWarning(name + ": 전투 사운드 연결 확인이 필요함", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (_battleFinished) return;
        BindActors();
        if (!_battleStarted)
        {
            if (!_heroParty.IsReady || !_enemyParty.IsReady
                || _combatManager.HeroParty.Count == 0 || _combatManager.EnemyParty.Count == 0) return;
            BeginBattle();
        }

        if (!HasLivingHero() || !HasLivingEnemy())
        {
            _battleFinished = true;
            PlayCue("BattleEnd", _sounds.BattleEnd, null);
            _musicSource.Stop();
            _musicStarted = false;
            return;
        }

        if (!_musicStarted) StartMusic();
        SyncRound();
    }

    private void BindActors()
    {
        IReadOnlyList<CPlayer> heroes = _combatManager.HeroParty;
        for (int i = 0; heroes != null && i < heroes.Count; i++) BindActor(heroes[i]);
        IReadOnlyList<CEnemy> enemies = _combatManager.EnemyParty;
        for (int i = 0; enemies != null && i < enemies.Count; i++) BindActor(enemies[i]);
    }

    private void BindActor(CEntity actor)
    {
        if (actor == null || _actors.Contains(actor)) return;
        _actors.Add(actor);
        actor.OnActionStarted += TurnStarted;
        actor.OnActionInterrupted += ActionInterrupted;
        if (actor.TokenManager != null)
        {
            Dictionary<EEffectType, int> stacks = new Dictionary<EEffectType, int>();
            EEffectType[] types = { EEffectType.Stun, EEffectType.Daze, EEffectType.Speed };
            for (int i = 0; i < types.Length; i++) stacks[types[i]] = actor.TokenManager.GetTokenStack(types[i]);
            Action<EEffectType, int, int> callback = (type, stack, remainingTurns) =>
            {
                if (!stacks.TryGetValue(type, out int previous)) return;
                stacks[type] = stack;
                if (stack == previous - 1) TokenChanged(actor, type, stack);
            };
            _tokenCallbacks.Add(actor, callback);
            actor.TokenManager.OnTokenChanged += callback;
        }
    }

    private void BeginBattle()
    {
        _battleStarted = true;
        _isBossMusic = false;
        IReadOnlyList<CEnemy> enemies = _combatManager.EnemyParty;
        for (int i = 0; enemies != null && i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].TryGetComponent(out CDenialLock denial))
            {
                _isBossMusic = true;
                break;
            }
        }
        StartMusic();
        PlayCue("BattleStart", _sounds.BattleStart, null);
    }

    private void StartMusic()
    {
        AudioClip clip = _isBossMusic ? _sounds.DenialMusic : _sounds.CityMusic;
        if (clip == null) return;
        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = _sounds.MusicVolume;
        _musicSource.PlayScheduled(AudioSettings.dspTime + 0.05);
        _musicStarted = true;
        OnMusicStarted?.Invoke(clip);
    }

    private void SyncRound()
    {
        int round = _combatUI.Round;
        if (round <= 0 || round == _lastRound) return;
        _lastRound = round;
        PlayCue("RoundStart", _sounds.RoundStart, null);
    }

    private void TurnStarted(CEntity actor)
    {
        if (_battleFinished || actor == null || actor.IsDead) return;
        // 같은 차례에서 UI를 다시 여는 취소·재선택은 새 턴이 아님
        if (_lastTurnActor == actor && _interruptedActor == actor && _interruptedFrame == Time.frameCount) return;
        if (!_battleStarted) BeginBattle();
        SyncRound();
        if (_turnCueAwaitingBegin && _lastTurnActor == actor)
        {
            _turnCueAwaitingBegin = false;
            return;
        }
        PlayTurn(actor, false);
    }

    private void ActionInterrupted(CEntity actor)
    {
        _interruptedActor = actor;
        _interruptedFrame = Time.frameCount;
    }

    private void TokenChanged(CEntity actor, EEffectType type, int stack)
    {
        if (_battleFinished || actor == null || actor.IsDead) return;
        bool startingTurn = actor is CPlayer hero && hero.IsStartingTurn
            || actor is CEnemy startingEnemy && startingEnemy.IsStartingTurn;
        if (type == EEffectType.Daze && startingTurn) return;
        if (type != EEffectType.Speed && (actor != _combatManager.MyTurn
            || actor.ActionState != EEntityActionState.Idle)) return;
        _pendingTokens.Add(new CTokenConsumption
        {
            Actor = actor,
            Type = type,
            Round = _combatUI.Round,
            EnemyStun = type == EEffectType.Stun && actor is CEnemy enemy && enemy.IsStartingTurn
        });
    }

    private void LateUpdate()
    {
        for (int i = 0; i < _pendingTokens.Count; i++)
        {
            CTokenConsumption pending = _pendingTokens[i];
            CEntity actor = pending.Actor;
            if (_battleFinished || actor == null || actor.IsDead) continue;
            // 만료·초기화와 구분해서 실제 순서 변경이 끝난 소모만 재생함
            bool consumed = pending.Type == EEffectType.Speed
                ? _combatUI.Round > pending.Round
                : _combatManager.MyTurn != actor || pending.EnemyStun;
            if (!consumed) continue;
            if (actor.TryGetComponent(out CEntityCombatAudio audio)) audio.PlayTokenConsumed(pending.Type);
            if (pending.Type == EEffectType.Speed) continue;
            if (_lastTurnActor == actor && _lastTurnFrame == Time.frameCount) continue;
            if (!_battleStarted) BeginBattle();
            SyncRound();
            PlayTurn(actor, true);
        }
        _pendingTokens.Clear();
    }

    private void PlayTurn(CEntity actor, bool awaitingBegin)
    {
        _lastTurnActor = actor;
        _lastTurnFrame = Time.frameCount;
        _turnCueAwaitingBegin = awaitingBegin;
        PlayCue(actor is CPlayer ? "HeroTurn" : "EnemyTurn",
            actor is CPlayer ? _sounds.HeroTurn : _sounds.EnemyTurn, actor);
    }

    private void PlayCue(string cue, AudioClip clip, CEntity actor)
    {
        if (clip == null || !_cueSource.isActiveAndEnabled) return;
        _cueSource.PlayOneShot(clip, _sounds.CueVolume);
        OnCuePlayed?.Invoke(cue, _combatUI.Round, actor);
    }

    private bool HasLivingHero()
    {
        IReadOnlyList<CPlayer> heroes = _combatManager.HeroParty;
        for (int i = 0; heroes != null && i < heroes.Count; i++)
        {
            if (heroes[i] != null && !heroes[i].IsDead) return true;
        }
        return false;
    }

    private bool HasLivingEnemy()
    {
        IReadOnlyList<CEnemy> enemies = _combatManager.EnemyParty;
        for (int i = 0; enemies != null && i < enemies.Count; i++)
        {
            if (enemies[i] != null && !enemies[i].IsDead) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        for (int i = 0; i < _actors.Count; i++)
        {
            CEntity actor = _actors[i];
            if (actor == null) continue;
            actor.OnActionStarted -= TurnStarted;
            actor.OnActionInterrupted -= ActionInterrupted;
            if (actor.TokenManager != null && _tokenCallbacks.TryGetValue(actor, out Action<EEffectType, int, int> callback))
                actor.TokenManager.OnTokenChanged -= callback;
        }
        _actors.Clear();
        _pendingTokens.Clear();
        _tokenCallbacks.Clear();
        if (_musicSource != null) _musicSource.Stop();
        if (_cueSource != null) _cueSource.Stop();
        _musicStarted = false;
    }
}
