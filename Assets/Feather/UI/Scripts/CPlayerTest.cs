using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTest : MonoBehaviour
{
    [SerializeField] private float rayDistance = 0.2f;
    [SerializeField] public bool _isGrounded;

    [SerializeField] private float _moveSpeed = 1f;
    float _x;
    float _y;
    float _z;

    //[SerializeField]private GroundTest groundTest;

    [SerializeField] private int _jumpPower = 10;

    [SerializeField] private Rigidbody _rb;



    // Start is called before the first frame update
    void Start()
    {
        _x = transform.position.x;
        _y = transform.position.y;
        _z = transform.position.z;

        _x += 1;
        _y += 1;

        //position = new Vector3(_x, _y, _z); // == 게임 오브젝트의 위치를 x= _x y= _y z= _z
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = transform.position + new Vector3(0, 0, 1f) * _moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-1f, 0, 0) * _moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0, 0, -1f) * _moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(1f, 0, 0) * _moveSpeed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded == true)
        {
            //transform.position += new Vector3(0f, 2f, 0) * moveSpeed * Time.deltaTime;
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //
        //}
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //
        //}
        // transform.position = new Vector3(_x, _y, _z);

    }

    private void OnCollisionEnter(Collision collision)
    {
        _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }

    // 이동, 점프 (wasd space)
    /*
     * 이동 -> transform.forward * 이동속도(필드) * Time.deltaTime
     * 점프 -> transform.up * 점프높이(필드)
     * 
     * 1. Start 시 transform 변화
     * 2. Update transform 변화를 Vector3||transform로 계속 이동시키기
     * 3. 키입력 (wasd 다 넣을 필요없음 1가지) || 지속 입력
     * 4. normalized 넣어보기
     * 5. 점프
     * 6. 나머지 구현
     */
}