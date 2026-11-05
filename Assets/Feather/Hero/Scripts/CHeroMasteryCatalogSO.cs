using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CHeroMasterySkill
{
    [SerializeField] private string _id;
    [SerializeField] private CHeroDataSO _hero;
    [SerializeField] private CHeroSkillSO _baseSkill;
    [SerializeField] private CHeroSkillSO _masteredSkill;
    [SerializeField, Min(1)] private int _cost = 1;

    [Header("Reference")]
    [SerializeField] private bool _isAvailable = true;
    [SerializeField] private string _referenceUrl;
    [SerializeField] private string _baseReference;
    [SerializeField] private string _masteredReference;
    [SerializeField] private string _remainingEffects;

    public bool IsAvailable
    {
        get { return _isAvailable; }
    }
    public string ReferenceUrl
    {
        get { return _referenceUrl; }
    }
    public string BaseReference
    {
        get { return _baseReference; }
    }
    public string MasteredReference
    {
        get { return _masteredReference; }
    }
    public string RemainingEffects
    {
        get { return _remainingEffects; }
    }

    public string Id
    {
        get { return _id; }
    }
    public CHeroDataSO Hero
    {
        get { return _hero; }
    }
    public CHeroSkillSO BaseSkill
    {
        get { return _baseSkill; }
    }
    public CHeroSkillSO MasteredSkill
    {
        get { return _masteredSkill; }
    }
    public int Cost
    {
        get { return _cost; }
    }

    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(_id) || _hero == null || string.IsNullOrWhiteSpace(_hero.Id)
            || _baseSkill == null || _masteredSkill == null || _baseSkill == _masteredSkill
            || _baseSkill.BaseSkill != _baseSkill || _masteredSkill.BaseSkill != _baseSkill || _cost != 1)
        {
            return false;
        }

        IReadOnlyList<CSkillDataSO> skills = _hero.AvailableSkills;
        if (skills == null) return false;
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i] == _baseSkill) return true;
        }
        return false;
    }
}

[CreateAssetMenu(menuName = "Data/Hero/Mastery Catalog", fileName = "HeroMasteryCatalogSO_")]
public class CHeroMasteryCatalogSO : ScriptableObject
{
    [SerializeField] private List<CHeroMasterySkill> _skills = new List<CHeroMasterySkill>();

    public bool TryValidate()
    {
        if (_skills == null) return false;
        for (int i = 0; i < _skills.Count; i++)
        {
            CHeroMasterySkill entry = _skills[i];
            if (entry == null || !entry.IsValid()) return false;
            for (int j = 0; j < i; j++)
            {
                if (_skills[j].Id == entry.Id
                    || (_skills[j].Hero == entry.Hero && _skills[j].BaseSkill == entry.BaseSkill)) return false;
            }
        }
        return true;
    }

    public CHeroMasterySkill Find(string id)
    {
        if (_skills == null) return null;
        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i] != null && _skills[i].Id == id) return _skills[i];
        }
        return null;
    }

    public CHeroMasterySkill Find(string heroId, CSkillDataSO skill)
    {
        if (_skills == null || skill == null) return null;
        skill = CHeroSkillSO.GetBaseSkill(skill);
        for (int i = 0; i < _skills.Count; i++)
        {
            CHeroMasterySkill entry = _skills[i];
            if (entry != null && entry.Hero != null && entry.Hero.Id == heroId && entry.BaseSkill == skill) return entry;
        }
        return null;
    }
}
