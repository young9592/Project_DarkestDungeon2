using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class CHeroInnMastery : MonoBehaviour
{
    [Header("Expedition")]
    [SerializeField] private CHeroExpeditionState _expeditionState;
    [Header("UI Test")]
    [SerializeField] private CHeroPartySO _testPartyPreset;

    private readonly List<CPlayer> _heroes = new List<CPlayer>();
    private readonly List<CPlayer> _originalHeroes = new List<CPlayer>();
    private CHeroInnTestSession _testSession;
    private string _innId;
    private string _notice;
    private static CHeroInnMastery _instance;

    public static CHeroInnMastery Instance { get { return _instance; } }
    public int Points { get { return Mastery != null ? Mastery.Points : 0; } }
    public bool IsTraining { get { return Mastery != null && Mastery.IsTraining; } }
    public string Notice { get { return _notice; } }
    public bool IsTestSession { get { return _testSession != null; } }
    public string TestSavePath { get { return _testSession != null ? _testSession.SavePath : string.Empty; } }
    public string[] HeroIds
    {
        get { return Service != null ? Service.GetLivingHeroIds() : new string[0]; }
    }
    private CHeroBattleStateService Service
    {
        get { return _testSession != null ? _testSession.Service
            : (_expeditionState != null ? _expeditionState.Service : null); }
    }
    private CHeroMasteryService Mastery
    {
        get { return _testSession != null ? _testSession.Mastery
            : (_expeditionState != null ? _expeditionState.Mastery : null); }
    }

    public event Action OnChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntime()
    {
        _instance = null;
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
        SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Inn" || _instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("HeroInnMastery");
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
        SceneManager.MoveGameObjectToScene(instance, scene);
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
        if (_expeditionState == null) _expeditionState = GetComponent<CHeroExpeditionState>();
        if (_expeditionState != null) _expeditionState.InitializeDefaultCatalog();
        CHeroParty party = FindObjectOfType<CHeroParty>();
        if (party != null && party.IsReady) BindParty(party.HeroList);
    }

    public bool BindParty(IReadOnlyList<CPlayer> heroes)
    {
        if (IsTraining || IsTestSession || heroes == null || heroes.Count > 4) return false;
        HashSet<CPlayer> seen = new HashSet<CPlayer>();
        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i] == null || !seen.Add(heroes[i])) return false;
        }
        _heroes.Clear();
        for (int i = 0; i < heroes.Count; i++) _heroes.Add(heroes[i]);
        return true;
    }

    public bool TryPrepareTestParty(int masteryPoints = 10)
    {
        _notice = "UI tests require Play Mode in the Inn scene and no active training.";
        if (!Application.isPlaying || (!Application.isEditor && !Debug.isDebugBuild)
            || gameObject.scene.name != "Inn" || IsTraining || IsTestSession
            || _expeditionState == null || masteryPoints < 0) return Notify(false);

        CHeroInnTestSession session = new CHeroInnTestSession();
        if (!session.TryPrepare(_testPartyPreset, _expeditionState.MasteryCatalog,
            gameObject.scene, masteryPoints, out _notice)) return Notify(false);

        _originalHeroes.Clear();
        _originalHeroes.AddRange(_heroes);
        _heroes.Clear();
        _heroes.AddRange(session.Heroes);
        _testSession = session;
        return Notify(true);
    }

    [ContextMenu("Prepare UI Test Party")]
    private void PrepareTestParty()
    {
        TryPrepareTestParty();
    }

    [ContextMenu("End UI Test Party")]
    public void EndTestSession()
    {
        if (_testSession == null) return;
        CancelTraining();
        CHeroInnTestSession session = _testSession;
        _testSession = null;
        _heroes.Clear();
        _heroes.AddRange(_originalHeroes);
        _originalHeroes.Clear();
        session.Clear();
        Notify(true);
    }

    public bool TryOpenTraining()
    {
        _notice = "Finish a battle before opening inn training.";
        if (gameObject.scene.name != "Inn" || Mastery == null || Service == null
            || !Service.IsBattleFinished || HeroIds.Length == 0 || IsTraining)
            return Notify(false);
        string innId = Guid.NewGuid().ToString("N");
        bool opened = Mastery.TryBeginInnTraining(innId, out _notice);
        if (opened) _innId = innId;
        return Notify(opened);
    }

    public bool CanMaster(string heroId, string skillId)
    {
        if (Mastery == null || !HasHero(heroId)) return false;
        return Mastery.CanMaster(heroId, skillId, out _);
    }

    public bool TryMaster(string heroId, string skillId)
    {
        _notice = "This Hero is not in the current party.";
        if (Mastery == null || !HasHero(heroId)) return Notify(false);
        return Notify(Mastery.TryMaster(heroId, skillId, out _notice));
    }

    public CSkillDataSO GetPreviewSkill(string heroId, CSkillDataSO skill)
    {
        if (Mastery == null || !HasHero(heroId)) return null;
        return Mastery.ResolveTrainingSkill(heroId, skill);
    }

    public bool TryResetTraining()
    {
        if (Mastery == null) return Notify(false);
        return Notify(Mastery.TryResetTraining(out _notice));
    }

    public bool TryConfirmTraining()
    {
        if (!IsTraining || gameObject.scene.name != "Inn") return Notify(false);
        bool confirmed;
        if (_testSession != null)
        {
            confirmed = Service.TryConfirmInnTraining(_innId, _heroes, out _notice);
        }
        else
        {
            confirmed = _expeditionState.TryConfirmInnTraining(_innId, _heroes);
            _notice = _expeditionState.Notice;
        }
        if (confirmed) _innId = null;
        return Notify(confirmed);
    }

    public void CancelTraining()
    {
        if (Mastery != null && IsTraining) Mastery.CancelTraining();
        _innId = null;
        _notice = string.Empty;
        Notify(true);
    }

    private bool HasHero(string heroId)
    {
        string[] ids = HeroIds;
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == heroId) return true;
        }
        return false;
    }

    private bool Notify(bool result)
    {
        OnChanged?.Invoke();
        return result;
    }

    private void OnDisable()
    {
        if (IsTestSession) EndTestSession();
        else CancelTraining();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
