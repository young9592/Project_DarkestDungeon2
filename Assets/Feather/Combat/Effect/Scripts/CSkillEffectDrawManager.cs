
using System.Collections.Generic;
using UnityEngine;

public class CSkillEffectDrawManager : MonoBehaviour
{
    private enum ERendererPositionOption
    {
        CenterUp,
        LeftUp,
        RightUp,
        LeftDown,
        RightDown,
        CenterDown,
        Center,
    }

    private enum ESelectRoot
    {
        Front,
        Back,
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

    [Header("Hit")]
    [SerializeField] private GameObject _hitPrefab;

    #region Highwayman
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

    [Header("TakeAim")]
    [SerializeField] private GameObject _backTakeAim;

    [Header("HighwayRobbery")]
    [SerializeField] private GameObject _frontRobbery;
    [SerializeField] private GameObject _backRobbery;

    [Header("DoubleCross")]
    [SerializeField] private GameObject _frontCross;
    #endregion

    // 중보병
    #region ManAtArms
    [Header("Crash")]
    [SerializeField] private GameObject _frontCrash;

    [Header("Rampart")]
    [SerializeField] private GameObject _frontRampart;
    [SerializeField] private GameObject _backRampart;

    [Header("Bolster")]
    [SerializeField] private GameObject _backBolster;

    [Header("HoldLine")]
    [SerializeField] private GameObject _frontHoldLine;
    [SerializeField] private GameObject _backHoldLine;

    [Header("Defender")]
    [SerializeField] private GameObject _frontDefender;
    [SerializeField] private GameObject _backDefender;


    #endregion
    // 도굴꾼
    #region GraveRobber

    #endregion
    // 역병의사
    #region PlagueDoctor

    #endregion

    // 잃은 영혼

    // 구울

    // 광신도들

    // 근접 자물쇠

    // 원거리 자물쇠

    // 치유 자물쇠

    // 스트레스 자물쇠


    #endregion

    #region Field
    private List<GameObject> _draws = new List<GameObject>();
    #endregion

    public void DrawEffect(string skillName, Transform actor, List<Transform> entitiesTr, bool isRiposte)
    {
        List<GameObject> draws = new List<GameObject>();

        HighWayManEffect(draws, skillName, actor, entitiesTr, isRiposte);
        ManAtArmsEffect(draws, skillName, actor, entitiesTr, isRiposte);
        GraveRobberEffect(draws, skillName, actor, entitiesTr, isRiposte);
        PlagueDoctorEffect(draws, skillName, actor, entitiesTr, isRiposte);

        // Monster

        _draws.AddRange(draws);
    }

    private void PlagueDoctorEffect(List<GameObject> draws, string skillName, Transform actor, List<Transform> entitiesTr, bool isRiposte)
    {
        if (entitiesTr[0].gameObject.GetComponent<CEntity>().EntityData.Id != "hero_plague_doctor")
        {
            return;
        }
    }
    private void GraveRobberEffect(List<GameObject> draws, string skillName, Transform actor, List<Transform> entitiesTr, bool isRiposte)
    {
        if (entitiesTr[0].gameObject.GetComponent<CEntity>().EntityData.Id != "hero_grave_robber")
        {
            return;
        }
    }
    private void ManAtArmsEffect(List<GameObject> draws, string skillName, Transform actor, List<Transform> entitiesTr, bool isRiposte)
    {
        if (entitiesTr[0].gameObject.GetComponent<CEntity>().EntityData.Id != "hero_man_at_arms")
        {
            return;
        }

        if (skillName == "분쇄")
        {
            GameObject front = Instantiate(_frontCrash, _frontEffect.transform);

            draws.Add(front);
            DrawBloodEffect(draws, entitiesTr[1], false);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "포효")
        {

        }
        else if (skillName == "고양")
        {
            DrawTargetEffect(draws, _backBolster, ESelectRoot.Back, actor, ERendererPositionOption.CenterUp);
        }
        else if (skillName == "지시")
        {

        }
        else if (skillName == "과감한 맹공")
        {

        }
        else if (skillName == "수호자")
        {
            DrawTargetEffect(draws, _frontDefender, ESelectRoot.Front, actor, ERendererPositionOption.Center);
            DrawTargetEffect(draws, _backDefender, ESelectRoot.Front, actor, ERendererPositionOption.Center);
        }
        else if (skillName == "전선 유지")
        {
            GameObject front = Instantiate(_frontHoldLine, _frontEffect.transform);
            GameObject back = Instantiate(_backHoldLine, _backEffect.transform);

            draws.Add(front);
            draws.Add(back);
            DrawBloodEffect(draws, entitiesTr[1], false);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "성채의 일격")
        {
            GameObject front = Instantiate(_frontRampart, _frontEffect.transform);
            GameObject back = Instantiate(_backRampart, _backEffect.transform);

            draws.Add(front);
            draws.Add(back);
            DrawBloodEffect(draws, entitiesTr[1], false);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "응징")
        {

        }
        else if (skillName == "사수")
        {

        }
        else if (skillName == "전략적 퇴각")
        {

        }


    }

    private void HighWayManEffect(List<GameObject> draws, string skillName, Transform actor, List<Transform> entitiesTr, bool isRiposte)
    {
        if (entitiesTr[0].gameObject.GetComponent<CEntity>().EntityData.Id != "hero_highwayman")
        {
            return;
        }

        if (skillName == "결투가의 진격")
        {
            GameObject front = Instantiate(_frontDuel, _frontEffect.transform);
            GameObject back = Instantiate(_backDuel, _backEffect.transform);

            draws.Add(front);
            draws.Add(back);

            DrawBloodEffect(draws, entitiesTr[1], false);

            // 반격시 속도가 2배 빠르기 때문에 속도 보정 해줘야합니다.
            SetRiposteSetting(isRiposte, draws);
        }
        // 권총사격, 마무리 사격
        else if (skillName == "권총 사격" || skillName == "마무리 속사" || skillName == "확인 사살")
        {
            GameObject front = Instantiate(_frontPistol, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.CenterUp);

            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "야비한 칼질")
        {
            GameObject front = Instantiate(_frontSlice, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);

            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "정맥 절개")
        {
            GameObject front = Instantiate(_frontVein, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);

            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "영거리 사격")
        {
            GameObject front = Instantiate(_frontPorint, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.Center);

            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "포도탄 발사")
        {
            GameObject front = Instantiate(_frontGrape, _frontEffect.transform);

            for (int i = 1; i < entitiesTr.Count; i++)
            {
                DrawBloodEffect(draws, entitiesTr[i], false);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.CenterUp);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.LeftUp);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.LeftDown);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.RightUp);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.RightDown);
                DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.Center);
            }

            // 대상이 3명[나, 적2명]이면 머즐 플래시 위치조정 필요
            if (entitiesTr.Count >= 3)
            {
                RectTransform rectTr = front.GetComponent<RectTransform>();

                rectTr.anchoredPosition += Vector2.left * 170f;
            }

            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "가늠 사격")
        {
            GameObject front = Instantiate(_frontPistol, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            DrawTargetEffect(draws, _hitBullet, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.CenterUp);
            DrawTargetEffect(draws, _tracking, ESelectRoot.Front, entitiesTr[1], ERendererPositionOption.CenterUp);
            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "정조준")
        {
            GameObject back = Instantiate(_backTakeAim, _backEffect.transform);
            draws.Add(back);
        }
        else if (skillName == "노상 강도질")
        {
            GameObject front = Instantiate(_frontRobbery, _frontEffect.transform);
            GameObject back = Instantiate(_backRobbery, _backEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            draws.Add(front);
            draws.Add(back);

            SetRiposteSetting(isRiposte, draws);
        }
        else if (skillName == "뒤통수 치기")
        {
            GameObject front = Instantiate(_frontCross, _frontEffect.transform);
            DrawBloodEffect(draws, entitiesTr[1], false);
            draws.Add(front);

            SetRiposteSetting(isRiposte, draws);
        }


    }

    // 반격 관련 엔티티 이펙트 처리
    public void DrawEffect(string skillName, CEntity actor, CEntity riposteEntity)
    {
        List<GameObject> draws = new List<GameObject>();

        if (skillName == "노상강도")
        {
            GameObject front = Instantiate(_frontSlice, _frontEffect.transform);
            DrawBloodEffect(draws, actor.gameObject.transform, false);

            draws.Add(front);
        }
        else if (skillName == "중보병")
        {

        }
        else if (skillName == "적 반격01")
        {

        }

        _draws.AddRange(draws);
    }

    // 피격 이펙트 처리
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

    // 타겟 이펙트 처리
    private void DrawTargetEffect(List<GameObject> draws, GameObject prefab, ESelectRoot root, Transform tr, ERendererPositionOption positionOption)
    {
        GameObject hitEnemy = Instantiate(prefab, SelectRoot(root));

        SkinnedMeshRenderer renderer = tr.GetComponentInChildren<SkinnedMeshRenderer>();

        Vector3 vec = renderer != null ? renderer.bounds.center + RendererPosition(renderer, positionOption) : tr.position;
        Vector3 screenPos = _highlightCamera.WorldToScreenPoint(vec);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_backCanvas.GetComponent<RectTransform>(), screenPos, _backCanvas.worldCamera, out Vector2 uiLocalPos);
        hitEnemy.GetComponent<RectTransform>().anchoredPosition = uiLocalPos;

        draws.Add(hitEnemy);
    }

    private Vector3 RendererPosition(SkinnedMeshRenderer renderer, ERendererPositionOption positionOption)
    {
        switch (positionOption)
        {
            case ERendererPositionOption.Center:
                return Vector3.zero;
            case ERendererPositionOption.CenterUp:
                return Vector3.up * (renderer.bounds.extents.y * 0.4f);
            case ERendererPositionOption.LeftUp:
                return Vector3.up * (renderer.bounds.extents.y * 0.4f) + Vector3.left * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case ERendererPositionOption.RightUp:
                return Vector3.up * (renderer.bounds.extents.y * 0.4f) + Vector3.right * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case ERendererPositionOption.LeftDown:
                return Vector3.down * (renderer.bounds.extents.y * 0.4f) + Vector3.left * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case ERendererPositionOption.RightDown:
                return Vector3.down * (renderer.bounds.extents.y * 0.4f) + Vector3.right * (renderer.bounds.extents.x * Random.Range(0, 0.2f));
            case ERendererPositionOption.CenterDown:
                return Vector3.down * (renderer.bounds.extents.y * 0.4f);
        }

        return Vector3.zero;
    }
    private Transform SelectRoot(ESelectRoot root)
    {
        switch (root)
        {
            case ESelectRoot.Front:
                return _frontEffect.transform;
            case ESelectRoot.Back:
                return _backEffect.transform;
        }

        CPrint.Error($"{gameObject.name} : 잘못된 설정값 입력됨.");
        return _frontEffect.transform;
    }
    private void SetRiposteSetting(bool isRiposte, List<GameObject> draws)
    {
        if (!isRiposte)
        {
            return;
        }

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
