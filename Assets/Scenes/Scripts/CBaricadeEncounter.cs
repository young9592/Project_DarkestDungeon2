using System.Collections.Generic;
using UnityEngine;

public class CBaricadeEncounter : MonoBehaviour
{
    #region Inspector
    [Header("Encounter Object ID")]
    [SerializeField] private string _id;

    [Header("LoadScene")]
    [SerializeField] private ESceneId _loadScene;

    [Header("EncounterGroup")]
	[SerializeField] private CEnemyGroupSO _enemyGroupSO;

    [Header("Target")]
    [SerializeField] private string _targetName = "StageCoach";
    #endregion

    #region Field
    private static HashSet<string> _destroyId = new HashSet<string>();
    #endregion

    private void Start()
    {
        if (_destroyId.Contains(_id))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_id == CSceneFlowManager.Instance.EncounterObjectID)
        {
            return;
        }

        if(other.transform.root.name == _targetName)
        {
            CStageCoachHandler.Instance.CrashBaricade();
            CSceneFlowManager.Instance.GetBarigateData(_loadScene, _id, _enemyGroupSO);
            CSceneFlowManager.Instance.GetStageCoach(other);
            CSceneFlowManager.Instance.LoadScene(ESceneId.Battle, CSceneFlowManager.EFadeType.Combat);
        }
    }

    public static void AddDestroyBaricade(string id)
    {
        _destroyId.Add(id);
    }

    public static void ResetDestroyBaricade()
    {
        _destroyId.Clear();
    }
}
