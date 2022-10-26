using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using DG.Tweening;
using Unity.VisualScripting;

public enum Skill
{
    _spark,   // 剣を振るう
    _spin, //spin
    _abilityHit,  // ヒット
    _heal      //治療
}
public class TacticalMode : MonoBehaviour
{
    
    public class GameEvent : UnityEvent { }
    [HideInInspector]
    public GameEvent _OnAttack;
    [HideInInspector]
    public GameEvent _OnModificationATB;

    public class TacticalModeEvent : UnityEvent<bool> { }
    [HideInInspector]
    public TacticalModeEvent _OnTacticalTrigger;
    [HideInInspector]
    public TacticalModeEvent _OnTargetSelectTrigger;

    private PlayerCharCont _playercharcount;
    private Animator _anime;
    public WeaponCollision _weapon;

    [Header("スローモーション")]
    public float _slowMotionTime = .005f;

    [Space]
    public bool _usingAbility;
    public bool _isAiming;
    public bool _tactialMode;
    public bool _dashing;

    [Space]
    [Header("ATB")]
    public float _atbSlider;
    public float _filledAtbValue = 100f;
    public int _atbCount;

    [Space]
    [Header("半径内のターゲット")]
    public List<Transform> _targets;
    public int _targetIndex;
    public Transform _aimObject;

    [Space]
    [Header("Cameras")]
    public GameObject _gameCam;
    public CinemachineVirtualCamera _targetCam;

    private CinemachineImpulseSource _camImpulseSource;
    void Start()
    {
        _weapon._onHit.AddListener((target) => HitTarget(target));
        _playercharcount = GetComponent<PlayerCharCont>();
        _anime = GetComponent<Animator>();
        _camImpulseSource = Camera.main.GetComponent<CinemachineImpulseSource>();

    }

    void Update()
    {
        if(_targets.Count > 0 && !_tactialMode && !_usingAbility)
        {
            _targetIndex = NearestTargetToCenter();
            _aimObject.LookAt(_targets[_targetIndex]);
        }

        //Attack
        if(Input.GetMouseButtonDown(0) && !_tactialMode && !_usingAbility)
        {
            _OnAttack.Invoke();

            if(!_dashing)
            _anime.SetTrigger("Slash");
        }

        if(Input.GetMouseButtonDown(1) && !_usingAbility)
        {
            if (_atbCount > 0 && !_tactialMode)
                SetTacticalMode(true);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAction();
        }
    }


    public void SpinAttack()
    {
        ModifyATB(-100);

        StartCoroutine(AbilityCooldown());

        SetTacticalMode(false);

        MoveTowardsTarget(_targets[_targetIndex]);

        _anime.SetTrigger("slashability");

        ShakeCamera(Skill._spin, false);
        ///
    }

    public void Heal()
    {
        ModifyATB(-100);

        StartCoroutine(AbilityCooldown());

        SetTacticalMode(false);

        _anime.SetTrigger("heal");

        

    }

    IEnumerator AbilityCooldown()
    {
        _usingAbility = true;
        yield return new WaitForSeconds(1f);
        _usingAbility = false;
    }

    IEnumerator DashCooldown()
    {
        _dashing = true;
        yield return new WaitForSeconds(1f);
        _dashing = false;
    }

    private Vector3 TargetOffset()
    {
        Vector3 position;
        position = _targets[_targetIndex].position;
        return Vector3.MoveTowards(position, transform.position, 1.2f);
    }

    private void CancelAction()
    {
        if (!_targetCam.gameObject.activeSelf && _tactialMode)
            SetTacticalMode(false);
        if (_targetCam.gameObject.activeSelf)
            SetAimCamera(false);
    }

    private void HitTarget(Transform x)
    {
        _OnModificationATB.Invoke();

        ShakeCamera(Skill._spark, true);
        if (_usingAbility)
            ShakeCamera(Skill._abilityHit, true, 4, 4, .3f);

        ModifyATB(25);

        //ここら辺でEnemyの情報を書く
    }


    private void ModifyATB(float amount)
    {
        _OnModificationATB.Invoke();

        _atbSlider += amount;
        _atbSlider = Mathf.Clamp(_atbSlider, 0, (_filledAtbValue * 2));

        if(amount > 0)
        {
            if(_atbSlider >= _filledAtbValue && _atbCount == 0)
                _atbCount = 1;
            if (_atbSlider >= (_filledAtbValue * 2) && _atbCount == 1)
                _atbCount = 2;
        }
        else
        {
            if (_atbCount <= _filledAtbValue)
                _atbCount = 0;
            if (_atbSlider >= _filledAtbValue && _atbCount == 0)
                _atbCount = 1;
        }

        _OnModificationATB.Invoke();

    }

    private void SetTacticalMode(bool on)
    {
        _playercharcount._active = !on;

        _tactialMode = on;

        if(!on)
        {
            SetAimCamera(false);
        }

        _camImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = on ? 0 : 2;

        float time = on ? _slowMotionTime: 1;
        Time.timeScale = time;
        //postprocessing


        _OnTacticalTrigger.Invoke(on);
    }

    private void SetAimCamera(bool on)
    {
        if (_targets.Count < 1)
            return;
        _OnTargetSelectTrigger.Invoke(on);

        _targetCam.LookAt = on ? _aimObject : null;
        _targetCam.Follow = on ? _aimObject : null;
        _targetCam.gameObject.SetActive(on);
        _isAiming = on;

    }


    /// <summary>
    /// カメラをシェイクさせる
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="shakeCamera"></param>
    /// <param name="shakeAmplitude">振幅</param>
    /// <param name="shakeFrequency">周波数</param>
    /// <param name="shkeSustain">継続時間 </param>
    private void ShakeCamera(Skill mode, bool shakeCamera, float shakeAmplitude = 2, float shakeFrequency = 2, float shkeSustain = .2f)
    {
        _camImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude;
        _camImpulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        _camImpulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shkeSustain;

        if (shakeCamera)
            _camImpulseSource.GenerateImpulse();
    }

    int NearestTargetToCenter()
    {
        float[] distance = new float[_targets.Count];

        for(int i = 0; i < _targets.Count; i++)
        {
            distance[i] = Vector2.Distance(Camera.main.WorldToScreenPoint(_targets[i].position), new Vector2(Screen.width / 2, Screen.height / 2));
        }

        float minDistance = Mathf.Min(distance);
        int index = 0;

        for(int i = 0; i < distance.Length; i++)
        {
            if (minDistance == distance[i])
                index = i;
        }
        return index;
    }

    private void MoveTowardsTarget(Transform target)
    {
        if(Vector3.Distance(transform.position, target.position) > 1 && Vector3.Distance(transform.position, target.position) < 10)
        {
            StartCoroutine(DashCooldown());
            transform.DOMove(TargetOffset(), .5f);
            transform.DOLookAt(_targets[_targetIndex].position, .2f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            _targets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            if (_targets.Contains(other.transform))
                _targets.Remove(other.transform);
        }
    }
}
