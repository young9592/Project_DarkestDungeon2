using UnityEngine;

[CreateAssetMenu(menuName = "Data/Battle Audio", fileName = "BattleAudioSO")]
public class CBattleAudioSO : ScriptableObject
{
    [Header("Music")]
    [SerializeField] private AudioClip _cityMusic;
    [SerializeField] private AudioClip _denialMusic;
    [SerializeField, Range(0.0f, 1.0f)] private float _musicVolume = 0.22f;
    [Header("Battle Cues")]
    [SerializeField] private AudioClip _battleStart;
    [SerializeField] private AudioClip _battleEnd;
    [SerializeField] private AudioClip _roundStart;
    [SerializeField] private AudioClip _heroTurn;
    [SerializeField] private AudioClip _enemyTurn;
    [SerializeField, Range(0.0f, 1.0f)] private float _cueVolume = 0.4f;

    public AudioClip CityMusic { get { return _cityMusic; } }
    public AudioClip DenialMusic { get { return _denialMusic; } }
    public float MusicVolume { get { return _musicVolume; } }
    public AudioClip BattleEnd { get { return _battleEnd; } }
    public AudioClip BattleStart { get { return _battleStart; } }
    public AudioClip RoundStart { get { return _roundStart; } }
    public AudioClip HeroTurn { get { return _heroTurn; } }
    public AudioClip EnemyTurn { get { return _enemyTurn; } }
    public float CueVolume { get { return _cueVolume; } }
}
