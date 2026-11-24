using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroExpeditionState : MonoBehaviour
{
    [Header("Expedition")]
    [SerializeField] private CHeroMasteryCatalogSO _masteryCatalog;

    private CHeroMasteryService _mastery;
    private CHeroBattleStateService _service;
    private string _notice;
    private bool _isInitialized;

    public CHeroMasteryService Mastery
    {
        get { return _mastery; }
    }
    public CHeroBattleStateService Service
    {
        get { return _service; }
    }
    public string Notice
    {
        get { return _notice; }
    }
    public string RunId
    {
        get { return _service != null ? _service.RunId : string.Empty; }
    }
    public string BattleId
    {
        get { return _service != null ? _service.BattleId : string.Empty; }
    }
    public bool IsReadyForBattle
    {
        get { return _service != null && _service.IsReadyForBattle; }
    }

    public string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "HeroExpedition", "expedition.json"); }
    }

    private void Awake()
    {
        InitializeDefaultCatalog();
    }

    public bool InitializeDefaultCatalog()
    {
        if (_masteryCatalog == null)
        {
            GameObject prefab = Resources.Load<GameObject>("HeroInnMastery");
            if (prefab != null && prefab.TryGetComponent(out CHeroExpeditionState settings))
                _masteryCatalog = settings._masteryCatalog;
        }
        return _masteryCatalog != null && Initialize(_masteryCatalog);
    }

    public bool TryConfirmInnTraining(string innId, IReadOnlyList<CPlayer> heroes)
    {
        return _service != null && _service.TryConfirmInnTraining(innId, heroes, out _notice);
    }

    public bool Initialize(CHeroMasteryCatalogSO catalog)
    {
        if (_service != null) return _masteryCatalog == catalog && _isInitialized;
        if (catalog == null || !catalog.TryValidate()) return false;
        _masteryCatalog = catalog;
        string masteryPath = Path.Combine(Application.persistentDataPath, "HeroExpedition", "mastery.json");
        _mastery = new CHeroMasteryService(catalog, masteryPath);
        _service = new CHeroBattleStateService(SavePath, _mastery);
        _isInitialized = _service.TryLoad(out _notice);
        return _isInitialized;
    }

    public bool TryStartExpedition(IReadOnlyList<CPlayer> heroes, int masteryPoints = 0)
    {
        if (_service == null || !_service.TryStartExpedition(Guid.NewGuid().ToString("N"),
            Guid.NewGuid().ToString("N"), heroes, masteryPoints, out _notice)) return false;
        _isInitialized = true;
        return true;
    }

    public bool TryRecordHeroDeath(string runId, string battleId, CPlayer hero)
    {
        return _service != null && _service.TryRecordHeroDeath(runId, battleId, hero, out _notice);
    }

    public bool IsSavedMemberAlive(int index)
    {
        return _service != null && _service.IsSavedMemberAlive(index);
    }

    public bool TryCompleteBattle(string runId, string battleId, IReadOnlyList<CPlayer> heroes)
    {
        return _service != null && _service.TryCompleteBattle(runId, battleId, heroes, out _notice);
    }

    // 전투 밖에서 세팅을 확정한 파티는 현재 열과 장착 목록을 넘김
    public bool TryPrepareNextBattle(IReadOnlyList<CPlayer> heroes, bool useCurrentPartySetup = false)
    {
        if (!InitializeDefaultCatalog() || _service == null) return false;
        if (string.IsNullOrEmpty(RunId)) return TryStartExpedition(heroes);
        if (!_service.IsBattleFinished) return TryRestoreBattle(heroes);
        return _service.TryPrepareNextBattle(RunId,
            Guid.NewGuid().ToString("N"), heroes, out _notice, useCurrentPartySetup);
    }

    public bool TryRestoreBattle(IReadOnlyList<CPlayer> heroes)
    {
        return _service != null && _service.TryRestoreBattle(RunId, BattleId, heroes, out _notice);
    }

    private void OnDestroy()
    {
        if (_mastery != null) _mastery.ReleaseRuntimeAssets();
    }
}
