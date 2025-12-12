using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ultrabolt.SkyEngine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public enum State
    {
        ready,
        idle,
        walk,
        run,
        talk,
        useTool,
        sleep,
        KO
    }

    public State state;

    CharacterController cc;
    public float basicMoveSpeed; //기본 이동 속도
    public float RunSpeed;

    // 앉기 애니메이션 관련 변수/ 외부 회전 복원 중임을 표시 (벤치 등에서 사용)
    [HideInInspector] public bool isRestoringRotation = false;

    //BenchInteractSimple.cs에서 접근하기 위해 접근제한자 변경 (앉기 애니메이션 관련)
    //private float moveSpeed; //현재 이동 속도 
    public float moveSpeed;

    //회전 스피드(좌우) 상하 회전은 카메라에 적용 예정
    public float rotSpeed;

    //점프력
    public float jumpPower;
    private float yVelocity;
    private bool jumpAble = true;
    float gravity = -15f;

    public float Rot = 0; //회전값

    //플레이어 체력
    private HpController hp;

    //플레이어 기력
    public float maxStamina;
    public float stamina;

    //스태미너 소모되는 량;
    private float useStamina;

    //스태미너 바
    public Slider StaminaBar;

    //스태미너 피로 상태
    private bool isTired;

    public Transform SummonPos;//소환위치

    public float mouseSens;

    public Slider MouseSensBar;

    public Animator anim;

    public Transform playerSkin;

    public Transform SpawnPoint;

    private float hungryTime = 0;

    // Start 사운드 관련
    [Header("Sound")]
    private float stepTimer = 0f;         // 발소리 간격 타이머 (시간 체크용)
    [SerializeField] private float stepInterval = 0.5f;  // 발소리 기본 간격 (초)
    [SerializeField] private float runStepMultiplier = 0.6f; // 달릴 때 간격 조정 (짧게)
    private bool jumpPlayed = false;      // 점프음이 중복 재생되지 않게 방지용
    private bool isInWater = false;
    private Vector3 _lastPos;
    private AudioSource currentFootstepSource; // 현재 재생 중인 발소리 AudioSource 추적
    // End 사운드 관련

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        hp = GetComponent<HpController>();

        moveSpeed = basicMoveSpeed;

        //if(PlayerPrefs.HasKey("PlayerStamina"))
        //    stamina = PlayerPrefs.GetFloat("PlayerStamina");
        //else
        stamina = maxStamina;

        StaminaBar.maxValue = maxStamina;

        StaminaBar.value = stamina;

        state = State.ready;

        useStamina = 0.01f;

        if (PlayerPrefs.HasKey("MouseSens"))
            MouseSensBar.value = PlayerPrefs.GetFloat("MouseSens");

        mouseSens = MouseSensBar.value;

        //if (PlayerPrefs.HasKey("PlayerPosX")) //플레이어의 위치가 저장되어 있다면
        //{
        //    Debug.Log("실행");
        //    Vector3 vector = transform.position;
        //    vector.x = PlayerPrefs.GetFloat("PlayerPosX");
        //    vector.y = PlayerPrefs.GetFloat("PlayerPosY");
        //    vector.z = PlayerPrefs.GetFloat("PlayerPosZ");

        //    transform.position = vector;//저장된 위치로 이동
        //}

        StartCoroutine(AutoSave());

        //사운드 관련
        _lastPos = transform.position;

        Rot = transform.localEulerAngles.y;
        StartCoroutine(ReadyStart());
    }

    private void Update()
    {
        if (state == State.ready)
            return;
        StaminaBar.value = stamina;
        if (state == State.sleep || state == State.KO || state == State.talk || state == State.useTool) //플레이어가 잠을 자고 있거나 기절한 상태인 경우
        {
            return;
        }

        // 1. 사용자의 입력을 받음
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        //if ( h == 0 && v == 0)
        if (inputDir.magnitude == 0)
        {
            state = State.idle;
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
        else if (state == State.idle)
        {
            state = State.walk;
            anim.SetBool("Walk", true);
        }

        // 2. 이동 방향을 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        // 플레이어의 회전값을 기준으로 이동 방향 전환
        dir = transform.TransformDirection(dir);

        // 2-2. 만일, 점프 중이었고, 다시 바닥에 착지했다면
        if (cc.isGrounded && jumpAble == false)
        {
            yVelocity = -1;
            jumpAble = true;
            anim.SetTrigger("Land");
        }
        else
            yVelocity += gravity * Time.deltaTime; //중력 적용, 떨어지는 속도

        // 앉기 상태일 때는 회전 로직 무시
        if (anim.GetBool("Sit"))
            return;

        //점프
        if (Input.GetKey(KeyCode.Space) && jumpAble)
        {
            yVelocity = jumpPower;
            jumpAble = false;
            anim.SetTrigger("Jump");

            // 점프 시 사운드 추가
            PlayJumpSound();
        }

        //달리기
        if (Input.GetKey(KeyCode.LeftShift) && state != State.run && stamina > 0 && (h != 0 || v != 0))
        {
            moveSpeed = RunSpeed;
            state = State.run;
            anim.SetBool("Run", true);
        }
        else if ((Input.GetKeyUp(KeyCode.LeftShift) || stamina <= 0) && !isTired) //달리기 멈춤
        {
            moveSpeed = basicMoveSpeed;
            state = State.walk;
            anim.SetBool("Run", false);
        }

        if (state == State.run)
        {
            stamina -= 0.2f * Time.deltaTime;
        }


        dir.y = yVelocity;

        //위에서 입력받은 값으로 움직이는 것을 시행하는 코드
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // 플레이어 스킨 회전
        if (inputDir.magnitude > 0.1f)
        {
            Vector3 moveDir = transform.TransformDirection(inputDir);
            moveDir.y = 0;
            playerSkin.rotation = Quaternion.Slerp(playerSkin.rotation, Quaternion.LookRotation(moveDir), 10f * Time.deltaTime);
        }

        if (!Input.GetKey(KeyCode.LeftAlt) && Cursor.lockState == CursorLockMode.Locked)
        {
            float rotX = Input.GetAxis("Mouse X") * mouseSens;

            Rot += rotX * rotSpeed * Time.deltaTime;
        }

        // 2. 회전 방향으로 물체를 회전시킴
        transform.eulerAngles = new Vector3(0, Rot, 0);

        if (stamina < 0 && !isTired)
        {
            moveSpeed /= 2;
            useStamina = 0.1f;
            isTired = true;
        }
        else if (isTired && stamina > 0)
        {
            moveSpeed = basicMoveSpeed;
            useStamina = 0.01f;
            isTired = false;
        }

        stamina -= useStamina * Time.deltaTime; //실시간 스태미나 소모
        //Debug.Log(stamina);

        if(stamina <= 30)
        {
            hungryTime -= Time.deltaTime;
            if(hungryTime <= 0)
            {
                //음향
                SFXSoundManager.Instance.Play("hungry", 0.3f, 0f, 0.1f);
                hungryTime = 10;
            }
        }

        if (stamina <= -15f) //스태미너가 -15가 되었을 경우
        {
            state = State.KO;
            hp.Heal(hp.MaxHp);
        }
        
        // 발소리 사운드 처리
        HandleFootstepAndJump();

        if (stamina < 0) //스태미너가 0이라면 사용 불가능할 예정인 행동들
            return;

    }

    // 앉기 관련 / 외부(벤치 등)에서 회전값을 강제로 동기화하기 위한 함수
    public void ForceSyncRotation()
    {
        // 현재 Transform의 Y회전값으로 내부 Rot 변수 갱신
        Rot = transform.eulerAngles.y;
        Debug.Log($"[Sync] TransformY={transform.eulerAngles.y}, Rot={Rot}");
    }

    // 회전 보정 (소수점 누적 방지)
    public void SnapRotation()
    {
        Rot = Mathf.Round(Rot * 1000f) / 1000f; // 소수점 3자리까지만 유지
    }

    // 앉기 관련 / BenchInteractSimple 이후에 재동기화시키기
    public void ForceApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, Rot, 0);
    }

    //  [추가] 발소리 및 점프 사운드 처리
    private void HandleFootstepAndJump()
    {
        if (SFXSoundManager.Instance == null || cc == null)
            return;

        bool isGrounded = cc.isGrounded;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool movePressed = Mathf.Abs(h) > 0.2f || Mathf.Abs(v) > 0.2f;
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        // 실제 이동 판정 (수평 속도만)
        Vector3 planarVel = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        float planarSpeed = planarVel.magnitude;
        float movedSqr = (transform.position - _lastPos).sqrMagnitude;

        bool movedEnough = movedSqr > 0.0005f;
        bool isActuallyMoving = isGrounded && movePressed && planarSpeed > 0.15f && movedEnough;

        if (isGrounded)
            jumpPlayed = false;

        // 멈췄거나 공중이면 즉시 발소리 중단
        if (!isActuallyMoving)
        {
            if (currentFootstepSource != null && currentFootstepSource.isPlaying)
            {
                currentFootstepSource.Stop();
                currentFootstepSource = null;
            }

            stepTimer = 0f;
            _lastPos = transform.position;
            return;
        }

        // 이동 중일 때만 일정 간격으로 재생
        stepTimer -= Time.deltaTime;

        // --- [NEW] 상태 전환 시 기존 발소리 즉시 정지 ---
        string newSoundKey = "footstep_walk";
        if (isInWater)
            newSoundKey = "footstep_water";
        else if (runPressed)
            newSoundKey = "footstep_run";

        if (currentFootstepSource != null && currentFootstepSource.isPlaying)
        {
            // 걷기↔달리기 / 달리기↔걷기 / 물 위↔육상 전환 시 즉시 교체
            if (currentFootstepSource.clip != SFXSoundManager.Instance.GetClip(newSoundKey))
            {
                currentFootstepSource.Stop();
                currentFootstepSource = null;
                stepTimer = 0f; // 간격 타이머 초기화 → 다음 루프에서 즉시 새 소리 재생
            }
        }

        if (stepTimer <= 0f)
        {
            string soundKey = newSoundKey;

            // 이미 소리 재생 중이면 새로 재생하지 않음 (중복 방지)
            if (currentFootstepSource == null || !currentFootstepSource.isPlaying)
            {
                currentFootstepSource = SFXSoundManager.Instance.PlayAtPoint(soundKey, transform.position);
            }

            // 다음 간격 설정
            stepTimer = stepInterval * (runPressed ? runStepMultiplier : 1f);
        }

        _lastPos = transform.position;
    }

    IEnumerator ReadyStart()
    {
        yield return new WaitForSeconds(5f);
        state = State.idle;
    }
    private void PlayJumpSound()
    {
        if (!jumpPlayed && SFXSoundManager.Instance != null)
        {
            jumpPlayed = true;
            Debug.Log("[SFX] Jump Sound");
            SFXSoundManager.Instance.PlayAtPoint("jump", transform.position);
        }
    }

    //  [추가] 물 감지
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isInWater = true;
            Debug.Log("물속 감지됨!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isInWater = false;
            Debug.Log("물 밖으로 나옴!");
        }
    }
    // 사운드 메서드

    public void Sleeping()
    {
        state = State.sleep;
        hp.Heal(hp.MaxHp);
    }

    public void EatFood(float food)
    {
        stamina += food;
        if (stamina > maxStamina)
            stamina = maxStamina;
        StaminaBar.value = stamina;
    }

    public void WakeUp()
    {
        if (state == State.KO)
        {
            Debug.Log("절반");
            stamina = maxStamina / 2;
        }
        else
        {
            stamina = maxStamina;
        }
        transform.position = SpawnPoint.position;
        playerSkin.localEulerAngles = new Vector3(0, 180, 0);
        state = State.idle;
    }

    public void HandUp()
    {
        anim.SetBool("OverHand", true);
        anim.SetTrigger("HandUp");
    }

    public void HandDown()
    {
        anim.SetBool("OverHand", false);
        anim.SetTrigger("UnderHand");
    }

    public void ThrowObject()
    {
        anim.SetBool("OverHand", false);
        anim.SetTrigger("Throw");
    }

    public void SaveTransform()
    {
        PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);

        //플레이어 스태미나 저장
        //PlayerPrefs.SetFloat("PlayerStamina", stamina);
    }

    public void MouseSensor()
    {
        mouseSens = MouseSensBar.value;
        PlayerPrefs.SetFloat("MouseSens", mouseSens);
    }

    IEnumerator AutoSave() //플레이어 위치 자동저장
    {
        while (true)
        {
            yield return new WaitForSeconds(120);
            SaveTransform();
        }
    }

}
