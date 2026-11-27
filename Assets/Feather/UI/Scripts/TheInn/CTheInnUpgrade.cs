
using TMPro;
using UnityEngine;

public class CTheInnUpgrade : MonoBehaviour
{
    #region Inspector
    [Header("Hero Data")]
    [SerializeField] private CHeroDataSO[] _heroData = new CHeroDataSO[4];

    [Header("Hero Skills")]
    [SerializeField] private CHeroSkillSO[] _heroSkill0 = new CHeroSkillSO[5];
    [SerializeField] private CHeroSkillSO[] _heroSkill1 = new CHeroSkillSO[5];
    [SerializeField] private CHeroSkillSO[] _heroSkill2 = new CHeroSkillSO[5];
    [SerializeField] private CHeroSkillSO[] _heroSkill3 = new CHeroSkillSO[5];

    [Header("Skill Slots")]
    [SerializeField] private CSkillSlot[] _skillslots = new CSkillSlot[5];

    [Header("Skill Tooltips")]
    [SerializeField] private _skillTooltipsSlot[] _skillTooltipsSlot = new _skillTooltipsSlot[5];

    [Header("Mastery")]
    [SerializeField] private CHeroMasteryCatalogSO _masteryCatalog;

    [Header("Confirm / Cancel UI")]
    [SerializeField] private GameObject _confirmCancelUI;

    [Header("Mastery Point")]
    [SerializeField] private TMP_Text _masteryPointText;
    #endregion

    private CHeroInnMastery _heroInnMastery;

    private int _currentHeroIndex = 0;
    private string _currentHeroId;

    private void Start()
    {
        UpdateHeroSkill(_currentHeroIndex);
    }
 
    public bool TryOpenTraining()
    {
        if (_heroInnMastery == null)
        {
            _heroInnMastery = CHeroInnMastery.Instance;
        }

        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return false;
        }

        bool result = _heroInnMastery.TryOpenTraining();
        Debug.Log("강화 시작 결과: " + result);
        Debug.Log("강화 실패 이유: " + _heroInnMastery.Notice);
        if (result)
        {
            ShowConfirmCancel(false);
            UpdateMasteryPoint();
        }
        return result;
    }

    public bool TryMasterySkill(int skillIndex)
    {
        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return false;
        }

        if (string.IsNullOrEmpty(_currentHeroId)) return false;

        string skillId = GetSkillId(skillIndex);

        if (string.IsNullOrEmpty(skillId)) return false;

        bool result = _heroInnMastery.TryMaster(_currentHeroId, skillId);

        if (result)
        {
            ShowConfirmCancel(true);
            UpdateMasteryPoint();
        }
        else
        {
            Debug.Log(_heroInnMastery.Notice);
        }
        return result;
    }
    public string GetSkillId(int skillIndex)
    {
        if (_masteryCatalog == null)
        {
            Debug.Log("카탈로그 연결 없음");
            return null;
        }

        CHeroSkillSO[] skills = GetHeroSkillArray();

        if (skills == null) return null;

        if (skillIndex < 0 || skillIndex >= skills.Length)
            return null;

        if (skills[skillIndex] == null)
            return null;

        CHeroMasterySkill masterySkill = _masteryCatalog.Find(_currentHeroId, skills[skillIndex]);

        if (masterySkill == null)
        {
            Debug.Log("해당 스킬의 강화항목 못 찾음");
            return null;
        }
        return masterySkill.Id;
    }
    public void UpdateHeroSkill(int heroindex)
    {
        _currentHeroIndex = heroindex;

        if (heroindex < 0 || heroindex >= _heroData.Length)
            return;

        if (_heroData[heroindex] == null) return;

        _currentHeroId = _heroData[heroindex].Id;

        CHeroSkillSO[] skills = GetHeroSkillArray();

        if (skills == null) return;

        UpdateSkillTooltips(skills);

        for (int i = 0; i < _skillslots.Length; i++)
        {
            if (_skillslots[i] == null) continue;

            if (i >= skills.Length)
            {
                _skillslots[i].SetSkill(null);
                continue;
            }

            _skillslots[i].SetSkill(skills[i]);
        }
    }
    public void OnConfirmButton()
    {
        if (_heroInnMastery == null)
            _heroInnMastery = CHeroInnMastery.Instance;

        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return;
        }

        bool result = _heroInnMastery.TryConfirmTraining();

        if (result)
        {
            ShowConfirmCancel(false);
            UpdateMasteryPoint();

            Debug.Log("강화 확정 성공");
        }
        else
        {
            Debug.Log(_heroInnMastery.Notice);
        }
    }

    public void OnCloseUpgradeUI()
    {
        if (_heroInnMastery == null)
            _heroInnMastery = CHeroInnMastery.Instance;

        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return;
        }

        _heroInnMastery.CancelTraining();

        ShowConfirmCancel(false);
        UpdateMasteryPoint();

        Debug.Log("강화 UI 닫힘 - 강화 취소");
    }
    public void OnResetButton()
    {
        if (_heroInnMastery == null)
            _heroInnMastery = CHeroInnMastery.Instance;

        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return;
        }

        bool result = _heroInnMastery.TryResetTraining();

        if (result)
        {
            ShowConfirmCancel(false);
            UpdateMasteryPoint();

            Debug.Log("강화 선택 초기화 성공");
        }
        else
        {
            Debug.Log(_heroInnMastery.Notice);
        }
    }
    public void OnMasterySkillButton(int skillIndex)
    {
        TryMasterySkill(skillIndex);
    }
    private void UpdateSkillTooltips(CHeroSkillSO[] skills)
    {
        for (int i = 0; i < _skillTooltipsSlot.Length; i++)
        {
            if (_skillTooltipsSlot[i] == null || skills[i] == null)
                continue;

            CHeroMasterySkill masteryData =
                _masteryCatalog.Find(_currentHeroId, skills[i]);

            if (masteryData == null)
            {
                _skillTooltipsSlot[i].SetTooltip(skills[i].SkillName, string.Empty, string.Empty);
                continue;
            }

            string baseDescription = masteryData.BaseReference;
            string masteredDescription = masteryData.MasteredReference;

            _skillTooltipsSlot[i].SetTooltip(skills[i].SkillName, baseDescription, masteredDescription);

        }
    }
    private void PrepareTestPartyForUI()
    {
        if (_heroInnMastery == null)
            _heroInnMastery = CHeroInnMastery.Instance;

        if (_heroInnMastery == null)
        {
            Debug.Log("강화 시스템 찾을 수 없음");
            return;
        }

        bool result = _heroInnMastery.TryPrepareTestParty();

        Debug.Log("테스트 파티 준비 결과: " + result);
        Debug.Log("테스트 파티 준비 메시지: " + _heroInnMastery.Notice);
    }

    private void TestEndSession()
    {
        if (_heroInnMastery == null)
            _heroInnMastery = CHeroInnMastery.Instance;

        if (_heroInnMastery != null)
        {
            _heroInnMastery.EndTestSession();
            ShowConfirmCancel(false);
            UpdateMasteryPoint();
        }
    }
    private void ShowConfirmCancel(bool isShow)
    {
        _confirmCancelUI.SetActive(isShow);
    }
    private void UpdateMasteryPoint()
    {
        if (_heroInnMastery == null) return;

        _masteryPointText.text = _heroInnMastery.Points.ToString();
    }
    private CHeroSkillSO[] GetHeroSkillArray()
    {
        switch (_currentHeroIndex)
        {
            case 0:
                return _heroSkill0;
            case 1:
                return _heroSkill1;
            case 2:
                return _heroSkill2;
            case 3:
                return _heroSkill3;
        }
        return null;
    }
}
