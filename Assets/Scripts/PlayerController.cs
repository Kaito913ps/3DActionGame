using UnityEngine;

/// <summary>
/// キャラクターを操作してジャンプさせる機能を提供するコンポーネント
/// WASD で移動し、Space でジャンプする（Input Manager 使用）
/// 足下にトリガーを設置すること。足下のトリガーが接触していたら「接地している」と判定し、ジャンプできる。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 1f;
    [SerializeField] float _jumpPower = 5f;
    Rigidbody _rb = default;
    Animator _anim = default;
    /// <summary>接地フラグ</summary>
    bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 入力を受け付ける
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 入力された方向を「カメラを基準とした XZ 平面上のベクトル」に変換する
        Vector3 dir = new Vector3(h, 0, v);
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0;

        // キャラクターを「入力された方向」に向ける
        if (dir != Vector3.zero)
        {
            this.transform.forward = dir;
        }

        // Y 軸方向の速度を保ちながら、速度ベクトルを求めてセットする
        Vector3 velocity = dir.normalized * _moveSpeed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;

        // ジャンプ処理
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }
    }

    void LateUpdate()
    {
        // アニメーションの処理
        if (_anim)
        {
            _anim.SetBool("IsGrounded", _isGrounded);
            Vector3 walkSpeed = _rb.velocity;
            walkSpeed.y = 0;
            _anim.SetFloat("Speed", walkSpeed.magnitude);

            if (Input.GetMouseButtonDown(0))
            {
                _anim.SetTrigger("Slash");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        _isGrounded = true;
    }

    void OnTriggerExit(Collider other)
    {
        _isGrounded = false;
    }
}
