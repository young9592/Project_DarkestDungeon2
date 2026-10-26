using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CSkillTester : MonoBehaviour
{
    [Header("테스트 대상")]
    [SerializeField] private CPlayer _caster;
    [SerializeField] private CEntity _target;

    [Header("스킬 리스트")]
    [SerializeField] private List<CSkillDataSO> _skillList = new List<CSkillDataSO>();
    [SerializeField] private int _selectIndex = 0;

    private readonly List<string> _guiLogs = new List<string>();
    private const int MAX_LOG_COUNT = 12;

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
        if (Input.GetKeyDown(KeyCode.R)) ResetAll();
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 10 };
        style.normal.textColor = Color.white;

        GUI.Box(new Rect(10, 10, 780, 500), "Skill Tester");

        GUILayout.BeginArea(new Rect(25, 40, 750, 450));

        GUILayout.Label("[단축키] Tab:스킬변경 | 1:스킬실행 | 2:타겟콤보 | 3:시전자치명타+힘 | 4:실명+회치 | 5:죽음의문턱 | R:초기화", style);
        string skillName = CurrentSkill != null ? CurrentSkill.SkillName : "없음";
        GUILayout.Label($"현재 선택된 스킬 ({_selectIndex + 1}/{_skillList.Count}) : < {skillName} >", style);
        GUILayout.Space(6);

        if (_caster != null)
        {
            int bleed = _caster.GetTotalDotDamageByType(EEffectType.Bleed);
            int blight = _caster.GetTotalDotDamageByType(EEffectType.Blight);
            GUILayout.Label(
                $"▶ 시전자 [{_caster.name}] HP : {_caster.CurrentHp} | Stress : {_caster.CurrentStress}/10 | " +
                $"문턱: {_caster.IsDeathsDoor} | DOT(출혈:{bleed}/중독:{blight}", style);
        }

        if (_target is CPlayer tp)
        {
            int bleed = tp.GetTotalDotDamageByType(EEffectType.Bleed);
            int blight = tp.GetTotalDotDamageByType(EEffectType.Blight);
            GUILayout.Label(
                $"▶ 대상 [{tp.name}] HP : {tp.CurrentHp} | Stress : {tp.CurrentStress}/10 | " +
                $"문턱: {tp.IsDeathsDoor} | 사망: {tp.IsDead} | DOT(출혈:{bleed}/중독:{blight}", style);
        }
        else if (_target is CEnemy te)
        {
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

    [ContextMenu("다음 스킬 선택")]
    public void SelectNextSkill()
    {
        if (_skillList == null || _skillList.Count == 0) return;
        _selectIndex = (_selectIndex + 1) % _skillList.Count;
        _guiLogs.Clear();
        CPrint.Log($"[스킬 변경] <{CurrentSkill.SkillName}> 선택");
    }

    [ContextMenu("현재 선택된 스킬 실행")]
    public void ExecuteCurrentSkill()
    {
        if (CEffectManager.Instance == null || _caster == null || _target == null || CurrentSkill == null) return;

        _guiLogs.Clear();
        CPrint.Log($"[스킬 실행: <{CurrentSkill.SkillName}>]");
        CEffectManager.Instance.ExecuteSkillEffects(_caster, _target, CurrentSkill, () =>
        {
            CPrint.Log($"[<{CurrentSkill.SkillName}> 연산 완료]");
        });
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

    [ContextMenu("초기화")]
    public void ResetAll()
    {
        _guiLogs.Clear();
        if(_caster  != null)
        {
            _caster.TokenManager.ClearAllTokens();
            _caster.TakeDamage(-999);
            _caster.ModifyStress(-10);
        }
        if (_target != null)
        {
            _target.TokenManager.ClearAllTokens();
            _target.TakeDamage(-999);
            _target.ModifyStress(-10);
        }
        CPrint.Log("[토큰 제거 및 체력/스트레스 회복 완료]");
    }
}
