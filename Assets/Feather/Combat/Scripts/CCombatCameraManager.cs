using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    #endregion

    #region Field
    private Transform _cameraTr;
    private Vector3 _initVec;
    private Coroutine _coroutine;
    private bool _meltdownCoroutineCompleted = false;
    #endregion

    #region Property
    public bool MeltdownCompleted => _meltdownCoroutineCompleted;
    #endregion

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
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
    public void CombatAction(CEntity actor, IReadOnlyList<CEntity> entities)
    {
        if (_coroutine != null)
        {
            return;
        }

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

        CombatActionProcess(combatTarget, actor, entities);
    }

    private void CombatActionProcess(CombatTarget combatTarget, CEntity actor, IReadOnlyList<CEntity> entities)
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

        _coroutine = StartCoroutine(CoAction(entitiesInitPos, entitiesTr, moveDir, combatTarget, hasRiposte));

    }

    private IEnumerator CoAction(List<Vector3> entitiesInitPos, List<Transform> entitiesTr, CameraMoveDirection moveDir, CombatTarget combatTarget, bool hasRiposeToken)
    {
        List<Vector3> camInitPos = new List<Vector3>();

        for (int i = 0; i < entitiesTr.Count; i++)
        {
            entitiesTr[i].position = new Vector3(entitiesTr.Count * 0.5f - Mathf.Abs(entitiesTr.Count - i), -1, 0);
            camInitPos.Add(entitiesTr[i].position);
        }

        float delay = 3f;
        float nextTime = Time.time + delay;
        float move = moveDir == CameraMoveDirection.Right ? 1f : -1f;
        List<Vector3> moveVec = new List<Vector3>();
        _camera.orthographicSize = 2f;

        for (int i = 0; i < camInitPos.Count; i++)
        {
            moveVec.Add(camInitPos[i] + new Vector3(move, 0, 0));
        }

        // 공격 방향에 따라 이펙트
        if (combatTarget == CombatTarget.Hero_Attack)
        {
            _combatUI.AttackRight();
        }
        else if (combatTarget == CombatTarget.Enemy_Attack)
        {
            _combatUI.AttackLeft();
        }

        while (nextTime > Time.time)
        {
            float t = 1f - (nextTime - Time.time) / delay;

            for (int i = 0; i < camInitPos.Count; i++)
            {
                entitiesTr[i].position = Vector3.Lerp(camInitPos[i], moveVec[i], t);
            }

            yield return null;
        }

        // TODO
        if (hasRiposeToken)
        {

        }

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

        _coroutine = null;

    }

    public void HeroMetlDown(CPlayer hero)
    {
        if (_coroutine != null)
        {
            return;
        }

        if (_meltdownCoroutineCompleted)
        {
            return;
        }

        _meltdownCoroutineCompleted = false;
        _coroutine = StartCoroutine(CoHeroTemptation(hero));
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

        // TODO : 확률에 따라 영웅의 기상 혹은 붕괴 연출 나눠야합니다.
        _combatUI.OpenMeltDown();

        // 붕괴 연출
        _camera.orthographicSize = 1f;


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


        // 임시 설정
        hero.ModifyStress(10);



        delay = 2f;
        nextTime = Time.time + delay;
        bool cameraBraveEffect = false;
        float bravePower = 5f;
        int tick = 0;

        Quaternion rot = initQuat * Quaternion.Euler(0, 0, 0.5f);
        Quaternion _cameraRot = _cameraTr.rotation;

        _combatUI.TogglePrefabRoot(false);

        while (nextTime > Time.time)
        {
            float dt = 1f - Mathf.Clamp01((nextTime - Time.time) / delay);
            tick++;

            // 연출
            heroTr.localRotation = Quaternion.Lerp(initQuat, rot, dt);
            heroTr.localScale = Vector3.Lerp(initScale, initScale * 1.03f, dt);

            if (tick > 30)
            {
                _cameraTr.rotation = Quaternion.Euler(cameraBraveEffect ? Random.Range(0, bravePower) / 100f : -Random.Range(0, bravePower) / 100f, 0, 0);
                cameraBraveEffect = !cameraBraveEffect;
                tick = Random.Range(0, 5);
            }

            yield return null;
        }

        _combatUI.TogglePrefabRoot(true);
        _cameraTr.rotation = _cameraRot;

        _combatUI.ResetCombatPanel();

        // 영웅 원래위치로 돌리기
        delay = 3f;
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

        _meltdownCoroutineCompleted = true;
        _coroutine = null;
    }
}
