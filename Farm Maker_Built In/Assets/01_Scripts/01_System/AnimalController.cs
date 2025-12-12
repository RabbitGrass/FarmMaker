using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AnimalController : MonoBehaviour, IHandsUp
{
    public enum AnimalState
    {
        idle,
        move,
        run,
        food,
        eat,
        friend,
        caught,
        rest,
        hurt,
        dead
    }

    //동물 종류(닭, 돼지, 고양이 등)
    public string AnimalType;
    // 사운드 관련
    public AnimalSFXSoundSet animalSoundSource;
    private Coroutine cryRoutine;
    //

    public AnimalState animalState;
    //private AnimalState prevState;

    //동물 들 수 있는지 여부
    public bool CatchAble;

    Rigidbody rb;

    public float WalkSpeed;
    public float RunSpeed;

    private float moveSpeed;
    //동물이 행동하는 시간(idle, move)
    float activeTime;

    //Run, Eat등 활동시간을 처음에 정했는지 여부를 비교하기 위한 부울
    bool isActiveTimeSet;

    float runTurnTime; //뛰는동안 우왕자왕 하게 방향을 바꿀 시간

    public float detectRange = 10f; // Food 감지 거리
    private Transform targetFood;   // 현재 목표로 삼은 Food

    public float activeDistance = 30f;

    public bool isActive = true;

    public Animator anim;

    public bool isTalk;

    [Header("Caught vector setting")]
    public Vector3 caughtPos;
    public Vector3 caughtRot;

    public Coroutine check;
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        check = StartCoroutine(CheckDistance());

        // 사운드 관련
        if (cryRoutine == null)
            cryRoutine = StartCoroutine(CryRandomly());
        // 사운드 관련
    }

    // start 사운드 관련
    private void OnDisable()
    {
        if (cryRoutine != null)
            StopCoroutine(cryRoutine);
        cryRoutine = null;
    }

    IEnumerator CryRandomly()
    {
        // 무한 반복
        while (true)
        {
            // 랜덤 대기 시간 (예: 5~20초 사이)
            float waitTime = Random.Range(5f, 20f);
            yield return new WaitForSeconds(waitTime);

            // 울음 조건 (비활성 중엔 울지 않게)
            //if (isActive && animalState == AnimalState.idle && animalSoundSource != null)
            if (isActive)
            {
                var clip = animalSoundSource.crying;
                if (clip != null)
                {
                    // 3D 위치 기반으로 재생 (중앙 사운드 매니저를 통함)
                    SFXSoundManager.Instance.PlayAtPoint(clip, transform.position, 0.05f);
                    
                }
            }
        }
    }
    // end 사운드 관련

    private void Start()
    {
        animalState = AnimalState.idle;
        moveSpeed = WalkSpeed;
        //prevState = animalState;
    }

    private void Update()
    {
        //if (animalState != prevState)
        //{
        //    Debug.Log($"[{name}] 상태 변경: {prevState} → {animalState}");
        //    prevState = animalState;
        //}
        //ActiveAI();
        if (!isActive)
            return;

        ActiveAI();
    }

    private void FixedUpdate()
    {
        if (animalState == AnimalState.move || animalState == AnimalState.run)
            rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
        else if (animalState == AnimalState.food)
            Food();
    }

    IEnumerator CheckDistance()
    {
        while (true)
        {
            if(animalState != AnimalState.friend)
                CheckPlayerDistance();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CheckPlayerDistance()//플레이어 감지
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = float.MaxValue;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
                closestDist = dist;
        }

        if (closestDist > activeDistance)
        {
            if (isActive)
                SetActiveAI(false);
        }
        else
        {
            if (!isActive)
                SetActiveAI(true);
        }
    }

    private void SetActiveAI(bool active)
    {
        if (isActive == active) //이미 isActive가 같다면 리턴
            return;

        isActive = active; //isActive를 전달받은 bool값으로 변경

        if (isActive)
        {
            //anim.enabled = true;
            rb.isKinematic = false;
        }
        else
        {
            //anim.enabled = false;
            rb.isKinematic = true;
            animalState = AnimalState.idle;
        }
    }

    private void ActiveAI()
    {
        if (animalState == AnimalState.caught)
        {
            Caught();
            return;
        }
        // Debug.Log("움직이기 시작");
        if (activeTime <= 0)
            activeTime = Random.Range(1, 4);

        if (activeTime > 0)
            activeTime -= Time.deltaTime;

        if (targetFood == null || !isTalk)
        {
            //Debug.Log("음식 찾는 중");
            FindFood();
        }

        if (animalState == AnimalState.rest) //rest상태면 아래 코드 막기
            return;

        switch (animalState)
        {
            case AnimalState.idle:
                Idle();
                break;
            case AnimalState.move:
                Move();
                break;
            case AnimalState.run:
                Run();
                break;
            case AnimalState.eat:
                Eat();
                break;
            case AnimalState.caught:
                Caught();
                break;
            case AnimalState.dead:
                Dead();
                break;
            default:
                break;
        }
    }

    void Idle()
    {
        if (anim.GetBool("Walk") || anim.GetBool("Run"))
        {
            // Debug.Log("아이들 실행");
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
        
        if(transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0)
        {
            Vector3 vector = transform.eulerAngles;
            vector.x = 0;
            vector.z = 0;
            transform.eulerAngles = vector;
        }
        //int r = Random.Range(0, 1000);

        if (isTalk)
            return;

        if(activeTime <= 0)
        {
            float rot = Random.Range(-180, 180);
            transform.eulerAngles = new Vector3(0, rot, 0);

            animalState = AnimalState.move;
        }
    }

    void Move()
    {
        //int r = Random.Range(0, 1000);
        if(!anim.GetBool("Walk"))
            anim.SetBool("Walk", true);

        if(activeTime <= 0)
        {
            animalState = AnimalState.idle;
        }
    }

    void FindFood()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRange);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Food"))
            {
                targetFood = hit.transform;
                animalState = AnimalState.food;
                //Debug.Log("음식 찾음" +  targetFood.name);
                break;
            }
        }
    }

    void Food()
    {
        if (targetFood == null)
        {
            Debug.Log("타깃 사라짐");
            animalState = AnimalState.idle;
            return;
        }

        // Food 쪽 방향 바라보기
        Vector3 dir = (targetFood.position - transform.position).normalized;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);
        float distance;

        if (targetFood.parent != null && !targetFood.root.CompareTag("Player")) //타깃인 음식이 플레이어 손에 들어있는지 여부 확인
            distance = 2f;
        else
            distance = 1.0f;

        var distanceToFood = Vector3.Distance(transform.position, targetFood.position); //타깃음식과 본인의 거리 탐지

        if (distanceToFood > detectRange) // 너무 멀어짐
        {
            Debug.LogWarning($"[{name}] 음식 {targetFood.name}이 너무 멀어서 무효화됨! 거리={distanceToFood}, 범위={detectRange}");
            targetFood = null;
            animalState = AnimalState.idle;
        }
        else if (distanceToFood > distance)
        {
            // 이동
            rb.MovePosition(transform.position + transform.forward * WalkSpeed * Time.deltaTime);
            if (!anim.GetBool("Walk"))
                anim.SetBool("Walk", true);
        }
        else //가까이 다가갔을 때
        {
            if (anim.GetBool("Walk"))
            {
                anim.SetBool("Walk", false);
            }

            if(targetFood.parent == null || (targetFood.parent != null && !targetFood.root.CompareTag("Player")))
            {
                FoodManager food = targetFood.GetComponent<FoodManager>();
                food.EatAnimal(gameObject, null);
                targetFood = null;
            }
        }
        

        //// 거리가 충분히 가까우면 먹기 상태 전환
        //else if (targetFood.parent == null)
        //{
        //    animalState = AnimalState.eat;
        //    targetFood = null;
        //}
    }

    void Eat()
    {
        //높은 확률로 애니메이션
    }
    public void Run()
    {
        if (!anim.GetBool("Run"))
        {
            anim.SetTrigger("RunStart");
            anim.SetBool("Run", true);

            //음향
            var clip = animalSoundSource.scream;
            if (clip != null)
            {
                // 3D 위치 기반으로 재생 (중앙 사운드 매니저를 통함)
                SFXSoundManager.Instance.PlayAtPoint(clip, transform.position, 0.05f);

            }
        }

        moveSpeed = RunSpeed;
        if (!isActiveTimeSet)
        {
            activeTime = Random.Range(5, 10);
            isActiveTimeSet = true;
        }

        // 일정 간격마다 랜덤 방향 전환
        runTurnTime -= Time.deltaTime;
        if (runTurnTime <= 0)
        {
            float randomY = Random.Range(-90f, 90f); // 좌우로 급하게 꺾임
            transform.Rotate(0, randomY, 0);
            runTurnTime = Random.Range(0.3f, 1.0f); // 0.3~1초마다 방향 변경
        }

        if (activeTime <= 0)
        {
            moveSpeed = WalkSpeed;
            isActiveTimeSet = false;
            animalState = AnimalState.idle;
        }
    }

    public void Talk(bool talk)
    {
        isTalk = talk;
        if(animalState != AnimalState.rest)
            animalState = AnimalState.idle;
    }

    public void Caught()
    {
        if(animalState != AnimalState.caught) //최초 실행시
            animalState = AnimalState.caught;

        if (!anim.GetBool("Walk"))
            anim.SetBool("Walk", true);

        if (anim.GetBool("Run"))
        {
            anim.SetBool("Run", false);
        }
    }

    public void Hurt()
    {
        animalState = AnimalState.hurt;
        //대충 뛴 후 몸이 붉은 애니메이션 뜰 예정
        //아마 높은 확률로 코루틴 돌 가능성이 높음
    }

    void Dead()
    {

    }

    //HandUp인터페이스
    public void InHandTransform()
    {
        transform.localPosition = caughtPos;
        transform.localEulerAngles = caughtRot;
        rb.isKinematic = true;

        Caught();
    }

    public void HandDownTransform()
    {
        transform.parent = null;
        rb.isKinematic = false;

        Vector3 vector = transform.eulerAngles;
        vector.x = 0;
        vector.z = 0;
        transform.eulerAngles = vector;

        animalState = AnimalState.idle;
    }

    public void Throw(GameObject player, float power)
    {
        rb.isKinematic = false;
        rb.AddForce(player.transform.forward * power);
       
        animalState = AnimalState.run;

        Vector3 vector = transform.eulerAngles;
        vector.x = 0;
        vector.z = 0;
        transform.eulerAngles = vector;

        transform.parent = null;
    }
}
