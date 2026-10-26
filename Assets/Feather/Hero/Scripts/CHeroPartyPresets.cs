using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CHeroSavedSkillBinding
{
    [SerializeField] private string _id;
    [SerializeField] private CSkillDataSO _skill;
    public string Id { get { return _id; } }
    public CSkillDataSO Skill { get { return _skill; } }
}

public class CHeroPartyPresets : MonoBehaviour
{
    [Header("Party")]
    [SerializeField] private CHeroPartyTest _party;
    [SerializeField] private CHeroPreparationPanel _preparation;
    [SerializeField] private CPlayer[] _heroPrefabs;
    [SerializeField] private CHeroSavedSkillBinding[] _skillBindings;
    [Header("Temp_Lee Presets")]
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _open;
    [SerializeField] private Button _close;
    [SerializeField] private Button _lastApplied;
    [SerializeField] private InputField[] _names;
    [SerializeField] private Text[] _summaries;
    [SerializeField] private Button[] _save;
    [SerializeField] private Button[] _load;
    [SerializeField] private Button[] _delete;
    [SerializeField] private Text _message;
    [SerializeField] private GameObject _confirmation;
    [SerializeField] private Text _confirmationText;
    [SerializeField] private Button _confirm;
    [SerializeField] private Button _dismiss;
    private CHeroPartySaveStore _store;
    private CHeroPartySaveData _data;
    private readonly List<ScriptableObject> _restoredAssets = new List<ScriptableObject>();
    private Action _pending;
    private string _notice;

    public string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "TempLee", "party-presets.json"); }
    }

    public bool IsOpen { get { return _root != null && _root.activeSelf; } }
    public string Notice { get { return _notice; } }

    private void Awake()
    {
        _store = new CHeroPartySaveStore(SavePath);
        if (_party == null || _preparation == null || _root == null || _open == null || _close == null
            || _lastApplied == null || _message == null || _confirmation == null || _confirmationText == null
            || _confirm == null || _dismiss == null || _names == null || _summaries == null
            || _save == null || _load == null || _delete == null || _names.Length != 3 || _summaries.Length != 3
            || _save.Length != 3 || _load.Length != 3 || _delete.Length != 3)
        {
            Debug.LogWarning("Party preset references are missing.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            if (_names[i] == null || _summaries[i] == null || _save[i] == null || _load[i] == null || _delete[i] == null)
            {
                Debug.LogWarning("Party preset slot references are missing.", this);
                enabled = false;
                return;
            }
        }

        _open.onClick.AddListener(Open);
        _close.onClick.AddListener(Close);
        _lastApplied.onClick.AddListener(() => LoadLastApplied());
        _confirm.onClick.AddListener(() =>
        {
            Action operation = _pending;
            Dismiss();
            operation?.Invoke();
        });
        _dismiss.onClick.AddListener(Dismiss);
        for (int i = 0; i < 3; i++)
        {
            int slot = i;
            _save[i].onClick.AddListener(() => RequestSave(slot));
            _load[i].onClick.AddListener(() => LoadPreset(slot));
            _delete[i].onClick.AddListener(() => RequestDelete(slot));
        }

        _root.SetActive(false);
        _confirmation.SetActive(false);
    }

    private bool Read()
    {
        bool valid = _store.TryRead(out _data, out string message);
        if (!valid)
        {
            SetNotice(message);
        }

        return valid;
    }

    private bool Commit(CHeroPartySaveData next)
    {
        if (!_store.TryWrite(next, out string message))
        {
            SetNotice(message);
            return false;
        }

        _data = next;
        return true;
    }

    public bool TryCapture(IReadOnlyList<CPlayer> prefabs, IReadOnlyList<ERank> ranks,
        IReadOnlyList<List<CSkillDataSO>> skills, out CHeroSavedParty saved)
    {
        saved = null;
        if (prefabs == null || ranks == null || skills == null || prefabs.Count != 4 || ranks.Count != 4 || skills.Count != 4)
        {
            return false;
        }

        CHeroSavedParty next = new CHeroSavedParty { IsSaved = true, Members = new CHeroSavedMember[4] };
        for (int i = 0; i < 4; i++)
        {
            if (prefabs[i] == null || prefabs[i].HeroData == null || skills[i] == null)
            {
                return false;
            }

            CHeroSavedMember member = new CHeroSavedMember
            {
                HeroId = prefabs[i].HeroData.Id,
                Rank = (int)ranks[i],
                SkillIds = new string[skills[i].Count]
            };
            for (int j = 0; j < skills[i].Count; j++)
            {
                member.SkillIds[j] = FindSkillId(skills[i][j]);
            }

            next.Members[i] = member;
        }

        if (!TryResolve(next, out _, out _, out _))
        {
            return false;
        }

        saved = next;
        return true;
    }

    private string FindSkillId(CSkillDataSO skill)
    {
        if (_skillBindings == null) return null;
        for (int i = 0; i < _skillBindings.Length; i++)
        {
            if (_skillBindings[i] != null && _skillBindings[i].Skill == skill)
            {
                return _skillBindings[i].Id;
            }
        }

        return null;
    }

    public bool TryResolve(CHeroSavedParty saved, out List<CPlayer> prefabs,
        out List<ERank> ranks, out List<List<CSkillDataSO>> skills)
    {
        prefabs = new List<CPlayer>();
        ranks = new List<ERank>();
        skills = new List<List<CSkillDataSO>>();
        if (saved == null || !saved.IsSaved || saved.Members == null || saved.Members.Length != 4 || _heroPrefabs == null || _skillBindings == null)
        {
            return false;
        }

        ERank occupied = ERank.None;
        for (int i = 0; i < 4; i++)
        {
            CHeroSavedMember member = saved.Members[i];
            if (member == null || string.IsNullOrWhiteSpace(member.HeroId) || member.SkillIds == null
                || member.SkillIds.Length < 1 || member.SkillIds.Length > 5)
            {
                return false;
            }

            CPlayer prefab = null;
            for (int j = 0; j < _heroPrefabs.Length; j++)
            {
                CPlayer candidate = _heroPrefabs[j];
                if (candidate != null && candidate.HeroData != null && candidate.HeroData.Id == member.HeroId)
                {
                    if (prefab != null) return false;
                    prefab = candidate;
                }
            }

            ERank rank = (ERank)member.Rank;
            if (prefab == null || prefabs.Contains(prefab) || !CSkillRankUtility.IsSingleRank(rank) || (occupied & rank) != ERank.None)
            {
                return false;
            }

            List<CSkillDataSO> equipped = new List<CSkillDataSO>();
            for (int j = 0; j < member.SkillIds.Length; j++)
            {
                if (string.IsNullOrWhiteSpace(member.SkillIds[j])) return false;
                CSkillDataSO skill = null;
                for (int k = 0; k < _skillBindings.Length; k++)
                {
                    CHeroSavedSkillBinding binding = _skillBindings[k];
                    if (binding != null && binding.Id == member.SkillIds[j])
                    {
                        if (skill != null) return false;
                        skill = binding.Skill;
                    }
                }

                bool available = false;
                IReadOnlyList<CSkillDataSO> availableSkills = prefab.HeroData.AvailableSkills;
                if (availableSkills != null)
                {
                    for (int k = 0; k < availableSkills.Count; k++)
                    {
                        if (availableSkills[k] == skill && skill != null) available = true;
                    }
                }

                if (!available || equipped.Contains(skill)) return false;
                equipped.Add(skill);
            }

            prefabs.Add(prefab);
            ranks.Add(rank);
            skills.Add(equipped);
            occupied |= rank;
        }

        return true;
    }

    public CHeroPartySO RestoreLastApplied()
    {
        ReleaseRestoredAssets();
        if (!Read() || _data.LastApplied == null) return null;
        if (!TryResolve(_data.LastApplied, out List<CPlayer> prefabs, out List<ERank> ranks, out List<List<CSkillDataSO>> skills))
        {
            SetNotice("Saved Heroes or skills are unavailable. Default party loaded.");
            return null;
        }

        List<CHeroPartyMember> members = new List<CHeroPartyMember>();
        for (int i = 0; i < 4; i++)
        {
            CHeroSkillLoadoutSO loadout = CHeroSkillLoadoutSO.CreateRuntime(prefabs[i].HeroData, skills[i]);
            _restoredAssets.Add(loadout);
            members.Add(new CHeroPartyMember(prefabs[i], ranks[i], loadout));
        }

        CHeroPartySO party = CHeroPartySO.CreateRuntime(members);
        _restoredAssets.Add(party);
        SetNotice("Saved party restored.");
        return party;
    }

    public bool SaveApplied(CHeroSavedParty saved)
    {
        if (!TryResolve(saved, out _, out _, out _) || !Read()) return false;
        _data.LastApplied = saved;
        if (!Commit(_data)) return false;
        SetNotice("Party applied and saved.");
        return true;
    }

    public void Open()
    {
        if (!enabled || _preparation == null || !_preparation.IsOpen) return;
        Read();
        Refresh();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        Dismiss();
    }

    public void Close()
    {
        Dismiss();
        _root.SetActive(false);
    }

    public bool HandleEscape()
    {
        if (!IsOpen) return false;
        if (_confirmation.activeSelf) Dismiss();
        else Close();
        return true;
    }

    public void RequestSave(int slot)
    {
        if (!IsOpen || !ValidSlot(slot) || !Read()) return;
        if (_data.Presets[slot] != null)
        {
            Ask("Replace preset " + (slot + 1) + "?", () => SavePreset(slot));
        }
        else
        {
            SavePreset(slot);
        }
    }

    public bool SavePreset(int slot)
    {
        if (!IsOpen || !ValidSlot(slot) || !Read()) return false;
        if (!_preparation.TryCaptureParty(out CHeroSavedParty saved))
        {
            SetNotice("Each Hero needs 1-5 skills before saving.");
            return false;
        }

        string name = _names[slot].text.Trim();
        saved.Name = string.IsNullOrWhiteSpace(name) ? "Preset " + (slot + 1) : name.Substring(0, Mathf.Min(24, name.Length));
        _data.Presets[slot] = saved;
        if (!Commit(_data)) return false;
        SetNotice("Preset saved. Apply the party separately.");
        Refresh();
        return true;
    }

    public bool LoadPreset(int slot)
    {
        if (!IsOpen || !ValidSlot(slot) || !Read()) return false;
        return LoadDraft(_data.Presets[slot]);
    }

    public bool LoadLastApplied()
    {
        return IsOpen && Read() && LoadDraft(_data.LastApplied);
    }

    private bool LoadDraft(CHeroSavedParty saved)
    {
        if (!_preparation.TryLoadSavedParty(saved))
        {
            SetNotice("Preset is empty or has unavailable Heroes or skills. Party kept.");
            return false;
        }

        Close();
        SetNotice("Preset loaded into editor. Apply to use it or Cancel to keep your party.");
        return true;
    }

    public void RequestDelete(int slot)
    {
        if (!IsOpen || !ValidSlot(slot) || !Read() || _data.Presets[slot] == null) return;
        Ask("Delete preset " + (slot + 1) + "?", () => DeletePreset(slot));
    }

    private void DeletePreset(int slot)
    {
        if (!Read()) return;
        _data.Presets[slot] = null;
        if (!Commit(_data)) return;
        SetNotice("Preset deleted. Applied party kept.");
        Refresh();
    }

    private bool ValidSlot(int slot)
    {
        return slot >= 0 && slot < 3;
    }

    private void Ask(string text, Action operation)
    {
        _pending = operation;
        _confirmationText.text = text;
        _confirmation.SetActive(true);
        _confirmation.transform.SetAsLastSibling();
    }

    private void Dismiss()
    {
        _pending = null;
        _confirmation.SetActive(false);
    }

    private void Refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            CHeroSavedParty saved = _data != null && _data.Presets != null ? _data.Presets[i] : null;
            _names[i].SetTextWithoutNotify(saved != null ? saved.Name : "Preset " + (i + 1));
            _load[i].interactable = saved != null;
            _delete[i].interactable = saved != null;
            _summaries[i].text = Summary(saved);
        }

        _lastApplied.interactable = _data != null && _data.LastApplied != null;
        _message.text = _notice ?? "Save the edited party here. Loading does not apply it.";
    }

    private string Summary(CHeroSavedParty saved)
    {
        if (saved == null) return "Empty";
        if (!TryResolve(saved, out List<CPlayer> prefabs, out List<ERank> ranks, out List<List<CSkillDataSO>> skills))
        {
            return "Unavailable Heroes or skills";
        }

        string text = string.Empty;
        for (int rank = 1; rank <= 4; rank++)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((int)ranks[i] != 1 << (rank - 1)) continue;
                text += (text.Length > 0 ? "\n" : "") + rank + ": " + prefabs[i].HeroData.Name;
                for (int j = 0; j < skills[i].Count; j++)
                {
                    text += (j == 0 ? " / " : " · ") + skills[i][j].SkillName;
                }
            }
        }

        return text;
    }

    private void SetNotice(string text)
    {
        _notice = text;
        if (_message != null) _message.text = text;
        if (_preparation != null) _preparation.SetSaveNotice(text);
    }

    private void ReleaseRestoredAssets()
    {
        for (int i = 0; i < _restoredAssets.Count; i++)
        {
            if (_restoredAssets[i] != null) Destroy(_restoredAssets[i]);
        }

        _restoredAssets.Clear();
    }

    private void OnDestroy()
    {
        ReleaseRestoredAssets();
    }
}
