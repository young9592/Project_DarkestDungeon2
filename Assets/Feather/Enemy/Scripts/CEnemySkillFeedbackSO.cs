using UnityEngine;

public enum EEnemyLaunchOrigin
{
    Body,
    Hand,
    Mouth
}

[CreateAssetMenu(menuName = "Data/Enemy/Skill Feedback", fileName = "EnemyFeedbackSO_")]
public class CEnemySkillFeedbackSO : ScriptableObject
{
    [Header("Visuals")]
    [SerializeField] private CHeroFeedbackVisual _launch;
    [SerializeField] private CHeroFeedbackVisual _projectile;
    [SerializeField] private CHeroFeedbackVisual _impact;
    [SerializeField] private EEnemyLaunchOrigin _origin;
    [SerializeField, Range(0.0f, 0.8f)] private float _releaseTime = 0.1f;
    [SerializeField, Range(1, 3)] private int _shotCount = 1;
    [SerializeField, Range(0.05f, 0.25f)] private float _shotSpacing = 0.15f;
    [SerializeField, Min(0.0f)] private float _arcHeight;

    [Header("Intent")]
    [SerializeField] private Color _intentColor = new Color(1.0f, 0.55f, 0.2f);

    public CHeroFeedbackVisual Launch { get { return _launch; } }
    public CHeroFeedbackVisual Projectile { get { return _projectile; } }
    public CHeroFeedbackVisual Impact { get { return _impact; } }
    public EEnemyLaunchOrigin Origin { get { return _origin; } }
    public float ReleaseTime { get { return _releaseTime; } }
    public int ShotCount { get { return _shotCount; } }
    public float ShotSpacing { get { return _shotSpacing; } }
    public float ArcHeight { get { return _arcHeight; } }
    public Color IntentColor { get { return _intentColor; } }
}
