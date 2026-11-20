using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CSceneFlowManager : MonoBehaviour
{
    public enum EFadeType
    {
        Default,
        Combat,
        Combat_Return,
    }

    #region Inspector
    [Header("Catalog")]
    [SerializeField] private CSceneCatalog _catalog;

    [Header("SceneLoadView")]
    [SerializeField] private GameObject _loadPanel;

    [Header("Progress Bar")]
    [SerializeField] private GameObject _progressPanel;
    [SerializeField] private Image _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Header("Option")]
    [SerializeField] private bool _enableKey = true;
    [SerializeField] private bool _dontDestroyOnLoad = true;
    [SerializeField] private List<Material> _fadeMaterials;
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
    private string _encounterObjectId = null;

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

    public void LoadScene(ESceneId id, EFadeType fadeType = EFadeType.Default)
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

        StartCoroutine(CoLoadingScene(id, sceneName, fadeType));
    }

    private IEnumerator CoLoadingScene(ESceneId id, string sceneName, EFadeType fadeType)
    {
        if (_isLoading)
        {
            yield break;
        }

        _isLoading = true;

        _loadPanel.SetActive(true);
        yield return null;

        CPrint.Log($"id = {id} / sceneName = {sceneName}");

        _fadeCoroutine = StartCoroutine(CoFadeOut(fadeType));
        yield return _fadeCoroutine;

        // 마차 정지
        if (fadeType == EFadeType.Combat)
        {
            _returnStageCoach.SetActive(false);
        }

        _loadingText.text = "로딩중...";
        _progressPanel.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (op.progress < 0.9f)
        {
            _progressBar.fillAmount = op.progress;
            yield return null;
        }

        _progressBar.fillAmount = op.progress;
        _progressPanel.SetActive(false);
        _loadingText.text = "";

        yield return null;

        SyncCursorToCurrentScene();

        yield return new WaitUntil(() => op.isDone);

        _fadeCoroutine = StartCoroutine(CoFadeIn(fadeType));
        yield return _fadeCoroutine;

        _loadPanel.SetActive(false);

        // 마차 불러오기
        if (fadeType == EFadeType.Combat_Return)
        {
            ReturnField();
        }

        // 게임오버
        if (id == ESceneId.Title)
        {
            Destroy(_returnStageCoach);
            _encounterEnemyGroup = null;
            _encounterEnemyGroup = null;
            _encounterObjectId = null;
            CBaricadeEncounter.ResetDestroyBaricade();
        }

        _isLoading = false;

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

    private IEnumerator CoFadeOut(EFadeType fadeType)
    {
        switch (fadeType)
        {
            case EFadeType.Default:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
                break;
            case EFadeType.Combat:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
                break;
            case EFadeType.Combat_Return:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
                break;
            default:
                break;
        }


        Material mat = _loadPanel.GetComponent<Image>().material;
        int percent = Shader.PropertyToID("_percent");
        _loadPanel.SetActive(true);

        float delay = 2f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
            mat.SetFloat(percent, t);
            yield return null;
        }

        mat.SetFloat(percent, 1f);
    }

    private IEnumerator CoFadeIn(EFadeType fadeType)
    {
        switch (fadeType)
        {
            case EFadeType.Default:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
                break;
            case EFadeType.Combat:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[1];
                break;
            case EFadeType.Combat_Return:
                _loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
                break;
            default:
                break;
        }

        Material mat = _loadPanel.GetComponent<Image>().material;
        int percent = Shader.PropertyToID("_percent");
        _loadPanel.SetActive(true);

        float delay = 2f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = 0;

            switch (fadeType)
            {
                case EFadeType.Combat:
                    t = Mathf.Clamp01((nextTime - Time.time) / delay);
                    break;
                default:
                    t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
                    break;
            }


            mat.SetFloat(percent, 1f - t);
            yield return null;
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

    public void GetBarigateData(ESceneId loadScene, string id, CEnemyGroupSO enemyGroupSO)
    {
        _returnFieldSceneId = loadScene;
        _encounterObjectId = id;
        _encounterEnemyGroup = enemyGroupSO;

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
