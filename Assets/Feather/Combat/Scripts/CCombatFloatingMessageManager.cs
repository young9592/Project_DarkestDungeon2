using System;
using System.Collections;
using UnityEngine;

public class CCombatFloatingMessageManager : MonoBehaviour
{
    #region Inspector
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("Floating Text Prefab")]
    [SerializeField] private GameObject _floatingTextRoot;
    [SerializeField] private GameObject _floatingTextPrefab;

    [Header("Icon Prefab")]
    [SerializeField] private GameObject _iconPrefabRoot;
    [SerializeField] private GameObject _deathStampPrefab;
    [SerializeField] private GameObject _stressPrefab;
    [SerializeField] private GameObject _virtuePrefab;
    [SerializeField] private GameObject _targetPrefab;
    #endregion


    // 상태 메세지 출력
    public void ShowMessage(GameObject target, CCombatFloatingText.EMessageType messageType, string text, Vector3 offset = default, float delay = 0f)
    {
        GameObject textPrefab = Instantiate(_floatingTextPrefab, _floatingTextRoot.transform);
        CCombatFloatingText floatText = textPrefab.GetComponent<CCombatFloatingText>();

        if (delay == 0f)
        {
            floatText.SetText(_canvas, target, messageType, text, offset);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => floatText.SetText(_canvas, target, messageType, text, offset), delay));
        }
    }

    // 현재는 죽음의 일격정도만 있습니다.
    // TODO : 스트레스, 스트레스 회복
    public GameObject ShowIcon(GameObject target, CCombatFloatingIcon.IconType iconType, float delay = 0f)
    {
        GameObject iconPrefab = default;

        switch (iconType)
        {
            case CCombatFloatingIcon.IconType.None:
                break;
            case CCombatFloatingIcon.IconType.DeathStemp:
                iconPrefab = _deathStampPrefab; 
                break;
            case CCombatFloatingIcon.IconType.Stress:
                iconPrefab = _stressPrefab;
                break;
            case CCombatFloatingIcon.IconType.Virtue:
                iconPrefab = _virtuePrefab;
                break;
            case CCombatFloatingIcon.IconType.Turn:
                iconPrefab = _targetPrefab;
                break;
            default:
                break;
        }


        GameObject showPrefab = Instantiate(iconPrefab, _iconPrefabRoot.transform);
        CCombatFloatingIcon icon = showPrefab.GetComponent<CCombatFloatingIcon>();

        if (delay == 0f)
        {
            icon.SetIcon(_canvas, target);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => icon.SetIcon(_canvas, target), delay));
        }

        return showPrefab;
    }
    // 메세지를 일정시간뒤에 출력하는 코루틴
    private IEnumerator CoDelayShow(Action action, float delay)
    {

        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
}
