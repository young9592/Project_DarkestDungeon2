using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero/Skill Feedback", fileName = "HeroFeedbackSO_")]
public class CHeroSkillFeedbackSO : ScriptableObject
{
    [Header("Visuals")]
    [SerializeField] private CHeroFeedbackVisual _launch;
    [SerializeField] private CHeroFeedbackVisual _projectile;
    [SerializeField] private CHeroFeedbackVisual _impact;
    [SerializeField, Range(0.0f, 0.8f)] private float _releaseTime = 0.1f;
    [SerializeField, Range(1, 3)] private int _shotCount = 1;
    [SerializeField, Range(0.05f, 0.25f)] private float _shotSpacing = 0.15f;
    [SerializeField, Min(0.0f)] private float _arcHeight;
    [Header("Audio")]
    [SerializeField] private AudioClip _launchSound;
    [SerializeField] private AudioClip _impactSound;
    [SerializeField, Range(0.0f, 1.0f)] private float _volume = 0.5f;

    public CHeroFeedbackVisual Launch { get { return _launch; } }
    public CHeroFeedbackVisual Projectile { get { return _projectile; } }
    public CHeroFeedbackVisual Impact { get { return _impact; } }
    public float ReleaseTime { get { return _releaseTime; } }
    public int ShotCount { get { return _shotCount; } }
    public float ShotSpacing { get { return _shotSpacing; } }
    public float ArcHeight { get { return _arcHeight; } }
    public AudioClip LaunchSound { get { return _launchSound; } }
    public AudioClip ImpactSound { get { return _impactSound; } }
    public float Volume { get { return _volume; } }
}
