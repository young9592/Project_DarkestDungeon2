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
    [SerializeField] private GameObject _stressHealPrefab;
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
    public GameObject ShowIcon(GameObject target, CIcon.IconType iconType, float delay = 0f)
    {
        GameObject iconPrefab = default;

        switch (iconType)
        {
            case CIcon.IconType.DeathBlow:
                iconPrefab = _deathStampPrefab;
                break;
            case CIcon.IconType.Stress:
                iconPrefab = _stressPrefab;
                break;
            case CIcon.IconType.StressHeal:
                iconPrefab = _stressHealPrefab;
                break;
        }


        GameObject showPrefab = Instantiate(iconPrefab, _iconPrefabRoot.transform);
        CIcon icon = showPrefab.GetComponent<CIcon>();

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
