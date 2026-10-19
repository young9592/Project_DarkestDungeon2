using UnityEngine;

/// <summary>
/// 전투 중 대상에게 부여되어 턴 계산하고 효과 적용시키는 런타임 객체
/// </summary>
public class CEffect
{
	#region Fields
	private CEffectSO _effectSO = null;
	private int _remainingTurns = 0;
	private int _currentDamage = 0; // 중첩 도트 데미지 관리
	#endregion

	#region Property
	public EEffectType EffectType => _effectSO.EffectType;
	public int RemainingTurns => _remainingTurns;
	public int CurrentDamage => _currentDamage;
	public CEffectSO Data => _effectSO;
	#endregion

	public CEffect(CEffectSO effectSO)
	{
		_effectSO = effectSO;
		_remainingTurns = effectSO.Duration;
		_currentDamage = effectSO.Damage;
	}

	// 동일한 DOT가 또 걸렸을 시 데미지를 합산하고 지속 시간을 초기화하는 함수
	public void StackEffect(CEffectSO newEffectSO)
	{
		_currentDamage += newEffectSO.Damage;
		_remainingTurns = Mathf.Max(_remainingTurns, newEffectSO.Duration);
	}

	private void Execute(CEffectContext context)
	{
        if (_effectSO.Strategy != null)
        {
			_effectSO.Strategy.Apply(context, _effectSO);
        }
		else
		{
			CPrint.Warn($"{_effectSO.Name}에 연결된 Strategy가 없습니다.");
		}
    }

	public void DecreaseTurn()
	{
		if (RemainingTurns > 0)
		{
			_remainingTurns--;
		}
	}

}
