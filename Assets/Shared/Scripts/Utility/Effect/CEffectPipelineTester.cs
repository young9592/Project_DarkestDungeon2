using System.Collections.Generic;
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

    private readonly List<string> _guiLogs = new List<string>();
    private const int MAX_LOG_COUNT = 12;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string cleanLog = logString.Replace("\n", " ");
        _guiLogs.Add(cleanLog);

        if (_guiLogs.Count > MAX_LOG_COUNT)
        {
            _guiLogs.RemoveAt(0);
        }
    }

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

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        style.normal.textColor = Color.white;

        GUI.Box(new Rect(10, 10, 750, 480), "Effect Pipeline Tester");

        GUILayout.BeginArea(new Rect(25, 40, 720, 440));

        GUILayout.Label("[단축키] 1:스킬실행 | 2:토큰부여 | 3:턴시작 | 4:스트레스+4 | 5:죽음의 문턱 | 6:회복 | 7:6턴 중독", style);
        GUILayout.Space(8);

        if (_casterPlayer != null)
        {
            int dotBleed = _casterPlayer.GetTotalDotDamageByType(EEffectType.Bleed);
            int dotBlight = _casterPlayer.GetTotalDotDamageByType(EEffectType.Blight);

            GUILayout.Label(
                $"▶ 시전자 [{_casterPlayer.name}] HP: {_casterPlayer.CurrentHp} | Stress: {_casterPlayer.CurrentStress}/10 | " +
                $"죽음의 문턱: {_casterPlayer.IsDeathsDoor} | 사망: {_casterPlayer.IsDead} | DOT: (출혈:{dotBleed}/중독:{dotBlight})", style);
        }

        if (_targetEntity != null && _targetEntity != _casterPlayer)
        {
            if(_targetEntity is CPlayer tp)
            {
                int dotBleed = tp.GetTotalDotDamageByType(EEffectType.Bleed);
                int dotBlight = tp.GetTotalDotDamageByType(EEffectType.Blight);
                GUILayout.Label(
                $"▶ 시전자 [{tp.name}] HP: {tp.CurrentHp} | Stress: {tp.CurrentStress}/10 | " +
                $"죽음의 문턱: {tp.IsDeathsDoor} | 사망: {tp.IsDead} | DOT: (출혈:{dotBleed}/중독:{dotBlight})", style);
            }
            else if (_targetEntity is CEnemy te)
            {
                GUILayout.Label($"▶ 대상 [{te.name}] HP: {te.CurrentHp} | 사망: {te.IsDead}", style);
            } 
        }

        GUILayout.Space(10);
        GUILayout.Label("[실시간 진행 로그 (데미지 / 토큰 / 상태이상]", style);

        for (int i = 0; i <_guiLogs.Count; i++)
        {
            GUILayout.Label("ㆍ " + _guiLogs[i], style);
        }

        GUILayout.EndArea();
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

        if (!_casterPlayer.IsDeathsDoor)
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
