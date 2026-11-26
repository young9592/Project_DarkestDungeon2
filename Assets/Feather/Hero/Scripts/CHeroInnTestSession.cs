using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CHeroInnTestSession
{
    private readonly List<CPlayer> _heroes = new List<CPlayer>();
    private GameObject _root;
    private string _directory;

    public IReadOnlyList<CPlayer> Heroes { get { return _heroes; } }
    public CHeroMasteryService Mastery { get; private set; }
    public CHeroBattleStateService Service { get; private set; }
    public string SavePath { get; private set; }

    public bool TryPrepare(CHeroPartySO preset, CHeroMasteryCatalogSO catalog,
        Scene scene, int points, out string message)
    {
        message = "The UI test party or mastery catalog is unavailable.";
        if (_root != null || preset == null || preset.Members == null
            || preset.Members.Count != 4 || catalog == null || !catalog.TryValidate() || points < 0) return false;

        try
        {
            _directory = Path.Combine(Application.temporaryCachePath, "HeroInnUITests", Guid.NewGuid().ToString("N"));
            SavePath = Path.Combine(_directory, "expedition.json");
            Mastery = new CHeroMasteryService(catalog, Path.Combine(_directory, "mastery.json"));
            Service = new CHeroBattleStateService(SavePath, Mastery);
            if (!Service.TryLoad(out message)) return Fail();

            _root = new GameObject("Inn UI Test Party");
            _root.hideFlags = HideFlags.DontSave;
            _root.SetActive(false);
            _root.transform.position = new Vector3(0.0f, -10000.0f, 0.0f);
            SceneManager.MoveGameObjectToScene(_root, scene);
            for (int i = 0; i < preset.Members.Count; i++)
            {
                CHeroPartyMember member = preset.Members[i];
                if (member == null || member.Prefab == null || member.Loadout == null)
                {
                    message = "A UI test party member is missing.";
                    return Fail();
                }
                CPlayer hero = UnityEngine.Object.Instantiate(member.Prefab, _root.transform);
                hero.gameObject.hideFlags = HideFlags.DontSave;
                hero.gameObject.SetActive(true);
                Renderer[] renderers = hero.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renderers.Length; j++) renderers[j].enabled = false;
                AudioSource[] sources = hero.GetComponentsInChildren<AudioSource>(true);
                for (int j = 0; j < sources.Length; j++) sources[j].enabled = false;
                CEntityHealthView view = hero.GetComponent<CEntityHealthView>();
                if (view != null) view.enabled = false;
                _heroes.Add(hero);
            }
            _root.SetActive(true);
            for (int i = 0; i < _heroes.Count; i++)
            {
                CHeroPartyMember member = preset.Members[i];
                if (!_heroes[i].TrySetRank(member.Rank) || !_heroes[i].TryEquipLoadout(member.Loadout))
                {
                    message = "The UI test party could not equip its starting skills.";
                    return Fail();
                }
            }
            if (!Service.TryStartExpedition(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"),
                _heroes, points, out message)
                || !Service.TryCompleteBattle(Service.RunId, Service.BattleId, _heroes, out message)) return Fail();

            message = "UI test party ready. Real expedition saves are unchanged.";
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
        {
            message = "Could not prepare the UI test save.";
            return Fail();
        }
    }

    private bool Fail()
    {
        Clear();
        return false;
    }

    public void Clear()
    {
        if (_root != null)
        {
            _root.SetActive(false);
            UnityEngine.Object.Destroy(_root);
            _root = null;
        }
        _heroes.Clear();
        if (Mastery != null) Mastery.ReleaseRuntimeAssets();
        Mastery = null;
        Service = null;
        if (!string.IsNullOrEmpty(_directory))
        {
            try
            {
                string[] files = { "expedition.json", "expedition.json.bak", "expedition.json.tmp",
                    "mastery.json", "mastery.json.bak", "mastery.json.tmp" };
                for (int i = 0; i < files.Length; i++)
                {
                    string path = Path.Combine(_directory, files[i]);
                    if (File.Exists(path)) File.Delete(path);
                }
                if (Directory.Exists(_directory) && Directory.GetFileSystemEntries(_directory).Length == 0)
                    Directory.Delete(_directory);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogWarning("Could not remove an Inn UI test save from the temporary cache.");
            }
        }
        _directory = null;
        SavePath = null;
    }
}
