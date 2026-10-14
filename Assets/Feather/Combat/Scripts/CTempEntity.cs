using System.Collections;
using UnityEngine;

public class CTempEntity : MonoBehaviour
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    #region Inspector
    [SerializeField] private EntityType _type;
    [SerializeField] private int _speed;
    [SerializeField] private int _randSpeed;
    // 일단 임시로
    [SerializeField] private bool _isDead = false;
    #endregion

    #region Field
    private MeshRenderer _mesh;
    private Color _initColor;
    private Color _attackColor = Color.red;
    #endregion

    #region Property
    public int Speed => _speed + _randSpeed;
    #endregion

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
        _initColor = _mesh.material.color;
    }

    public bool Attack()
    {
        if (_type == EntityType.Player)
        {
            if (!Input.GetKeyDown(KeyCode.A))
            {
                return false;
            }
        }

        _mesh.material.color = _attackColor;
        StartCoroutine(CoAttackDelay());
        return true;
    }

    private IEnumerator CoAttackDelay()
    {
        yield return new WaitForSeconds(3.0f);
        _mesh.material.color = _initColor;
    }

    public void RoundRandomSpeed()
    {
        _randSpeed = Random.Range(0, 7);
    }
}
