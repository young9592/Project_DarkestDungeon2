using UnityEngine;

public class CFanaticIgnition : MonoBehaviour
{
    [SerializeField] private CSkillDataSO _igniteSkill;
    [SerializeField] private CEnemyDataSO _ignitedData;
    [SerializeField] private CSkillDataSO _allySkill;
    [SerializeField] private CSkillDataSO _stealthIgnoringSkill;
    [Header("Ignition Presentation")]
    [SerializeField] private GameObject[] _igniteEffects;
    [SerializeField] private GameObject[] _burningEffects;

    private int _normalTurns;
    private bool _isIgnited;

    public CSkillDataSO AvailableIgnition
    {
        get { return !_isIgnited && _normalTurns >= 2 && _ignitedData != null ? _igniteSkill : null; }
    }

    public bool IsAllySkill(CSkillDataSO skill)
    {
        return _isIgnited && skill != null && skill == _allySkill;
    }

    public bool CanTargetStealth(CSkillDataSO skill)
    {
        return _isIgnited && skill != null && skill == _stealthIgnoringSkill;
    }

    public void BeginTurn()
    {
        if (!_isIgnited && _normalTurns < 2) _normalTurns++;
    }

    public CEnemyDataSO TryIgnite(CSkillDataSO skill)
    {
        if (skill == null || skill != AvailableIgnition) return null;
        _isIgnited = true;
        StopIgnitionEffects();
        SetEffectsActive(_burningEffects, true);
        return _ignitedData;
    }

    public void PlayIgnitionEffects()
    {
        SetEffectsActive(_igniteEffects, true);
    }

    public void StopIgnitionEffects()
    {
        SetEffectsActive(_igniteEffects, false);
    }

    public void HideEffects()
    {
        StopIgnitionEffects();
        SetEffectsActive(_burningEffects, false);
    }

    public void ResetIgnition()
    {
        HideEffects();
        _normalTurns = 0;
        _isIgnited = false;
    }

    private void OnEnable()
    {
        SetEffectsActive(_burningEffects, _isIgnited);
    }

    private void OnDisable()
    {
        HideEffects();
    }

    private void SetEffectsActive(GameObject[] effects, bool active)
    {
        if (effects == null) return;

        for (int i = 0; i < effects.Length; i++)
        {
            GameObject effect = effects[i];
            if (effect == null || effect.activeSelf == active) continue;

            effect.SetActive(active);
            if (!active) continue;

            ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < systems.Length; j++)
            {
                ParticleSystem system = systems[j];
                if (system.TryGetComponent(out ParticleSystemRenderer renderer) && renderer.enabled)
                    system.Play(false);
            }
        }
    }
}
