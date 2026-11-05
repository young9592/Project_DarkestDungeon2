using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCombatCameraManager : MonoBehaviour
{
    // 전투 연출 타입
    public enum CombatTarget
    {
        None,
        Hero_Attack,
        Hero_SoloBuff,
        Hero_SingleTargetBuff,
        Hero_MultiTargetBuff,
        Enemy_Attack,
        Enemy_SoloBuff,
        Enemy_SingleTargetBuff,
        Enemy_MultiTargetBuff,
    }

    public enum CameraMoveDirection
    {
        None,
        Left,
        Right
    }


    #region Inspector

    [Header("Connect")]
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private Camera _camera;

    [Header("Option")]
    [SerializeField] private string _highlightLayerName = "Highlight";
    [SerializeField] private string _defaultLayerName = "Default";
    #endregion

    #region Field
    private Transform _cameraTr;
    private Vector3 _initVec;
    #endregion

    private void Awake()
    {
        if (CPrint.NullUnityObjectCheck(gameObject.name, _camera))
        {
            enabled = false;
            return;
        }

        if (CPrint.NullUnityObjectCheck(gameObject.name, _combatUI))
        {
            enabled = false;
            return;
        }

        int layer = LayerMask.NameToLayer(_highlightLayerName);

        if (layer == -1)
        {
            CPrint.Error($"{gameObject.name} 강조 레이어 설정 오류.");
            enabled = false;
            return;
        }

        _cameraTr = _camera.transform;
        _initVec = _cameraTr.position;
    }

    public void ViewReset()
    {
        _cameraTr.position = _initVec;
    }
    public void ViewLeft()
    {

    }

    public void ViewRight()
    {

    }

    // 행동 시 카메라 연출
    public Coroutine CombatAction(CEntity actor, IReadOnlyList<CEntity> entities)
    {
        CombatTarget combatTarget = CombatTarget.None;

        // 판정
        bool isHero = actor is CPlayer;

        if (isHero)
        {
            // 자버프
            if (entities.Count == 0)
            {
                combatTarget = CombatTarget.Hero_SoloBuff;
            }
            else
            {
                // 동맹 체크
                bool isAlly = entities[0] is CPlayer;

                if (isAlly)
                {
                    combatTarget = CombatTarget.Hero_SingleTargetBuff;

                    // 본인 포함된 스킬인 경우
                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (actor.Equals(entities[i]))
                        {
                            combatTarget = CombatTarget.Hero_MultiTargetBuff;
                            break;
                        }
                    }
                }
                else
                {
                    combatTarget = CombatTarget.Hero_Attack;
                }
            }
        }
        else
        {
            if (entities.Count == 0)
            {
                combatTarget = CombatTarget.Enemy_SoloBuff;
            }
            else
            {
                bool isAlly = entities[0] is CEnemy;

                if (isAlly)
                {
                    combatTarget = CombatTarget.Enemy_SingleTargetBuff;

                    for (int i = 0; i < entities.Count; i++)
                    {
                        if (actor.Equals(entities[i]))
                        {
                            combatTarget = CombatTarget.Enemy_MultiTargetBuff;
                            break;
                        }
                    }
                }
                else
                {
                    combatTarget = CombatTarget.Enemy_Attack;
                }
            }
        }

        return CombatActionProcess(combatTarget, actor, entities);
    }

    private Coroutine CombatActionProcess(CombatTarget combatTarget, CEntity actor, IReadOnlyList<CEntity> entities)
    {
        // 초기위치
        List<Vector3> entitiesInitPos = new List<Vector3>();
        // 이동 시키기 위해 transform 가져오기
        List<Transform> entitiesTr = new List<Transform>();
        // 카메라 이동방향
        CameraMoveDirection moveDir = CameraMoveDirection.None;
        // 반격 토큰을 가지고 있다면 중간에 화면을 다시 꺽어줘야합니다.
        bool hasRiposte = false;

        switch (combatTarget)
        {
            case CombatTarget.None:
                break;
            case CombatTarget.Hero_Attack:

                entitiesInitPos.Add(actor.gameObject.transform.position);
                entitiesTr.Add(actor.gameObject.transform);

                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesInitPos.Add(entities[i].gameObject.transform.position);
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Right;

                // TODO : 상대에게 반격 토큰 있는지 체크 필요

                break;
            case CombatTarget.Hero_SoloBuff:
                entitiesInitPos.Add(actor.gameObject.transform.position);
                entitiesTr.Add(actor.gameObject.transform);
                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Hero_SingleTargetBuff:
                if (actor.CurrentRank > entities[0].CurrentRank)
                {
                    entitiesInitPos.Add(actor.gameObject.transform.position);
                    entitiesTr.Add(actor.gameObject.transform);

                    entitiesInitPos.Add(entities[0].gameObject.transform.position);
                    entitiesTr.Add(entities[0].gameObject.transform);
                }
                else
                {
                    entitiesInitPos.Add(entities[0].gameObject.transform.position);
                    entitiesTr.Add(entities[0].gameObject.transform);

                    entitiesInitPos.Add(actor.gameObject.transform.position);
                    entitiesTr.Add(actor.gameObject.transform);
                }
                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Hero_MultiTargetBuff:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesInitPos.Add(entities[i].gameObject.transform.position);
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Enemy_Attack:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesInitPos.Add(entities[i].gameObject.transform.position);
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                entitiesInitPos.Add(actor.gameObject.transform.position);
                entitiesTr.Add(actor.gameObject.transform);

                moveDir = CameraMoveDirection.Left;

                // TODO : 상대에게 반격 토큰 있는지 체크 필요
                break;
            case CombatTarget.Enemy_SoloBuff:
                entitiesInitPos.Add(actor.gameObject.transform.position);
                entitiesTr.Add(actor.gameObject.transform);
                moveDir = CameraMoveDirection.Left;
                break;
            case CombatTarget.Enemy_SingleTargetBuff:
                if (actor.CurrentRank < entities[0].CurrentRank)
                {
                    entitiesInitPos.Add(actor.gameObject.transform.position);
                    entitiesTr.Add(actor.gameObject.transform);

                    entitiesInitPos.Add(entities[0].gameObject.transform.position);
                    entitiesTr.Add(entities[0].gameObject.transform);
                }
                else
                {
                    entitiesInitPos.Add(entities[0].gameObject.transform.position);
                    entitiesTr.Add(entities[0].gameObject.transform);

                    entitiesInitPos.Add(actor.gameObject.transform.position);
                    entitiesTr.Add(actor.gameObject.transform);
                }
                moveDir = CameraMoveDirection.Left;

                break;
            case CombatTarget.Enemy_MultiTargetBuff:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesInitPos.Add(entities[i].gameObject.transform.position);
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Left;
                break;

        }

        return StartCoroutine(CoAction(entitiesInitPos, entitiesTr, moveDir, combatTarget, hasRiposte));
    }

    private IEnumerator CoAction(List<Vector3> entitiesInitPos, List<Transform> entitiesTr, CameraMoveDirection moveDir, CombatTarget combatTarget, bool hasRiposeToken)
    {
        List<Vector3> camInitPos = new List<Vector3>();

        float space = 2f * Mathf.Clamp01((5f - entitiesTr.Count) / 4f) + 0.5f;

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            float x = (i - (entitiesTr.Count - 1) * 0.5f) * space;

            entitiesTr[i].position = new Vector3(x, -0.7f, 0);
            camInitPos.Add(entitiesTr[i].position);
        }

        float delay = 2f;
        float nextTime = Time.time + delay;
        float move = moveDir == CameraMoveDirection.Right ? 0.5f : -0.5f;
        List<Vector3> moveVec = new List<Vector3>();
        _camera.orthographicSize = 2f;

        for (int i = 0; i < camInitPos.Count; i++)
        {
            moveVec.Add(camInitPos[i] + new Vector3(move, 0, 0));
        }

        _combatUI.ToggleEntityStateRoot(false);
        _combatUI.ToggleTurnGuideRoot(false);

        // 공격 방향에 따라 이펙트
        if (combatTarget == CombatTarget.Hero_Attack)
        {
            _combatUI.AttackRight();
        }
        else if (combatTarget == CombatTarget.Enemy_Attack)
        {
            _combatUI.AttackLeft();
        }

        int layer = LayerMask.NameToLayer(_highlightLayerName);

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            HighlightCharacter(entitiesTr[i].gameObject, layer);
        }


        while (nextTime > Time.time)
        {
            float t = 1f - (nextTime - Time.time) / delay;

            for (int i = 0; i < camInitPos.Count; i++)
            {
                entitiesTr[i].position = Vector3.Lerp(camInitPos[i], moveVec[i], t);
                entitiesTr[i].gameObject.GetComponentInChildren<Animator>().speed = 0;
                
                if(combatTarget == CombatTarget.Hero_Attack)
                {
                    if (entitiesTr[i].gameObject.GetComponent<CEntity>() is CPlayer)
                    {
                        continue;
                    }

                    Animator animator = entitiesTr[i].gameObject.GetComponentInChildren<Animator>();
                    int hit = Animator.StringToHash("Hit");

                    animator.Play(hit);
                }
                else if(combatTarget == CombatTarget.Enemy_Attack)
                {
                    if (entitiesTr[i].gameObject.GetComponent<CEntity>() is CEnemy)
                    {
                        continue;
                    }

                    Animator animator = entitiesTr[i].gameObject.GetComponentInChildren<Animator>();
                    int hit = Animator.StringToHash("Hit");

                    animator.Play(hit);
                }
            }

            yield return null;
        }

        // TODO
        if (hasRiposeToken)
        {

        }

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            entitiesTr[i].gameObject.GetComponentInChildren<Animator>().speed = 1;
        }

        _combatUI.CloseCombatPanel();
        _combatUI.ToggleEntityStateRoot(true);
        _combatUI.ToggleTurnGuideRoot(true);

        delay = 3f;
        nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = 1f - (nextTime - Time.time) / delay;

            for (int i = 0; i < entitiesTr.Count; i++)
            {
                entitiesTr[i].position = Vector3.Lerp(entitiesTr[i].position, entitiesInitPos[i], t);
            }

            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, 5f, t);

            yield return null;
        }

        layer = LayerMask.NameToLayer(_defaultLayerName);

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            HighlightCharacter(entitiesTr[i].gameObject, layer);
        }


    }

    public IEnumerator CoHeroTemptation(CPlayer hero)
    {
        // 시험 메세지 열기
        _combatUI.OpenTemptation(hero);

        // 시험 메세지 연출
        RectTransform tr = _combatUI.HeroTemptationTextTr;
        tr.localScale = Vector3.one;

        float delay = 3f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float dt = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            tr.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, dt);
            yield return null;
        }

        // 사전작업
        _camera.orthographicSize = 1f;

        int layer = LayerMask.NameToLayer(_highlightLayerName);
        HighlightCharacter(hero.gameObject, layer);

        // 영웅 초기값 저장
        Vector3 initPos = hero.gameObject.transform.position;
        Quaternion initQuat = hero.gameObject.transform.rotation;
        Vector3 initScale = hero.gameObject.transform.localScale;

        Transform heroTr = hero.gameObject.transform;

        // 카메라 앞으로 영웅 끌어오기
        if (hero.HeroData.Id == "hero_man_at_arms")
        {
            heroTr.position = new Vector3(0, -1, 0);
        }
        else if (hero.HeroData.Id == "hero_highwayman")
        {
            heroTr.position = new Vector3(0.35f, -1.25f, 0);
        }
        else if (hero.HeroData.Id == "hero_grave_robber")
        {
            heroTr.position = new Vector3(0, -1.2f, 0);
        }
        else if (hero.HeroData.Id == "hero_plague_doctor")
        {
            heroTr.position = new Vector3(0.2f, -1.1f, 0);
        }


        // 연출 발생 사전작업
        CHeroStressResolution resolution = hero.StressResolution;
        int requestId = hero.StressResolution.RequestId;


        delay = 2f;
        nextTime = Time.time + delay;
        bool cameraBraveEffect = false;
        float bravePower = 5f;
        int tick = 0;

        Quaternion rot = initQuat * Quaternion.Euler(0, 0, 0.5f);
        Quaternion _cameraRot = _cameraTr.rotation;

        _combatUI.TogglePrefabRoot(false);

        // 붕괴 혹은 기상 시작
        int rand = Random.Range(0, 10);
        EHeroStressResult result = EHeroStressResult.Meltdown;

        if (rand < 5)
        {
            resolution.TryBeginResult(requestId, EHeroStressResult.Resolute);
            result = EHeroStressResult.Resolute;
            _combatUI.OpenResolute();
        }
        else
        {
            resolution.TryBeginResult(requestId, EHeroStressResult.Meltdown);
            result = EHeroStressResult.Meltdown;
            _combatUI.OpenMeltDown();
        }

        while (nextTime > Time.time)
        {
            float dt = 1f - Mathf.Clamp01((nextTime - Time.time) / delay);
            tick++;

            switch (result)
            {
                case EHeroStressResult.Meltdown:
                    heroTr.localRotation = Quaternion.Lerp(initQuat, rot, dt);
                    heroTr.localScale = Vector3.Lerp(initScale, initScale * 1.03f, dt);

                    if (tick > 30)
                    {
                        _cameraTr.rotation = Quaternion.Euler(cameraBraveEffect ? Random.Range(0, bravePower) / 100f : -Random.Range(0, bravePower) / 100f, 0, 0);
                        cameraBraveEffect = !cameraBraveEffect;
                        tick = Random.Range(0, 5);
                    }

                    yield return null;
                    break;
                case EHeroStressResult.Resolute:
                    heroTr.localScale = Vector3.Lerp(initScale, initScale * 1.03f, dt);
                    yield return null;
                    break;
            }
        }

        _combatUI.TogglePrefabRoot(true);
        _cameraTr.rotation = _cameraRot;

        _combatUI.ResetCombatPanel();

        // 영웅 원래위치로 돌리기
        delay = 2f;
        nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float dt = 1f - Mathf.Clamp01((nextTime - Time.time) / delay);

            heroTr.position = Vector3.Lerp(heroTr.position, initPos, dt);
            heroTr.rotation = Quaternion.Lerp(heroTr.rotation, initQuat, dt);
            heroTr.localScale = Vector3.Lerp(heroTr.localScale, initScale, dt);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, 5f, dt);
            yield return null;
        }

        heroTr.position = initPos;
        heroTr.rotation = initQuat;
        heroTr.localScale = initScale;
        _camera.orthographicSize = 5f;

        layer = LayerMask.NameToLayer(_defaultLayerName);
        HighlightCharacter(hero.gameObject, layer);
        resolution.TryApplyResult(requestId);
    }

    private void HighlightCharacter(GameObject target, int layer)
    {
        target.layer = layer;

        for (int i = 0; i < target.transform.childCount; i++)
        {
            HighlightCharacter(target.transform.GetChild(i).gameObject, layer);
        }
    }
}
