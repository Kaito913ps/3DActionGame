using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    private float _InputX;
    private float _InputZ;

    private bool _active = true;

    [SerializeField]
    private float _walkSpeed = 2.0f;
    [SerializeField]
    private float _runSpeed = 4.0f;

    private Vector3 _movement;

    private float _gravity = 20.0f;
    private float _sppeSmoothing = 10.0f;
    private float _rotateSpeed = 500.0f;
    private float _runAfeterSecounds = 0.1f;

    [Header("移動方向")]
    private Vector3 _moveDirection = Vector3.zero;
    private float _verticalSpeed = 0.0f;
    private float _moveSpeed = 0.0f;

    
    private CollisionFlags _collisionFlags;

    //歩き始める速度
    private float _walkTimeStart = 0.0f;

    private Animator _animator;
    private CharacterController _controller;


    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _moveDirection = transform.TransformDirection(Vector3.forward);
    }

    // Update is called once per frame
    void Update()
    {
        Transform cameraTransform = Camera.main.transform;
        // camera の x-z 平面から forward ベクターを求める 
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        //右方向ベクターは常にforwardに直交
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        _InputX = _active ? Input.GetAxis("Horizontal") : 0;
        _InputZ = _active ? Input.GetAxis("Vertical") : 0;

        //カメラと連動した進行方向を得る
        Vector3 targetDirection = _InputX * right + _InputZ * forward;
        
        if((_collisionFlags & CollisionFlags.CollidedBelow) != 0)
        {
            //順方向ではない？
            if (targetDirection != Vector3.zero)
            {
                //ゆっくり移動か？
                if (_moveSpeed < _walkSpeed * 0.9f)
                {
                    // 即時ターン
                    _moveDirection = targetDirection.normalized;
                }
                else
                {
                    //スムーズにターン
                    _moveDirection = Vector3.RotateTowards(_moveDirection, targetDirection, _runSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
                    _moveDirection = _moveDirection.normalized;
                }
            }

            //向きをスムーズに変更
            float curSmooth = _sppeSmoothing * Time.deltaTime;
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            if (Time.time - _runAfeterSecounds > _walkTimeStart)
                targetSpeed *= _runSpeed;
            else
                targetSpeed *= _walkSpeed;

            _moveSpeed = Mathf.Lerp(_moveSpeed, targetSpeed, curSmooth);

            _animator.SetFloat("spd", _moveSpeed);
            _animator.SetBool("fall", false);

            if(_moveSpeed < _walkSpeed * 0.3f)
                _walkTimeStart = Time.time;
            _verticalSpeed = 0.0f;
        }
        else
        {
            //浮いている
            _verticalSpeed -= _gravity * Time.deltaTime;//重力を適応
            if(_verticalSpeed < -4.0f)
            {
                _animator.SetBool("fall", true);
            }
        }

        _movement = _moveDirection * _moveSpeed + new Vector3(0, _verticalSpeed, 0);   // 移動量を計算
        _movement *= Time.deltaTime;

        CharacterController controller = GetComponent<CharacterController>();   // キャラクターコントローラを取得
        _collisionFlags = controller.Move(_movement);   // キャラを移動

        if ((_collisionFlags & CollisionFlags.CollidedBelow) != 0)       // 接地してると移動方向に回転
        {
            transform.rotation = Quaternion.LookRotation(_moveDirection);
        }
    }
}
