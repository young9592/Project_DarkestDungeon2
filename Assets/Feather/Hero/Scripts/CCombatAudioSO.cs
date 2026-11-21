using System;
using UnityEngine;

[Serializable]
public class CCombatSoundEntry
{
    [SerializeField] private string _key;
    [SerializeField] private AudioClip _clip;

    public string Key { get { return _key; } }
    public AudioClip Clip { get { return _clip; } }
}

[Serializable]
public class CEnemySkillSound
{
    [SerializeField] private CSkillDataSO _skill;
    [SerializeField] private AudioClip _preparation;
    [SerializeField] private AudioClip _use;
    [SerializeField] private AudioClip _miss;
    [SerializeField] private AudioClip _return;
    [SerializeField] private AudioClip _comboPreparation;
    [SerializeField] private AudioClip _comboUse;
    [SerializeField] private AudioClip _comboMiss;
    [SerializeField] private AudioClip _comboReturn;

    public CSkillDataSO Skill { get { return _skill; } }
    public AudioClip Preparation { get { return _preparation; } }
    public AudioClip Use { get { return _use; } }
    public AudioClip Miss { get { return _miss; } }
    public AudioClip Return { get { return _return; } }
    public AudioClip ComboPreparation { get { return _comboPreparation; } }
    public AudioClip ComboUse { get { return _comboUse; } }
    public AudioClip ComboMiss { get { return _comboMiss; } }
    public AudioClip ComboReturn { get { return _comboReturn; } }
}

[Serializable]
public class CActorCombatSound
{
    [SerializeField] private CEntityDataSO _data;
    [SerializeField] private AudioClip _hurt;
    [SerializeField] private AudioClip _recovery;
    [SerializeField] private AudioClip _death;
    [SerializeField] private AudioClip _deathFoley;
    [SerializeField] private AudioClip _deathDoor;
    [SerializeField] private AudioClip _riposteReturn;
    [SerializeField] private CEnemySkillSound[] _skills;

    public CEntityDataSO Data { get { return _data; } }
    public AudioClip Hurt { get { return _hurt; } }
    public AudioClip Recovery { get { return _recovery; } }
    public AudioClip Death { get { return _death; } }
    public AudioClip DeathFoley { get { return _deathFoley; } }
    public AudioClip DeathDoor { get { return _deathDoor; } }
    public AudioClip RiposteReturn { get { return _riposteReturn; } }

    public CEnemySkillSound GetSkill(CSkillDataSO skill)
    {
        for (int i = 0; _skills != null && i < _skills.Length; i++)
        {
            if (_skills[i] != null && _skills[i].Skill == skill) return _skills[i];
        }
        return null;
    }
}

[CreateAssetMenu(menuName = "Data/Combat Audio", fileName = "CombatAudioSO")]
public class CCombatAudioSO : ScriptableObject
{
    [Header("Volume")]
    [SerializeField, Range(0.0f, 1.0f)] private float _volume = 0.5f;
    [Header("Actors")]
    [SerializeField] private CActorCombatSound[] _actors;
    [Header("Battle")]
    [SerializeField] private CCombatSoundEntry[] _sounds;

    public float Volume { get { return _volume; } }

    public CActorCombatSound GetActor(CEntityDataSO data)
    {
        for (int i = 0; _actors != null && i < _actors.Length; i++)
        {
            if (_actors[i] != null && _actors[i].Data == data) return _actors[i];
        }
        return null;
    }

    public AudioClip GetSound(string key)
    {
        for (int i = 0; _sounds != null && i < _sounds.Length; i++)
        {
            if (_sounds[i] != null && _sounds[i].Key == key) return _sounds[i].Clip;
        }
        return null;
    }
}
