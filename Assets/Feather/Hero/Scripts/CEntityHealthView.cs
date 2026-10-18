using UnityEngine;

[DisallowMultipleComponent]
public class CEntityHealthView : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField, Min(40.0f)] private float _width = 72.0f;
    [SerializeField] private Color _heroColor = new Color(0.25f, 0.72f, 0.44f);
    [SerializeField] private Color _enemyColor = new Color(0.78f, 0.29f, 0.25f);
    private CPlayer _hero;
    private CEnemy _enemy;
    private Camera _camera;
    private GUIStyle _labelStyle;
    private int _previousHp;
    private int _change;
    private float _changeUntil;

    public bool ShowBar { get; set; } = true;

    public int CurrentHp
    {
        get { return _hero != null ? _hero.CurrentHp : _enemy != null ? _enemy.CurrentHp : 0; }
    }

    public int MaxHp
    {
        get
        {
            if (_hero != null && _hero.HeroData != null)
            {
                return _hero.HeroData.MaxHealth;
            }

            return _enemy != null && _enemy.EnemyData != null ? _enemy.EnemyData.MaxHealth : 0;
        }
    }

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _enemy = GetComponent<CEnemy>();
        _camera = Camera.main;
    }

    private void Start()
    {
        _previousHp = CurrentHp;
    }

    private void Update()
    {
        int hp = CurrentHp;
        if (hp == _previousHp)
        {
            return;
        }

        _change = hp - _previousHp;
        _previousHp = hp;
        _changeUntil = Time.unscaledTime + 1.2f;
    }

    private void OnGUI()
    {
        if (!ShowBar || _camera == null || MaxHp <= 0)
        {
            return;
        }

        Vector3 point = _camera.WorldToScreenPoint(transform.position);
        if (point.z <= 0.0f)
        {
            return;
        }

        float scale = Mathf.Clamp(Screen.height / 720.0f, 0.6f, 1.4f);
        Rect bar = new Rect(point.x - _width * scale * 0.5f, Screen.height - point.y + 3 * scale, _width * scale, 19 * scale);
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.fontStyle = FontStyle.Bold;
        }

        _labelStyle.fontSize = Mathf.RoundToInt(13 * scale);
        Color previous = GUI.color;
        GUI.color = new Color(0.03f, 0.04f, 0.05f, 0.95f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        Rect fill = new Rect(bar.x + scale, bar.y + scale, (bar.width - 2 * scale) * Mathf.Clamp01((float)CurrentHp / MaxHp), bar.height - 2 * scale);
        GUI.color = _hero != null ? _heroColor : _enemyColor;
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(bar, CurrentHp + " / " + MaxHp, _labelStyle);
        if (Time.unscaledTime < _changeUntil)
        {
            GUI.color = _change > 0 ? new Color(0.45f, 1.0f, 0.6f) : new Color(1.0f, 0.55f, 0.45f);
            GUI.Label(new Rect(bar.x, bar.y - 22 * scale, bar.width, 20 * scale), (_change > 0 ? "+" : "") + _change, _labelStyle);
        }

        GUI.color = previous;
    }
}
