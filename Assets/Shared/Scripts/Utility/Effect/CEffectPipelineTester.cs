using Unity.VisualScripting;
using UnityEngine;

public class CEffectPipelineTester : MonoBehaviour
{
    [Header("테스트 대상")]
    [SerializeField] private CPlayer _casterPlayer;
    [SerializeField] private CEntity _targetEntity;

    [Header("스킬 파이프라인 데이터")]
    [SerializeField] private CSkillDataSO _testSkill;

    [Header("도트 시물레이션")]
    [SerializeField] private CEffectSO _blight3TurnSO;
    [SerializeField] private CEffectSO _blight2TurnSO;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Skill();
        if (Input.GetKeyDown(KeyCode.Alpha2)) GrantToken();
        if (Input.GetKeyDown(KeyCode.Alpha3)) TurnStart();
        if (Input.GetKeyDown(KeyCode.Alpha4)) Stress();
        if (Input.GetKeyDown(KeyCode.Alpha5)) DeathsDoor();
        if (Input.GetKeyDown(KeyCode.Alpha6)) HealAndDeathsDoor();
        if (Input.GetKeyDown(KeyCode.Alpha7)) Dot();
    }

    [ContextMenu("1. 스킬 실행 [Key : 1]")]
    public void Skill()
    {
        if (CEffectManager.Instance == null || _casterPlayer == null || _targetEntity == null)
        {
            return;
        }

        CPrint.Log("== [스킬 파이프라인 테스트] ==");
        CEffectManager.Instance.ExecuteSkillEffects(_casterPlayer, _targetEntity, _testSkill, () =>
        {
            CPrint.Log("Effect 연산 종료");
        });
    }

    [ContextMenu("2. 토큰 부여 [Key : 2]")]
    public void GrantToken()
    {
        CPrint.Log("== [토큰 부여 테스트] ==");
        _casterPlayer.TokenManager.AddToken(EEffectType.Strength, 1);
        _casterPlayer.TokenManager.AddToken(EEffectType.Crit, 1);

        _casterPlayer.TokenManager.AddToken(EEffectType.Block, 1);
        _casterPlayer.TokenManager.AddToken(EEffectType.Vulnerable, 1);
        CPrint.Log("토큰 부여 완료, 데미지 변화를 확인하시오");
    }

    [ContextMenu("3. 도트 정산, 기절 체크 / 턴 시작 호출")]
    public void TurnStart()
    {
        CPrint.Log("== [턴 시작] ==");
        if (_targetEntity is CPlayer targetplayer)
        {
            targetplayer.OnTurnStart();
        }
        else
        {
            _casterPlayer.OnTurnStart();
        }
    }

    [ContextMenu("4. 스트레스 +4 증가")]
    public void Stress()
    {
        CPrint.Log("== [스트레스 증가 및 붕괴 테스트] ==");
        _casterPlayer.ModifyStress(4);
    }

    [ContextMenu("5. 죽음의 문턱 진입 및 죽음의 일격 테스트")]
    public void DeathsDoor()
    {
        CPrint.Log("== [죽음의 문턱 / 즉사 저항 테스트] ==");

        if (!_casterPlayer.IsDead)
        {
            CPrint.Log("죽음의 문턱 상태");
            _casterPlayer.TakeDamage(_casterPlayer.CurrentHp);
        }
        else
        {
            CPrint.Log("이미 죽음의 문턱 상태");
            _casterPlayer.TakeDamage(1);
        }
    }

    [ContextMenu("6. 체력 회복 + 죽음의 문턱 해제")]
    public void HealAndDeathsDoor()
    {
        CPrint.Log("== [체력 회복 및 문턱 탈출] ==");
        _casterPlayer.TakeDamage(-5);
    }

    [ContextMenu("7. 6턴 중독 (3중독 + 2중독) 테스트")]
    public void Dot()
    {
        if (_blight2TurnSO == null || _blight3TurnSO == null)
        {
            return;
        }

        CPrint.Log("== [도트 뎀 테스트] ==");

        CPrint.Log("-- [1턴] 전투 중 3중독");
        _casterPlayer.ApplyStatusEffect(new CEffect(_blight3TurnSO));

        CPrint.Log("-- [2턴] 턴 시작 정산 후 2중독 추가 피격");
        _casterPlayer.OnTurnStart();
        _casterPlayer.ApplyStatusEffect(new CEffect(_blight2TurnSO));

        CPrint.Log("-- [3턴] 전투 정산");
        _casterPlayer.OnTurnStart();

        CPrint.Log("-- [4턴] 전투 정산");
        _casterPlayer.OnTurnStart();

        CPrint.Log("-- [5턴] 전투 정산");
        _casterPlayer.OnTurnStart();

        CPrint.Log("-- [6턴] 전투 정산");
        _casterPlayer.OnTurnStart();
    }
}
