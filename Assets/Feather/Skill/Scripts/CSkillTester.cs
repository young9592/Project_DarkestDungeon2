using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Rendering;

public class CSkillTester : MonoBehaviour
{
    [Header("테스트 대상")]
    [SerializeField] private CPlayer _caster;
    [SerializeField] private CEntity _target;
    [SerializeField] private List<CEntity> _subTargets = new List<CEntity>();

    [Header("스킬 리스트")]
    [SerializeField] private List<CSkillDataSO> _skillList = new List<CSkillDataSO>();
    [SerializeField] private int _selectIndex = 0;

    [SerializeField] private CSkillDataSO _dummyAttackSkill;

    private readonly List<string> _guiLogs = new List<string>();
    private const int MAX_LOG_COUNT = 12;

    private static readonly EEffectType[] _displayTokenTypes = new EEffectType[]
    {
        EEffectType.Block, EEffectType.BlockPlus,
        EEffectType.Dodge, EEffectType.DodgePlus,
        EEffectType.Strength, EEffectType.Weak,
        EEffectType.Crit, EEffectType.Vulnerable,
        EEffectType.Blind, EEffectType.Combo,
        EEffectType.Taunt, EEffectType.Speed, EEffectType.Stun
    };

    public CSkillDataSO CurrentSkill =>
        (_skillList != null && _skillList.Count > 0 && _selectIndex >= 0 && _selectIndex < _skillList.Count)
        ? _skillList[_selectIndex] : null;

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
        if (Input.GetKeyDown(KeyCode.Tab)) SelectNextSkill();
        if (Input.GetKeyDown(KeyCode.Alpha1)) ExecuteCurrentSkill();
        if (Input.GetKeyDown(KeyCode.Alpha2)) Setup_Combo();
        if (Input.GetKeyDown(KeyCode.Alpha3)) Setup_CritCondition();
        if (Input.GetKeyDown(KeyCode.Alpha4)) Setup_MissDominoCondition();
        if (Input.GetKeyDown(KeyCode.Alpha5)) Setup_TargetDeathsDoor();
        if (Input.GetKeyDown(KeyCode.Alpha6)) ExecuteDummyAttack();
        if (Input.GetKeyDown(KeyCode.Alpha7)) Setup_BlockToken();
        if (Input.GetKeyDown(KeyCode.Alpha8)) Setup_DodgeToken();
        if (Input.GetKeyDown(KeyCode.Alpha9)) NextTurn();
        if (Input.GetKeyDown(KeyCode.Alpha0)) CureDots();
        if (Input.GetKeyDown(KeyCode.R)) ResetAll();
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 10 };
        style.normal.textColor = Color.white;

        GUI.Box(new Rect(10, 10, 780, 500), "Skill Tester");

        GUILayout.BeginArea(new Rect(25, 40, 750, 450));

        GUILayout.Label("[단축키] Tab:스킬변경 | 1:시전자공격 | 2:타겟콤보 | 3:시전자치명타+힘 | 4:실명+회피+ | 5:죽음의문턱", style);
        GUILayout.Label("[방어/반격] 6:더미공격 | 7:양측 방어 | 8:양측 회피 | 9:턴 종료 | 0:도트 치유 | R:초기화", style);
        string skillName = CurrentSkill != null ? CurrentSkill.SkillName : "없음";
        int totalSkills = _skillList != null ? _skillList.Count : 0;
        int currentNumber = totalSkills > 0 ? (_selectIndex + 1) : 0;
        GUILayout.Label($"현재 선택된 스킬 ({currentNumber}/{totalSkills}) : < {skillName} >", style);
        GUILayout.Space(6);

        if (_caster != null)
        {
            int bleed = _caster.GetTotalDotDamageByType(EEffectType.Bleed);
            int blight = _caster.GetTotalDotDamageByType(EEffectType.Blight);
            string tokens = GetActiveTokensString(_caster);

            GUILayout.Label(
                $"▶ 시전자 [{_caster.name}] HP : {_caster.CurrentHp} | Stress : {_caster.CurrentStress}/10 | " +
                $"문턱: {_caster.IsDeathsDoor} | DOT(출혈:{bleed}/중독:{blight}", style);
        }

        if (_target is CPlayer tp)
        {
            int bleed = tp.GetTotalDotDamageByType(EEffectType.Bleed);
            int blight = tp.GetTotalDotDamageByType(EEffectType.Blight);
            string tokens = GetActiveTokensString(tp);
            GUILayout.Label(
                $"▶ 대상 [{tp.name}] HP : {tp.CurrentHp} | Stress : {tp.CurrentStress}/10 | " +
                $"문턱: {tp.IsDeathsDoor} | 사망: {tp.IsDead} | DOT(출혈:{bleed}/중독:{blight}", style);
        }
        else if (_target is CEnemy te)
        {
            string tokens = GetActiveTokensString(te);
            GUILayout.Label($"▶ 대상 [{te.name}] HP: {te.CurrentHp} | 사망: {te.IsDead}", style);
        }

        GUILayout.Space(10);
        GUILayout.Label("[실시간 스킬 연산 로그]", style);

        for (int i = 0; i < _guiLogs.Count; i++)
        {
            GUILayout.Label("ㆍ " + _guiLogs[i], style);
        }

        GUILayout.EndArea();
    }

    private string GetActiveTokensString(CEntity entity)
    {
        if (entity == null || entity.TokenManager == null) return "없음";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _displayTokenTypes.Length; i++)
        {
            EEffectType type = _displayTokenTypes[i];
            if (entity.TokenManager.HasToken(type))
            {
                sb.Append($"[{type}] ");
            }
        }

        return sb.Length > 0 ? sb.ToString() : "없음";
    }

    [ContextMenu("다음 스킬 선택")]
    public void SelectNextSkill()
    {
        if (_skillList == null || _skillList.Count == 0) return;
        _selectIndex = (_selectIndex + 1) % _skillList.Count;
        _guiLogs.Clear();
        CPrint.Log($"[스킬 변경] <{_selectIndex + 1}/{_skillList.Count}> <{CurrentSkill.SkillName}> 선택");
    }

    [ContextMenu("시전자 스킬 실행")]
    public void ExecuteCurrentSkill()
    {
        if (CEffectManager.Instance == null || _caster == null || _target == null || CurrentSkill == null) return;

        _guiLogs.Clear();

        if (CurrentSkill.IsGroupTarget && _subTargets != null && _subTargets.Count > 0)
        {
            List<CEntity> allTargets = new List<CEntity> { _target };
            for (int i = 0; i < _subTargets.Count; i++)
            {
                if (_subTargets[i] != null && !allTargets.Contains(_subTargets[i]))
                {
                    allTargets.Add(_subTargets[i]);
                }
            }

            CPrint.Log($"[광역 스킬 : <{CurrentSkill.SkillName}> (대상 {allTargets.Count}명]");
            CEffectManager.Instance.ExecuteSkillEffects(_caster, allTargets, CurrentSkill, () =>
            {
                CPrint.Log($"[<{CurrentSkill.SkillName}> 광역 연산 완료]");
            });
        }
        else
        {
            CPrint.Log($"[시전자 공격: <{CurrentSkill.SkillName}> ({_caster.name} → {_target.name})]");
            CEffectManager.Instance.ExecuteSkillEffects(_caster, _target, CurrentSkill, () =>
            {
                CPrint.Log($"[<{CurrentSkill.SkillName}> 연산 완료]");
            });
        }
    }

    [ContextMenu("더미 공격 실행")]
    public void ExecuteDummyAttack()
    {
        if (CEffectManager.Instance == null || _caster == null || _target == null) return;

        if (_target.IsDead)
        {
            CPrint.Log("[더미 사망 상태] R키를 눌러 초기화 해주세요.");
            return;
        }

        CSkillDataSO skillToUse = _dummyAttackSkill != null ? _dummyAttackSkill : CurrentSkill;
        if (skillToUse == null)
        {
            CPrint.Warn("[더미] 사용할 스킬 데이터가 없습니다.");
            return;
        }

        _guiLogs.Clear();
        CPrint.Log($"[더미 공격 실행: <{skillToUse.SkillName}> ({_target.name} → {_caster.name})]");
        CEffectManager.Instance.ExecuteSkillEffects(_target, _caster, skillToUse, () =>
        {
            CPrint.Log($"[더미 공격 <{skillToUse.SkillName}> 연산 완료]");
        });
    }

    public void NextTurn()
    {
        _guiLogs.Clear();
        CPrint.Log("[턴 시작]");

        if (_caster != null && !_caster.IsDead)
        {
            _caster.OnTurnStart();
        }

        if (_target is CPlayer targetPlayer && !targetPlayer.IsDead && targetPlayer != _caster)
        {
            targetPlayer.OnTurnStart();
        }    

        if (_subTargets != null)
        {
            for (int i = 0; i < _subTargets.Count; i++)
            {
                if (_subTargets[i] is CPlayer subPlayer && !subPlayer.IsDead)
                {
                    subPlayer.OnTurnStart();
                }
            }
        }
    }

    [ContextMenu("DOT 상태이상 치료")]
    public void CureDots()
    {
        _guiLogs.Clear();
        CPrint.Log("[DOT 치료 테스트]");

        if (_caster != null)
        {
            _caster.CureStatusEffect(EEffectType.None);
        }

        if (_target != null && _target != _caster)
        {
            _target.CureStatusEffect(EEffectType.None);
        }
    }

    [ContextMenu("타겟 Combo 토큰 부여")]
    public void Setup_Combo()
    {
        if (_target == null) return;
        _target.TokenManager.AddToken(EEffectType.Combo, 1);
    }

    [ContextMenu("시전자 Crit + 힘 토큰 부여")]
    public void Setup_CritCondition()
    {
        if (_caster == null) return;
        _caster.TokenManager.AddToken(EEffectType.Crit, 1);
        _caster.TokenManager.AddToken(EEffectType.Strength, 1);
    }

    [ContextMenu("시전자 실명 + 타겟 회피+ 부여")]
    public void Setup_MissDominoCondition()
    {
        if (_caster == null | _target == null) return;
        _caster.TokenManager.AddToken(EEffectType.Blind, 1);
        _target.TokenManager.AddToken(EEffectType.DodgePlus, 1);
    }

    [ContextMenu("타겟 죽음의 문턱")]
    public void Setup_TargetDeathsDoor()
    {
        if(_target is CPlayer tp && !tp.IsDeathsDoor)
        {
            tp.TakeDamage(tp.CurrentHp);
        }
    }

    [ContextMenu("방어 토큰 부여")]
    public void Setup_BlockToken()
    {
        _guiLogs.Clear();
        CPrint.Log("== [양측에 Block 토큰 1개씩 부여]");
        if (_caster != null) _caster.TokenManager.AddToken(EEffectType.Block, 1);
        if (_target != null) _target.TokenManager.AddToken(EEffectType.Block, 1);
    }

    [ContextMenu("회피 토큰 부여")]
    public void Setup_DodgeToken()
    {
        _guiLogs.Clear();
        CPrint.Log("== [양측에 Dodge 토큰 1개씩 부여]");
        if (_caster != null) _caster.TokenManager.AddToken(EEffectType.Dodge, 1);
        if (_target != null) _target.TokenManager.AddToken(EEffectType.Dodge, 1);
    }

    [ContextMenu("초기화")]
    public void ResetAll()
    {
        _guiLogs.Clear();
        if(_caster  != null)
        {
            _caster.ResetEntityState();
        }
        if (_target != null)
        {
            _target.ResetEntityState();
        }
        if (_subTargets != null)
        {
            for (int i = 0; i < _subTargets.Count; i++)
            {
                if (_subTargets[i] != null) _subTargets[i].ResetEntityState();
            }
        }
        CPrint.Log("[토큰 제거 및 체력/스트레스 회복 완료]");
    }
}
