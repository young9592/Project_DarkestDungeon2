using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESceneId
{
    Temp_Bulid = 0,
    Title,
    Temp_Park,
    Temp_Sin_Chapter,
    Temp_Sin_TheCrossroads,
    Temp_Go,
    Temp_Lee,
    Temp_Jo
}

[Serializable]
public class SceneEntry
{
    public ESceneId id;
    public string sceneName;
}

public class CSceneCatalog : MonoBehaviour
{


    #region Inspector
    [Header("SceneEntry")]
    [SerializeField] private List<SceneEntry> _scenes = new List<SceneEntry>();
    #endregion

    #region Field
    private readonly Dictionary<ESceneId, string> _idToName = new Dictionary<ESceneId, string>();
    private readonly Dictionary<string, ESceneId> _nameToId = new Dictionary<string, ESceneId>();
    public IReadOnlyList<SceneEntry> Entries => _scenes;
    #endregion


    private void Awake()
    {
        BuildMaps();
    }


    [ContextMenu("BuildMaps (Rebuild Catalog)")]
    public void BuildMaps()
    {
        _idToName.Clear();
        _nameToId.Clear();

        for (int i = 0; i < _scenes.Count; i++)
        {
            SceneEntry e = _scenes[i];

            // 프로그래머 실수1
            if (e == null)
            {
                continue;
            }

            // 프로그래머 실수2
            if (string.IsNullOrEmpty(e.sceneName))
            {
                CPrint.Warn("엔트리가 비었습니다. 확인 필요");
                continue;
            }

            // 중복 체크를 했는데 같은 씬 ID가 이미 있으면 제외
            if (_idToName.ContainsKey(e.id))
            {
                CPrint.Warn($"{e.id} 중복 / 기존 = {_idToName[e.id]} / 신규 = {e.sceneName}");
                continue;
            }

            // 같은 씬 이름이 이미 있으면 제외
            if (_nameToId.ContainsKey(e.sceneName))
            {
                continue;
            }

            _idToName.Add(e.id, e.sceneName);
            _nameToId.Add(e.sceneName, e.id);
        }

        CPrint.Log($"Scene List Count {_scenes.Count}");
        CPrint.Log($"Map Count (ID → Name) {_idToName.Count}");
        CPrint.Log($"Map Count (Name → ID) {_nameToId.Count}");
    }

    public bool TryGetSceneName(ESceneId id, out string sceneName)
    {
        return _idToName.TryGetValue(id, out sceneName);
    }

    public string GetSceneName(ESceneId id)
    {
        if (_idToName.TryGetValue(id, out string name))
        {
            return name;
        }

        return string.Empty;
    }

    public bool TryGetSceneId(string sceneName, out ESceneId id)
    {
        return _nameToId.TryGetValue(sceneName, out id);
    }

    public List<SceneEntry> GetEntries()
    {
        return _scenes;
    }
}
