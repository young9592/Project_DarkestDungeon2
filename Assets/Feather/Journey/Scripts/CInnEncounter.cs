using UnityEngine;

public class CInnEncounter : MonoBehaviour
{
    #region Inspector
    [Header("Target")]
    [SerializeField] private string _targetName = "StageCoach";

    [Header("Option")]
    [SerializeField] private ESceneId _desireSceneId = ESceneId.Inn;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == _targetName)
        {
            CStageCoachFollowCamera coachFollowCamera = other.gameObject.transform.root.gameObject.GetComponent<CStageCoachFollowCamera>();
            coachFollowCamera.ChangeCinematicType(CStageCoachFollowCamera.ECinematicType.Inn, _desireSceneId);

            CInputDispatcher.Instance.enabled = false;

            CStageCoachHandler.Instance.CrashBaricade();

        }
    }
}
