using UnityEngine;

[CreateAssetMenu(menuName = "Data/Combat UI Audio", fileName = "CombatUIAudioSO")]
public class CCombatUIAudioSO : ScriptableObject
{
    [Header("Clips")]
    [SerializeField] private AudioClip _select;
    [SerializeField] private AudioClip _cancel;
    [SerializeField] private AudioClip _confirm;
    [SerializeField] private AudioClip _pass;
    [SerializeField] private AudioClip _invalid;
    [SerializeField] private AudioClip _hover;
    [SerializeField] private AudioClip _enemyHover;
    [SerializeField, Range(0.0f, 1.0f)] private float _volume = 0.45f;

    public AudioClip Select { get { return _select; } }
    public AudioClip Cancel { get { return _cancel; } }
    public AudioClip Invalid { get { return _invalid; } }
    public AudioClip Pass { get { return _pass != null ? _pass : _confirm; } }
    public AudioClip Confirm { get { return _confirm; } }
    public AudioClip Hover { get { return _hover; } }
    public AudioClip EnemyHover { get { return _enemyHover; } }
    public float Volume { get { return _volume; } }
}
