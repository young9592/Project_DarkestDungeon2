
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CSkillEffectDrawManager : MonoBehaviour
{
    private enum EBulletHitPosition
    {
        Default,
        LeftUp,
        RightUp,
        LeftDown,
        RightDown,
        Center,
    }

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

    [Header("WickedSlice")]
    [SerializeField] private GameObject _frontSlice;

    [Header("PointBlankShot")]
    [SerializeField] private GameObject _frontPorint;

    [Header("GrapeShot")]
    [SerializeField] private GameObject _frontGrape;

    [Header("OpenVein")]
    [SerializeField] private GameObject _frontVein;

    [Header("TrackingShot")]
    [SerializeField] private GameObject _tracking;

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
            DrawBulletHitEffect(draws, entitiesTr[1], EBulletHitPosition.Default);

            draws.Add(front);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        else if (skillName == "야비한 칼질")
        {
            GameObject front = Instantiate(_frontSlice, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        else if (skillName == "정맥 절개")
        {
            GameObject front = Instantiate(_frontVein, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        else if (skillName == "영거리 사격")
        {
            GameObject front = Instantiate(_frontPorint, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawBulletHitEffect(draws, entitiesTr[1], EBulletHitPosition.Default);

            draws.Add(front);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        else if (skillName == "포도탄 발사")
        {
            GameObject front = Instantiate(_frontGrape, _frontEffect.transform);

            for (int i = 1; i < entitiesTr.Count; i++)
            {
                DrawBloodEffect(draws, entitiesTr[i], false);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.Default);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.LeftUp);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.RightUp);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.LeftDown);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.RightDown);
                DrawBulletHitEffect(draws, entitiesTr[i], EBulletHitPosition.Center);
            }

            // 대상이 3명이면 머즐 플래시 위치조정 필요
            if (entitiesTr.Count >= 3)
            {
                RectTransform rectTr = front.GetComponent<RectTransform>();

                rectTr.anchoredPosition += Vector2.left * 170f;
            }

            draws.Add(front);

            if (isRiposte)
            {
                SetRiposteSetting(draws);
            }
        }
        else if (skillName == "가늠 사격")
        {
            GameObject front = Instantiate(_frontPistol, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawBulletHitEffect(draws, entitiesTr[1], EBulletHitPosition.Default);
            DrawTrakingMarkEffect(draws, entitiesTr[1]);
            draws.Add(front);
        }

        _draws.AddRange(draws);
    }
    public void DrawEffect(string skillName, CEntity actor, CEntity riposteEntity)
    {
        List<GameObject> draws = new List<GameObject>();

        if (skillName == "노상강도")
        {
            GameObject front = Instantiate(_frontSlice, _frontEffect.transform);
            DrawBloodEffect(draws, actor.gameObject.transform, false);
        }
        else if (skillName == "중보병")
        {

        }

        _draws.AddRange(draws);
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
    private void DrawBulletHitEffect(List<GameObject> draws, Transform tr, EBulletHitPosition positionOption)
    {
        GameObject hitEnemy = Instantiate(_hitBullet, _frontEffect.transform);

        SkinnedMeshRenderer renderer = tr.GetComponentInChildren<SkinnedMeshRenderer>();


        Vector3 vec = renderer != null ? renderer.bounds.center + BulletHitPosition(renderer, positionOption) : tr.position;
        Vector3 screenPos = _highlightCamera.WorldToScreenPoint(vec);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_backCanvas.GetComponent<RectTransform>(), screenPos, _backCanvas.worldCamera, out Vector2 uiLocalPos);
        hitEnemy.GetComponent<RectTransform>().anchoredPosition = uiLocalPos;

        draws.Add(hitEnemy);
    }
    private void DrawTrakingMarkEffect(List<GameObject> draws, Transform tr)
    {
        GameObject hitEnemy = Instantiate(_tracking, _frontEffect.transform);

        SkinnedMeshRenderer renderer = tr.GetComponentInChildren<SkinnedMeshRenderer>();

        Vector3 vec = renderer != null ? renderer.bounds.center + BulletHitPosition(renderer, EBulletHitPosition.Default) : tr.position;
        Vector3 screenPos = _highlightCamera.WorldToScreenPoint(vec);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_backCanvas.GetComponent<RectTransform>(), screenPos, _backCanvas.worldCamera, out Vector2 uiLocalPos);
        hitEnemy.GetComponent<RectTransform>().anchoredPosition = uiLocalPos;

        draws.Add(hitEnemy);
    }

    private Vector3 BulletHitPosition(SkinnedMeshRenderer renderer, EBulletHitPosition positionOption)
    {
        switch (positionOption)
        {
            case EBulletHitPosition.Default:
                return Vector3.up * (renderer.bounds.extents.y * 0.3f);
            case EBulletHitPosition.LeftUp:
                return Vector3.up * (renderer.bounds.extents.y * 0.3f) + Vector3.left * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case EBulletHitPosition.RightUp:
                return Vector3.up * (renderer.bounds.extents.y * 0.3f) + Vector3.right * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case EBulletHitPosition.LeftDown:
                return Vector3.down * (renderer.bounds.extents.y * 0.3f) + Vector3.left * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case EBulletHitPosition.RightDown:
                return Vector3.down * (renderer.bounds.extents.y * 0.3f) + Vector3.right * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case EBulletHitPosition.Center:
                return Vector3.zero;
        }

        return Vector3.zero;
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
