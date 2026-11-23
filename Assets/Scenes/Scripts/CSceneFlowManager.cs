using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CSceneFlowManager : MonoBehaviour
{

    #region Inspector
    [Header("Catalog")]
    [SerializeField] private CSceneCatalog _catalog;

    [Header("SceneLoadView")]
    [SerializeField] private GameObject _loadPanel;

    [Header("Progress Bar")]
    [SerializeField] private GameObject _progressPanel;
    [SerializeField] private Image _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Header("Transition")]
    [SerializeField] private CSceneTransition _transition;

    [Header("Option")]
    [SerializeField] private bool _enableKey = true;
    [SerializeField] private bool _dontDestroyOnLoad = true;
    #endregion

    #region Field
    private static CSceneFlowManager _instance;
    private int _cursorIndex = 0;
    private Coroutine _fadeCoroutine;
    private static bool _isLoading = false;
    // 씬 로딩을 통해 전달해야하는 데이터
    private ESceneId _returnFieldSceneId;
    private GameObject _returnStageCoach;
    private CEnemyGroupSO _encounterEnemyGroup = null;
    private CBaricadeEncounter.EChapterBoss _chapterBoss;
    private string _encounterObjectId = null;
    private ESceneId _addtiveSceneId = ESceneId.Combat_Valley;

    #endregion

    #region Property
    public static CSceneFlowManager Instance => _instance;
    public bool IsLoading => _isLoading;
    public ESceneId ReturnFieldSceneId => _returnFieldSceneId;
    public CEnemyGroupSO EncounterEnemyGroup => _encounterEnemyGroup;
    public string EncounterObjectID => _encounterObjectId;

    #endregion


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (_catalog == null)
        {
            CPrint.Error("카탈로그 없음");
            Destroy(_catalog);
            return;
        }

        _catalog.BuildMaps();

        SyncCursorToCurrentScene();

        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        if (!_enableKey)
        {
            return;
        }
        if (_catalog == null)
        {
            return;
        }
        if (_isLoading)
        {
            return;
        }

        HandleHotkeys();
    }

    private void SyncCursorToCurrentScene()
    {
        List<SceneEntry> entries = new List<SceneEntry>();

        if (entries == null || entries.Count == 0)
        {
            return;
        }

        string currentName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].sceneName == currentName)
            {
                CPrint.Log($"Cursor[{i}]");
                _cursorIndex = i;
                return;
            }
        }

        _cursorIndex = 0;
        CPrint.Warn($"커서 싱크 실패 : {currentName}");
    }

    public void LoadScene(ESceneId id)
    {
        bool loadSuccess = _catalog.TryGetSceneName(id, out string sceneName);

        if (!loadSuccess)
        {
            CPrint.Warn($"Load Fail : [{id}] 없음");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            CPrint.Warn($"Load Fail : 씬 네임이 비어있음");
            return;
        }

        StartCoroutine(CoLoadingScene(id, sceneName));
    }

    private IEnumerator CoLoadingScene(ESceneId id, string sceneName)
    {
        if (_isLoading)
        {
            yield break;
        }

        _isLoading = true;

        _catalog.TryGetSceneId(SceneManager.GetActiveScene().name, out ESceneId prevId);
        _loadPanel.SetActive(true);
        yield return null;

        CPrint.Log($"id = {id} / sceneName = {sceneName}");

        _fadeCoroutine = StartCoroutine(_transition.CoFadeOut(prevId, id, _loadPanel));
        yield return _fadeCoroutine;

        // 사전작업
        PrevLoadSetting(prevId, id);

        // TODO : 횃불 로딩창 변경필요
        _loadingText.text = "로딩중...";
        _progressPanel.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        CombatBackgroundLoad(prevId, out AsyncOperation op2);

        while (op.progress < 0.9f)
        {
            _progressBar.fillAmount = op.progress;
            yield return null;
        }

        while (op2 != null && op2.progress < 0.9f)
        {
            yield return null;
        }

        // TODO : 횃불 로딩창 변경필요
        _progressBar.fillAmount = op.progress;
        _progressPanel.SetActive(false);
        _loadingText.text = "";

        SyncCursorToCurrentScene();

        yield return new WaitUntil(() => op.isDone);

        _fadeCoroutine = StartCoroutine(_transition.CoFadeIn(prevId, id, _loadPanel));
        yield return _fadeCoroutine;

        _loadPanel.SetActive(false);

        NextLoadSetting(prevId, id);

        _isLoading = false;

    }
    private void CombatBackgroundLoad(ESceneId prevId, out AsyncOperation op)
    {

        if (prevId == ESceneId.Valley)
        {
            _catalog.TryGetSceneName(ESceneId.Combat_Valley, out string sceneName);
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _addtiveSceneId = ESceneId.Combat_Valley;
            return;
        }
        else if (prevId == ESceneId.Sprawl && _chapterBoss == CBaricadeEncounter.EChapterBoss.None)
        {
            _catalog.TryGetSceneName(ESceneId.Combat_Sprawl, out string sceneName);
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _addtiveSceneId = ESceneId.Combat_Sprawl;
            return;
        }
        else if (prevId == ESceneId.Sprawl && _chapterBoss == CBaricadeEncounter.EChapterBoss.Chapter01)
        {
            // TODO : 가능하면 보스 전투씬도 추가
            _catalog.TryGetSceneName(ESceneId.Combat_Sprawl, out string sceneName);
            op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _addtiveSceneId = ESceneId.Combat_Sprawl;
            return;
        }
        else if (prevId == ESceneId.Battle)
        {
            _catalog.TryGetSceneName(_addtiveSceneId, out string sceneName);
            SceneManager.UnloadSceneAsync(sceneName);
        }

        op = null;
    }
    private void PrevLoadSetting(ESceneId prevId, ESceneId nextId)
    {
        // 마차 정지
        if (nextId == ESceneId.Battle)
        {
            _returnStageCoach.SetActive(false);
        }
    }

    private void NextLoadSetting(ESceneId prevId, ESceneId nextId)
    {
        // 게임오버
        if (prevId == ESceneId.Battle && nextId == ESceneId.Title)
        {
            Destroy(_returnStageCoach);
            _encounterEnemyGroup = null;
            _encounterEnemyGroup = null;
            _encounterObjectId = null;
            CBaricadeEncounter.ResetDestroyBaricade();
        }

        // 마차 불러오기
        if (prevId == ESceneId.Battle && (nextId == ESceneId.Valley || nextId == ESceneId.Sprawl))
        {
            ReturnField();
        }

    }

    private void ReloadScene()
    {
        string current = SceneManager.GetActiveScene().name;

        if (!_catalog.TryGetSceneId(current, out ESceneId id))
        {
            return;
        }

        CPrint.Log($"{current} Reload");
        LoadScene(id);
    }

    private void HandleHotkeys()
    {
        if (!_enableKey)
        {
            return;
        }

        if (Keyboard.current.enterKey.isPressed)
        {
            ReloadScene();
        }
    }


    public void SetEncounterGroup(out CEnemyGroupSO enemyGroupSO)
    {
        enemyGroupSO = _encounterEnemyGroup;

        // 데이터 성공적 전달
        if (enemyGroupSO != null)
        {
            _encounterEnemyGroup = null;
        }
    }

    public void GetBarigateData(ESceneId loadScene, string id, CEnemyGroupSO enemyGroupSO, CBaricadeEncounter.EChapterBoss isBoss)
    {
        _returnFieldSceneId = loadScene;
        _encounterObjectId = id;
        _encounterEnemyGroup = enemyGroupSO;
        _chapterBoss = isBoss;

        if (_encounterEnemyGroup == null)
        {
            CPrint.Error("적 그룹 데이터 전달받기 실패.");
        }

        CBaricadeEncounter.AddDestroyBaricade(_encounterObjectId);
    }

    public void GetStageCoach(Collider other)
    {
        _returnStageCoach = other.transform.root.gameObject;

        DontDestroyOnLoad(_returnStageCoach);
    }

    public void ReturnField()
    {
        SceneManager.MoveGameObjectToScene(_returnStageCoach, SceneManager.GetActiveScene());
        _returnStageCoach.SetActive(true);
    }


}
