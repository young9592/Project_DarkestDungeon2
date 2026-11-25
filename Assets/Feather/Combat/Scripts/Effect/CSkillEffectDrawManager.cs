
using System.Collections.Generic;
using UnityEngine;

public class CSkillEffectDrawManager : MonoBehaviour
{
    #region Inspector
    // 캐릭터의 앞/뒤에 그리기
    [Header("Draw Root")]
    [SerializeField] private GameObject _frontEffect;
    [SerializeField] private GameObject _backEffect;

    [Header("Highlight Camera")]
    [SerializeField] private Camera _highlightCamera;

    [Header("Draw Canvas")]
    [SerializeField] private Canvas _frontCanvas;
    [SerializeField] private Canvas _backCanvas;

    // 노상강도
    [Header("Duelists Advence")]
    [SerializeField] private GameObject _frontDuel;
    [SerializeField] private GameObject _backDuel;

    [Header("Pistol Shot")]
    [SerializeField] private GameObject _frontPistol;
    [SerializeField] private GameObject _hitBullet;
    // 중보병

    // 도굴꾼

    // 역병의사

    // 잃은 영혼

    // 구울

    // 광신도들

    // 근접 자물쇠

    // 원거리 자물쇠

    // 치유 자물쇠

    // 스트레스 자물쇠


    [Header("Hit")]
    [SerializeField] private GameObject _hitPrefab;
    #endregion

    #region Field
    private List<GameObject> _draws = new List<GameObject>();
    #endregion

    public void DrawEffect(string skillName, List<Transform> entitiesTr, bool isRiposte)
    {
        List<GameObject> draws = new List<GameObject>();



        if (skillName == "결투가의 진격")
        {
            GameObject front = Instantiate(_frontDuel, _frontEffect.transform);
            GameObject back = Instantiate(_backDuel, _backEffect.transform);

            draws.Add(front);
            draws.Add(back);
            
            DrawBloodEffect(draws, entitiesTr[1], false);

            // 반격시 속도가 2배 빠르기 때문에 속도 보정 해줘야합니다.
            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        // 권총사격, 마무리 사격
        else if (skillName == "권총 사격" || skillName == "마무리 속사" || skillName == "확인 사살")
        {
            GameObject front = Instantiate(_frontPistol, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawBulletHitffect(draws, entitiesTr[1]);

            draws.Add(front);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }



        _draws.AddRange(draws);
    }
    public void DrawEffect(string skillName, CEntity actor, CEntity riposteEntity)
    {

    }

    private void DrawBloodEffect(List<GameObject> draws, Transform tr, bool _moveLeft)
    {
        GameObject hitEnemy = Instantiate(_hitPrefab, _backEffect.transform);

        SkinnedMeshRenderer renderer = tr.GetComponentInChildren<SkinnedMeshRenderer>();
        Vector3 vec = renderer != null ? renderer.bounds.center : tr.position;
        Vector3 screenPos = _highlightCamera.WorldToScreenPoint(vec);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_backCanvas.GetComponent<RectTransform>(), screenPos, _backCanvas.worldCamera, out Vector2 uiLocalPos);
        hitEnemy.GetComponent<RectTransform>().anchoredPosition = uiLocalPos;

        if (_moveLeft)
        {
            CSkillEffectDraw drawEffect = hitEnemy.GetComponent<CSkillEffectDraw>();
            drawEffect.ChangeDirection();
        }

        draws.Add(hitEnemy);
    }
    private void DrawBulletHitffect(List<GameObject> draws, Transform tr)
    {
        GameObject hitEnemy = Instantiate(_hitBullet, _frontEffect.transform);

        SkinnedMeshRenderer renderer = tr.GetComponentInChildren<SkinnedMeshRenderer>();
        Vector3 vec = renderer != null ? renderer.bounds.center + Vector3.up * (renderer.bounds.extents.y * 0.2f) : tr.position;
        Vector3 screenPos = _highlightCamera.WorldToScreenPoint(vec);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_backCanvas.GetComponent<RectTransform>(), screenPos, _backCanvas.worldCamera, out Vector2 uiLocalPos);
        hitEnemy.GetComponent<RectTransform>().anchoredPosition = uiLocalPos;

        draws.Add(hitEnemy);
    }

    private void SetRiposteSetting(List<GameObject> draws)
    {
        for (int i = 0; i < draws.Count; i++)
        {
            CSkillEffectDraw drawEffect = draws[i].GetComponent<CSkillEffectDraw>();

            drawEffect.SetRiposteSpeed(2f);
        }
    }

    public void DisableAllEffect()
    {
        for (int i = 0; i < _draws.Count; i++)
        {
            if (_draws[i] == null)
            {
                continue;
            }

            _draws[i].SetActive(false);
        }
    }
}
