using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CInterfaceAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private CCombatAudioSO _sounds;
    [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.2f;
    private static CInterfaceAudio _instance;
    private AudioSource _source;
    private AudioSource _musicIntroSource;
    private AudioSource _musicLoopSource;
    private bool _titleMusic;
    public static CInterfaceAudio Instance { get { return _instance; } }
    private readonly Dictionary<CPlayer, List<CSkillDataSO>> _loadouts = new Dictionary<CPlayer, List<CSkillDataSO>>();
    private float _nextBind;
    private readonly Dictionary<string, int> _playedFrames = new Dictionary<string, int>();
    public event Action<string, AudioClip> OnSoundPlayed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (_instance != null) return;
        CCombatAudioSO sounds = Resources.Load<CCombatAudioSO>("InterfaceAudioSO");
        if (sounds == null) return;
        GameObject owner = new GameObject("Interface Audio");
        CInterfaceAudio audio = owner.AddComponent<CInterfaceAudio>();
        audio._sounds = sounds;
        audio.SyncMusic(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0.0f;
        _musicIntroSource = gameObject.AddComponent<AudioSource>();
        _musicLoopSource = gameObject.AddComponent<AudioSource>();
        _musicIntroSource.playOnAwake = false;
        _musicLoopSource.playOnAwake = false;
        _musicIntroSource.spatialBlend = 0.0f;
        _musicLoopSource.spatialBlend = 0.0f;
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _nextBind = 0.0f;
        _loadouts.Clear();
        if (mode == LoadSceneMode.Single) SyncMusic(scene.name);
    }

    private void SyncMusic(string sceneName)
    {
        bool title = sceneName == "Title" || sceneName == "Character_Information_Title"
            || sceneName == "ESC_Exit_Button_Title";
        if (title == _titleMusic) return;
        _titleMusic = title;
        _musicIntroSource.Stop();
        _musicLoopSource.Stop();
        if (!title || _sounds == null) return;
        AudioClip intro = _sounds.GetSound("title_intro");
        AudioClip loop = _sounds.GetSound("title_loop");
        if (intro == null || loop == null) return;
        double start = AudioSettings.dspTime + 0.05;
        _musicIntroSource.clip = intro;
        _musicIntroSource.loop = false;
        _musicIntroSource.volume = _musicVolume;
        _musicLoopSource.clip = loop;
        _musicLoopSource.loop = true;
        _musicLoopSource.volume = _musicVolume;
        _musicIntroSource.PlayScheduled(start);
        _musicLoopSource.PlayScheduled(start + (double)intro.samples / intro.frequency);
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextBind) return;
        _nextBind = Time.unscaledTime + 0.5f;
        BindButtons();
        CPlayer[] heroes = FindObjectsOfType<CPlayer>();
        for (int i = 0; i < heroes.Length; i++)
        {
            if (!_loadouts.ContainsKey(heroes[i]) && heroes[i].EquippedSkills != null)
                _loadouts.Add(heroes[i], new List<CSkillDataSO>(heroes[i].EquippedSkills));
        }
    }

    private void LateUpdate()
    {
        foreach (KeyValuePair<CPlayer, List<CSkillDataSO>> entry in _loadouts)
        {
            if (entry.Key == null || !entry.Key.isActiveAndEnabled) continue;
            IReadOnlyList<CSkillDataSO> current = entry.Key.EquippedSkills;
            if (current == null) continue;
            bool added = false;
            bool removed = false;
            for (int i = 0; i < current.Count; i++)
                if (current[i] != null && !entry.Value.Contains(current[i])) added = true;
            for (int i = 0; i < entry.Value.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < current.Count; j++)
                    if (current[j] == entry.Value[i]) found = true;
                if (entry.Value[i] != null && !found) removed = true;
            }
            if (removed) Play("skill_unequip");
            if (added) Play("skill_equip");
            entry.Value.Clear();
            for (int i = 0; i < current.Count; i++) entry.Value.Add(current[i]);
        }
    }

    public void BindButtons()
    {
        CSlot[] inventory = FindObjectsOfType<CSlot>(true);
        for (int i = 0; i < inventory.Length; i++) BindSlot(inventory[i].gameObject);
        CStoreSlot[] store = FindObjectsOfType<CStoreSlot>(true);
        for (int i = 0; i < store.Length; i++) BindSlot(store[i].gameObject);
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            // 전투 버튼은 선택 확정 시점을 보는 전용 컴포넌트에서 재생함
            if (button.GetComponent<CInterfaceSlotAudio>() != null
                || button.GetComponent<CCombatUIButtonAudio>() != null
                || button.GetComponentInParent<CCombatUI>(true) != null
                || button.GetComponent<CInterfaceButtonAudio>() != null) continue;
            CInterfaceButtonAudio relay = button.gameObject.AddComponent<CInterfaceButtonAudio>();
            relay.Configure(this);
        }
    }

    private void BindSlot(GameObject slot)
    {
        if (slot.GetComponent<CInterfaceSlotAudio>() != null) return;
        slot.AddComponent<CInterfaceSlotAudio>().Configure(this);
    }

    public void Play(string key)
    {
        if (_sounds == null || _source == null) return;
        if (_playedFrames.TryGetValue(key, out int frame) && frame == Time.frameCount) return;
        AudioClip clip = _sounds.GetSound(key);
        if (clip == null) return;
        _playedFrames[key] = Time.frameCount;
        _source.PlayOneShot(clip, _sounds.Volume);
        OnSoundPlayed?.Invoke(key, clip);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (_instance == this) _instance = null;
    }
}
