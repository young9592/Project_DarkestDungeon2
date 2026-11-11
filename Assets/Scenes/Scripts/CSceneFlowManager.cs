using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


#region Build용 임시 SceneManager
/*
빌드용 임시 SceneManager입니다.

 */
#endregion


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

    [Header("Option")]
    [SerializeField] private bool _enableKey = true;
    [SerializeField] private bool _dontDestroyOnLoad = true;

    #endregion

    #region Field
    private static CSceneFlowManager _instance;
    private int _cursorIndex = 0;
    private bool _isLoading = false;

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

        _loadPanel.SetActive(true);
        yield return null;

        CPrint.Log($"id = {id} / sceneName = {sceneName}");


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

        delay = 2f;
        nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
            mat.SetFloat(percent, 1f - t);
            yield return null;
        }

        yield return null;

        _loadPanel.SetActive(false);

        SyncCursorToCurrentScene();


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

    // 일균님이랑 키가 겹칠수 있기 때문에 qwertyu로 배정
    private void HandleHotkeys()
    {
        if (Keyboard.current.qKey.isPressed)
        {
            LoadScene(ESceneId.Title);
        }
        if (Keyboard.current.wKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Park);
        }
        if (Keyboard.current.eKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Park_Character_Information_Title);
        }
        if (Keyboard.current.rKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Sin_Chapter);
        }
        if (Keyboard.current.tKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Sin_TheCrossroads);
        }
        if (Keyboard.current.yKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Sin_TheInn);
        }
        if (Keyboard.current.uKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Go);
        }

        if (Keyboard.current.iKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Lee);
        }

        if (Keyboard.current.oKey.isPressed)
        {
            LoadScene(ESceneId.Temp_Jo);
        }

        if (Keyboard.current.enterKey.isPressed)
        {
            ReloadScene();
        }
    }
}
