
using UnityEngine;

public class CEffect
{
	#region Inspector
	[SerializeField] private CEffectSO _effectSO = null;
	[SerializeField] private int _remainingTurns = 0;
	#endregion

	#region Property
	public EEffectType EffectType => _effectSO.EffectType;
	public int RemainingTurns => _remainingTurns;
	#endregion
}
