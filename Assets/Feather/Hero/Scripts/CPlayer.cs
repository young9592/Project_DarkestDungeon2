using System.Collections.Generic;
using UnityEngine;
using System;

public class CPlayer : CEntity
{
	#region Inspector
	[Header("Data")]
	[SerializeField] private CHeroDataSO _heroData;

	[Header("Runtime Status")]
	[SerializeField] private int _currentHp;
	[SerializeField] private int _currentStress;
	// 현재 플레이어가 위치한 열 테스트로 1로 설정함
	[SerializeField] private ERank _currentRank = ERank.Rank1;
	#endregion

	#region Field
	// 전투에 들고 온 장착할 스킬 리스트
	private List<CSkillDataSO> _equippedSkills = new List<CSkillDataSO>();
	// 유저가 클릭해서 실행 대기 중인 스킬
	private CSkillDataSO _selectedSkill;
	#endregion

	#region Events
	// 스킬 타케팅까지 완료되었을시 BattleCore에 '행동 실행' 요청 이벤트
	public event Action<CPlayer, CEntity, CSkillDataSO> OnSkillActionRequested;
    #endregion

    private void Awake()
    {
        if (CPrint.NullUnityObjectCheck(gameObject.name, _heroData))
		{
			enabled = false;
			return;
		}

		Initialize();
    }

	private void Initialize()
	{
		_currentHp = _heroData.MaxHealth;
		_currentStress = 0;

		// TODO : 실제 게임시 여관이나 메뉴에서 장착한 스킬만 가져와야 함
		// 현재는 SO에 등록되어있는 모든 스킬을 장착했다고 설정함
		if (_heroData.AvailableSkills != null)
		{
			foreach(var skill in _heroData.AvailableSkills)
			{
				_equippedSkills.Add(skill);
			}
		}
	}

	#region Skill Usage
	/// </summary>
	/// 1. 유저가 스킬 버튼 클릭시 호출
	/// </summary>
	public void SelectSkill(int skillIndex)
	{
		if (skillIndex < 0 || skillIndex >= _equippedSkills.Count)
		{
			CPrint.Warn("유효하지 않은 스킬 인덱스입니다.");
			return;
		}

		CSkillDataSO skillToUse = _equippedSkills[skillIndex];

		if (skillToUse.UsableRank.HasFlag(_currentRank))
		{
			_selectedSkill = skillToUse;
			CPrint.Log($"[{_heroData.DefaultName}] : <{_selectedSkill.SkillName}> 스킬 선택  완료. 타겟 지정해주세요.");

			// TODO : UI 매니저로 TargetableRank에 해당하는 적들의 발밑 하이라이트 처리
		}
		else
		{
			CPrint.Warn($"현재 위치({_currentRank})에서는 <{skillToUse.SkillName}> 스킬을 사용할 수 없습니다.");
		}
	}

	/// <summary>
	/// 2. 스킬이 선택된 상태에서 적 클릭했을 때의 호출
	/// </summary>
	public void ExecuteSkillToTarget(CEntity target, ERank targetRank)
	{
		if (_selectedSkill == null)
		{
            CPrint.Warn("선택된 스킬이 없습니다. 먼저 스킬을 선택해주세요.");
            return;
        }

		if (_selectedSkill.TargetableRank.HasFlag(targetRank))
		{
			CPrint.Log($"[{_heroData.DefaultName}]가 {target.name}({target.name})에게 <{_selectedSkill.SkillName}> 스킬 사용");

			// 데미지 수치 연산 처리 이벤트
			OnSkillActionRequested?.Invoke(this, target, _selectedSkill);

			_selectedSkill = null;
		}
		else
		{
			CPrint.Warn($"대상의 위치({targetRank})는 <{_selectedSkill.SkillName}> 스킬의 타겟이 될 수 없습니다.");
		}
	}

	/// <summary>
	/// 우클릭이나 취소 키 눌렀을 때 스킬 선택 취소
	/// </summary>
	public void CancelSkillSelection()
	{
		_selectedSkill = null;
		CPrint.Log("스킬 선택을 취소하였습니다.");
	}
	#endregion

	#region CEntity Overrides
	// Effect Core 파이프라인에서 최종 연산된 데미지를 받음
	public override void TakeDamage(int finalDamage)
	{
		_currentHp -= finalDamage;
		CPrint.Log($"[{_heroData.DefaultName}] 피격 \n 데미지 : {finalDamage}, 남은 체력: {_currentHp}");

		if (_currentHp <= 0)
		{
			_currentHp = 0;
			EnterDeathsDoor();
		}
	}

	// Effect Core 파이프라인에서 필터링시킨 상태이상을 적용받음
    public override void ApplyStatusEffect(CEffect statusEffect)
    {
		CPrint.Log($"[{_heroData.DefaultName}] 에게 상태이상 [{statusEffect.Data.Name}] 적용!");

		// TODO : 전달받은 상태이상 타입인 EffectType을 확인하여 부착시키거나 토큰을 누적시키는 로직 구현 필요
    }

	private void EnterDeathsDoor()
	{
		CPrint.Log($"[{_heroData.DefaultName}] 죽음의 문턱 진입 \n 다음 피격 시 생존 난수를 굴립니다.");
		// TODO : 죽음의 문턱 토글 켜기 및 능력치 감소 디버프등 적용
	}
	#endregion

}
