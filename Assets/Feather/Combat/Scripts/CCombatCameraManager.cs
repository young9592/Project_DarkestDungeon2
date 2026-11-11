
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
    [SerializeField] private CCombatManager _combatManager;
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private Camera _camera;
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Option")]
    [SerializeField] private string _highlightLayerName = "Highlight";
    [SerializeField] private string _defaultLayerName = "Default";
    #endregion

    #region Field
    private HashSet<CEntity> _onceMessage = new HashSet<CEntity>();
    private Transform _cameraTr;
    private Vector3 _initVec;
    #endregion

    private void Awake()
    {
        if (CPrint.NullUnityObjectCheck(gameObject.name, _combatManager))
        {
            enabled = false;
            return;
        }

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

        if (CPrint.NullUnityObjectCheck(gameObject.name, _messageManager))
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
        // 이동 시키기 위해 transform 가져오기
        List<Transform> entitiesTr = new List<Transform>();
        // 카메라 이동방향
        CameraMoveDirection moveDir = CameraMoveDirection.None;
        // 반격 토큰을 가지고 있다면 중간에 화면을 다시 꺽어줘야합니다.
        CEntity riposteEntity = null;

        switch (combatTarget)
        {
            case CombatTarget.None:
                break;
            case CombatTarget.Hero_Attack:
                entitiesTr.Add(actor.gameObject.transform);

                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Right;

                // TODO : 상대에게 반격 토큰 있는지 체크 필요
                ERank minRank = ERank.None;

                for (int i = 0; i < entities.Count; i++)
                {
                    bool hasRiposte = entities[i].TokenManager.HasToken(EEffectType.Riposte);

                    if (hasRiposte)
                    {
                        if (minRank == ERank.None)
                        {
                            minRank = entities[i].CurrentRank;
                            riposteEntity = entities[i];
                            continue;
                        }

                        if (minRank > entities[i].CurrentRank)
                        {
                            minRank = entities[i].CurrentRank;
                            riposteEntity = entities[i];
                            continue;
                        }
                    }
                }

                break;
            case CombatTarget.Hero_SoloBuff:
                entitiesTr.Add(actor.gameObject.transform);
                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Hero_SingleTargetBuff:
                if (actor.CurrentRank > entities[0].CurrentRank)
                {
                    entitiesTr.Add(actor.gameObject.transform);
                    entitiesTr.Add(entities[0].gameObject.transform);
                }
                else
                {
                    entitiesTr.Add(entities[0].gameObject.transform);
                    entitiesTr.Add(actor.gameObject.transform);
                }
                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Hero_MultiTargetBuff:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Right;
                break;
            case CombatTarget.Enemy_Attack:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesTr.Add(entities[i].gameObject.transform);
                }
                entitiesTr.Add(actor.gameObject.transform);

                moveDir = CameraMoveDirection.Left;

                // TODO : 상대에게 반격 토큰 있는지 체크 필요

                // TODO : 상대에게 반격 토큰 있는지 체크 필요
                minRank = ERank.None;

                for (int i = 0; i < entities.Count; i++)
                {
                    bool hasRiposte = entities[i].TokenManager.HasToken(EEffectType.Riposte);

                    if (hasRiposte)
                    {
                        if (minRank == ERank.None)
                        {
                            minRank = entities[i].CurrentRank;
                            riposteEntity = entities[i];
                            continue;
                        }

                        if (minRank > entities[i].CurrentRank)
                        {
                            minRank = entities[i].CurrentRank;
                            riposteEntity = entities[i];
                            continue;
                        }
                    }
                }
                break;
            case CombatTarget.Enemy_SoloBuff:
                entitiesTr.Add(actor.gameObject.transform);
                moveDir = CameraMoveDirection.Left;
                break;
            case CombatTarget.Enemy_SingleTargetBuff:
                if (actor.CurrentRank < entities[0].CurrentRank)
                {
                    entitiesTr.Add(actor.gameObject.transform);
                    entitiesTr.Add(entities[0].gameObject.transform);
                }
                else
                {
                    entitiesTr.Add(entities[0].gameObject.transform);
                    entitiesTr.Add(actor.gameObject.transform);
                }
                moveDir = CameraMoveDirection.Left;

                break;
            case CombatTarget.Enemy_MultiTargetBuff:
                for (int i = 0; i < entities.Count; i++)
                {
                    entitiesTr.Add(entities[i].gameObject.transform);
                }

                moveDir = CameraMoveDirection.Left;
                break;

        }

        return StartCoroutine(CoAction(actor, entitiesTr, moveDir, combatTarget, riposteEntity));
    }

    private IEnumerator CoAction(CEntity actor, List<Transform> entitiesTr, CameraMoveDirection moveDir, CombatTarget combatTarget, CEntity riposteEntity)
    {
        _onceMessage.Clear();

        List<Vector3> camInitPos = new List<Vector3>();

        float space = 2f * Mathf.Clamp01((5f - entitiesTr.Count) / 4f) + 1f;

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            float x = (i - (entitiesTr.Count - 1) * 0.5f) * space;

            entitiesTr[i].position = new Vector3(x, -0.7f, 0);
            camInitPos.Add(entitiesTr[i].position);
        }

        // 준비
        float delay = riposteEntity == null ? 2f : 1f;
        float messageDelay = 0.1f;
        float nextTime = Time.time + delay;
        float nextMsgTime = Time.time + messageDelay;
        float move = moveDir == CameraMoveDirection.Right ? 0.5f : -0.5f;
        List<Vector3> moveVec = new List<Vector3>();
        _camera.orthographicSize = 2f;

        for (int i = 0; i < camInitPos.Count; i++)
        {
            moveVec.Add(camInitPos[i] + new Vector3(move, 0, 0));
        }

        // UI 정리
        _combatUI.ToggleEntityStateRoot(false);
        _combatUI.ToggleIndicatorRoot(false);
        _combatUI.HideAllIndicator();
        
        // 공격 방향에 따라 이펙트
        if (combatTarget == CombatTarget.Hero_Attack)
        {
            _combatUI.AttackRight();
        }
        else if (combatTarget == CombatTarget.Enemy_Attack)
        {
            _combatUI.AttackLeft();
        }

        // 강조대상 설정
        int layer = LayerMask.NameToLayer(_highlightLayerName);

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            HighlightCharacter(entitiesTr[i].gameObject, layer);
        }

        yield return null;

        // 재생
        while (nextTime > Time.time)
        {
            float t = 1f - (nextTime - Time.time) / delay;

            for (int i = 0; i < camInitPos.Count; i++)
            {
                entitiesTr[i].position = Vector3.Lerp(camInitPos[i], moveVec[i], t);

                GameObject entityObject = entitiesTr[i].gameObject;
                CEntity entity = entityObject.GetComponent<CEntity>();
                Animator animator = entityObject.GetComponentInChildren<Animator>();
                bool entityIsHero = entity is CPlayer;

                if (animator != null)
                {
                    animator.speed = 0;
                }

                if (nextMsgTime > Time.time)
                {
                    continue;
                }

                if (combatTarget == CombatTarget.Hero_Attack || combatTarget == CombatTarget.Enemy_Attack)
                {
                    if (_onceMessage.Contains(entity))
                    {
                        continue;
                    }

                    if (entityIsHero && combatTarget == CombatTarget.Enemy_Attack)
                    {
                        if (entity.IsDead)
                        {
                            _messageManager.ShowIcon(entityObject, CCombatFloatingIcon.IconType.DeathStemp);
                            _messageManager.ShowMessage(entityObject, CCombatFloatingText.EMessageType.Death, "죽음의 일격!");
                            _onceMessage.Add(entity);
                        }
                        else if (entity.IsDeathsDoor)
                        {
                            _messageManager.ShowMessage(entityObject, CCombatFloatingText.EMessageType.Death_Door, "죽음의 문턱!");
                            _onceMessage.Add(entity);
                        }
                    }
                    else if (!entityIsHero && combatTarget == CombatTarget.Hero_Attack)
                    {
                        if (entity is CEnemyCorpse)
                        {
                            continue;
                        }

                        if (entity.IsDead)
                        {
                            _messageManager.ShowIcon(entityObject, CCombatFloatingIcon.IconType.DeathStemp);
                            _messageManager.ShowMessage(entityObject, CCombatFloatingText.EMessageType.Death, "죽음의 일격!");
                            _onceMessage.Add(entity);
                        }
                    }
                }
            }
            yield return null;
        }






        // 반격 액션 재생 [ 임시 ]
        // 아직 전략 사용법을 몰라서 임시 처리하였습니다.
        if (riposteEntity != null && !riposteEntity.IsDead && (combatTarget == CombatTarget.Hero_Attack || combatTarget == CombatTarget.Enemy_Attack))
        {
            _onceMessage.Clear();
            moveVec.Clear();
            camInitPos.Clear();

            delay = 1f;
            nextTime = Time.time + delay;
            move *= -1;

            for (int i = 0; i < entitiesTr.Count; i++)
            {
                camInitPos.Add(entitiesTr[i].position);
                moveVec.Add(entitiesTr[i].position + new Vector3(move, 0, 0));
            }

            // 반대 방향으로 반격
            if (combatTarget == CombatTarget.Hero_Attack)
            {
                _combatUI.AttackLeft();
            }
            else if (combatTarget == CombatTarget.Enemy_Attack)
            {
                _combatUI.AttackRight();
            }

            actor.CancelAction();
            riposteEntity.CancelAction();
            
            // 데미지 처리
            bool isHero = riposteEntity is CPlayer;
            if (isHero)
            {
                riposteEntity.TokenManager.ConsumeToken(EEffectType.Riposte);
                int rand = Random.Range(1, 5);
                actor.TakeDamage(rand);
            }
            else
            {
                riposteEntity.TokenManager.ConsumeToken(EEffectType.Riposte);
                int rand = Random.Range(1, 5);
                actor.TakeDamage(rand);
            }

            yield return null;

            bool _once = false;

            while (nextTime > Time.time)
            {
                float t = 1f - (nextTime - Time.time) / delay;

                if (!_once)
                {
                    bool actorIsHero = actor is CPlayer;
                    GameObject actorObject = actor.gameObject;

                    if (actorIsHero && combatTarget == CombatTarget.Enemy_Attack)
                    {
                        if (actor.IsDead)
                        {
                            _messageManager.ShowIcon(actorObject, CCombatFloatingIcon.IconType.DeathStemp);
                            _messageManager.ShowMessage(actorObject, CCombatFloatingText.EMessageType.Death, "죽음의 일격!");
                            _onceMessage.Add(actor);
                        }
                        else if (actor.IsDeathsDoor)
                        {
                            _messageManager.ShowMessage(actorObject, CCombatFloatingText.EMessageType.Death_Door, "죽음의 문턱!");
                            _onceMessage.Add(actor);
                        }
                    }
                    else if (!actorIsHero && combatTarget == CombatTarget.Hero_Attack)
                    {
                        if (actor is CEnemyCorpse)
                        {
                            continue;
                        }

                        if (actor.IsDead)
                        {
                            _messageManager.ShowIcon(actorObject, CCombatFloatingIcon.IconType.DeathStemp);
                            _messageManager.ShowMessage(actorObject, CCombatFloatingText.EMessageType.Death, "죽음의 일격!");
                            _onceMessage.Add(actor);
                        }
                    }
                    _once = true;
                }

                for (int i = 0; i < camInitPos.Count; i++)
                {
                    entitiesTr[i].position = Vector3.Lerp(camInitPos[i], moveVec[i], t);

                    GameObject entityObject = entitiesTr[i].gameObject;
                    CEntity entity = entityObject.GetComponent<CEntity>();
                    Animator animator = entityObject.GetComponentInChildren<Animator>();
                    bool entityIsHero = entity is CPlayer;

                    if (animator != null)
                    {
                        animator.speed = 0;
                    }
                }
                yield return null;
            }
        }













        for (int i = 0; i < entitiesTr.Count; i++)
        {
            Animator animator = entitiesTr[i].gameObject.GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.speed = 1;
            }
        }

        _combatUI.CloseCombatPanel();
        _combatUI.ToggleIndicatorRoot(true);
        _combatUI.ToggleEntityStateRoot(true);
        _combatManager.MyTurnIndicator.ViewMyturnIndicator();

        delay = 2f;
        nextTime = Time.time + delay;

        // 돌아갈 위치 계산
        List<Vector3> returnVectorList = new List<Vector3>();
        for (int i = 0; i < entitiesTr.Count; i++)
        {
            CEntity entity = entitiesTr[i].gameObject.GetComponent<CEntity>();
            ERank rank = entity.CurrentRank;
            bool isHero = entity is CPlayer;
            int index = (int)System.MathF.Log((float)rank, 2);
            Vector3 returnVec = Vector3.zero;

            returnVec = ReturnVector(isHero, entity, index);
            returnVectorList.Add(returnVec);
        }

        while (nextTime > Time.time)
        {
            float t = 1f - (nextTime - Time.time) / delay;

            for (int i = 0; i < entitiesTr.Count; i++)
            {
                entitiesTr[i].position = Vector3.Lerp(entitiesTr[i].position, returnVectorList[i], t);
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
    private Vector3 ReturnVector(bool isHero, CEntity entity, int rank)
    {
        Vector3 returnVec = Vector3.zero;

        if (isHero)
        {
            returnVec = _combatManager.HeroRankPosition[rank].transform.position;
        }
        else
        {
            int size = (entity is CEnemy) ? (entity as CEnemy).EnemyData.FormationSize : (entity as CEnemyCorpse).FormationSize;

            for (int j = rank; j < rank + size; j++)
            {
                returnVec += _combatManager.EnemyRankPosition[j].transform.position;
            }

            returnVec = new Vector3(returnVec.x / size, returnVec.y / size, returnVec.z / size);
        }

        return returnVec;
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

        _cameraTr.rotation = _cameraRot;
        _combatUI.TogglePrefabRoot(true);
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
