using UnityEngine;
using System.Collections;

public class PlayerCharCont : MonoBehaviour
{
    public bool _active = true;

    public float walkSpeed = 2.0f; 
    public float runSpeed = 4.0f; 

    private Vector3 movement;

    private float gravity = 20.0f; 
    private float speedSmoothing = 10.0f;
    private float rotateSpeed = 500.0f;
    private float runAfterSeconds = 0.1f;

    // カレント移動方向
    private Vector3 moveDirection = Vector3.zero;
    // カレント垂直方向速度
    private float verticalSpeed = 0.0f;
    // カレント水平方向速度
    private float moveSpeed = 0.0f;

    //  controller.Move が返すコリジョンフラグ
    private CollisionFlags collisionFlags;

    // 歩き始める速度
    private float walkTimeStart = 0.0f;

    private CharacterController _controller;
    private Animator _animator;

    // Use this for initialization
    void Start()
    {
        moveDirection = transform.TransformDirection(Vector3.forward);
        _controller =this.GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Transform cameraTransform = Camera.main.transform;
        // camera の x-z 平面から forward ベクターを求める 
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward); 
        forward.y = 0;
        forward = forward.normalized;
        // 右方向ベクターは常にforwardに直交
        Vector3 right = new Vector3(forward.z, 0, -forward.x);    

        float InputZ = _active ? Input.GetAxisRaw("Vertical") : 0;
        float InputX = _active ? Input.GetAxisRaw("Horizontal") : 0;

        // カメラと連動した進行方向を得る
        Vector3 targetDirection = InputX * right + InputZ * forward;

        // 接地？
        if ((collisionFlags & CollisionFlags.CollidedBelow) != 0)  
        {
            // 順方向ではない？
            if (targetDirection != Vector3.zero) 
            {
                // ゆっくり移動か？
                if (moveSpeed < walkSpeed * 0.9) 
                {
                    // 即時ターン
                    moveDirection = targetDirection.normalized; 
                }
                else 
                {
                    // スムースにターン
                    moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
                    moveDirection = moveDirection.normalized;
                }
            }

            // 向きをスムースに変更
            float curSmooth = speedSmoothing * Time.deltaTime;     
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            if (Time.time - runAfterSeconds > walkTimeStart)
                targetSpeed *= runSpeed;
            else
                targetSpeed *= walkSpeed;

            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);

          
            // Animator に移動速度のパラメータを渡す
            _animator.SetFloat("spd", moveSpeed); 
            _animator.SetBool("fall", false);

            if (moveSpeed < walkSpeed * 0.3)
                walkTimeStart = Time.time;
            verticalSpeed = 0.0f;
        }
        else // 浮いている
        {
            // 重力を適応
            verticalSpeed -= gravity * Time.deltaTime;  
            if (verticalSpeed < -4.0)
            {
                _animator.SetBool("fall", true);
            }
        }

        // 移動量を計算
        movement = moveDirection * moveSpeed + new Vector3(0, verticalSpeed, 0);   
        movement *= Time.deltaTime;

     
        collisionFlags = _controller.Move(movement);   // キャラを移動

        // 接地してると移動方向に回転
        if ((collisionFlags & CollisionFlags.CollidedBelow) != 0)       
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}