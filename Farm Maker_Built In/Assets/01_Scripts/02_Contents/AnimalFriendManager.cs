using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class AnimalFriendManager : MonoBehaviour
{
    private GameObject Friend;
    public bool isFirend;
    public float maxFriendShip;
    public float friendShip;

    public Transform TalkCam;

    public GameObject cam;

    public GameObject animalFriendBox;
    public Sprite animalBasicBox;

    //여기서는 활성화 여부만 따질 것
    public GameObject FriendBox;
    public Image animalPhoto; //동물 사진
    public TMP_Text animalName; //동물 이름
    public Button[] summonBtn;    //소환 버튼
    public string petName;
    public Slider FriendlyBar;
    public Slider HungerBar;

    private AnimalHunger hunger;

    private Vector3 LastPosition;

    public float detectRange;

    private float speed;

    private bool isFar;

    private Rigidbody rb;

    AnimalController animal;

    private bool isSummon = false; //소환 여부

    private float summonCoolTime = 5f;

    public bool canPetted; //쓰다듬기 가능 여부

    // 친밀도가 증가했을시 하트 이펙트
    public GameObject HeartEffect;

    private AudioSource talkAudioSource;  // 대화 사운드용 AudioSource

    private Canvas AnimalInfoCanvas;

    public GameObject[] AnimalPos;

    private Transform RestPoint;

    public TMP_Text PetNameInfoText;

    private bool isSit;
    private void Start()
    {
        animal = GetComponent<AnimalController>();
        rb = GetComponent<Rigidbody>();

        cam = GameObject.Find("Main Camera");
        
        HungerBar.maxValue = hunger.MaxHunger;
        HungerBar.value = hunger.hunger;

        // 오디오소스 동적으로 생성 (루프용)
        talkAudioSource = gameObject.AddComponent<AudioSource>();
        talkAudioSource.spatialBlend = 1f; // 3D 사운드
        talkAudioSource.loop = true;
        talkAudioSource.playOnAwake = false;
        talkAudioSource.volume = 0.3f;

        AnimalInfoCanvas = GetComponentInChildren<Canvas>();
    }

    private void OnEnable()
    {
        FriendlyBar.maxValue = maxFriendShip;
        FriendlyBar.value = friendShip;
        hunger = GetComponent<AnimalHunger>();
        if (isFirend)
        {
            YourFriend();
        }
        else
        {
            FriendBox.SetActive(false);
            //animalName.color = new Color(255, 255, 255, 100);
            //foreach (Button btn in summonBtn)
                //btn.interactable = false;
            //summonBtn[1].gameObject.SetActive(false);
        }

        if (PetNameInfoText != null)
            PetNameInfoText.text = petName;
    }

    private void Update()
    {
        if (!AnimalPos[0].activeSelf && !isSit)
        {
            AnimalPos[0].SetActive(true);
            for(int i = 1; i < AnimalPos.Length; i++)
            {
                if(AnimalPos[i].activeSelf)
                    AnimalPos[i].SetActive(false);
            }
        }
        HungerBar.value = hunger.hunger;
        if (!isSummon)
            return;
        if (!isFar)
            FindFriend();
        if (RestPoint != null && !isFar && !isSit && animal.animalState == AnimalController.AnimalState.idle)
        {
            AnimerRest();
        }
        if(animal.animalState != AnimalController.AnimalState.rest && isSit)
        {
            EndAnimalRest();
        }
    }

    private void FixedUpdate()
    {
        if (!isSummon) return;
        if (isFar)
        {
            if ((isSit))
            {
                isSit = false;
            }
            GoToFriend();
        }
    }

    private void FindFriend()
    {
        // // Friend 쪽 방향 바라보기
        //Vector3 dir = (Friend.transform.position - transform.position).normalized;
        //dir.y = 0;
        //Quaternion lookRot = Quaternion.LookRotation(dir);
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);

        var distanceToFriend = Vector3.Distance(transform.position, Friend.transform.position); //친구와 본인의 거리 탐지

        if (distanceToFriend > detectRange) //멀어짐
        {
            animal.animalState = AnimalController.AnimalState.friend;
            animal.isActive = false;
            isFar = true;
        }
    }

    private void GoToFriend()
    {

        // Friend 쪽 방향 바라보기
        Vector3 dir = (Friend.transform.position - transform.position).normalized;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);

        float distance = 2f;

        var distanceToFriend = Vector3.Distance(transform.position, Friend.transform.position); //친구와 본인의 거리 탐지

        if (distanceToFriend > detectRange)
        {
            speed = animal.RunSpeed;
            if (!animal.anim.GetBool("walk"))
                animal.anim.SetBool("Walk", true);

            if (!animal.anim.GetBool("Run"))
            {
                animal.anim.SetTrigger("RunStart");
                animal.anim.SetBool("Run", true);
            }
        }
        else if (distanceToFriend > detectRange / 2)
        {
            // 이동
            speed = animal.WalkSpeed;
            if (animal.anim.GetBool("Run"))
                animal.anim.SetBool("Run", false);
            if (!animal.anim.GetBool("Walk"))
                animal.anim.SetBool("Walk", true);
        }
        else if (distance > distanceToFriend)
        {
            if(animal.animalState != AnimalController.AnimalState.caught)
                animal.animalState = AnimalController.AnimalState.idle;
            animal.isActive = true;
            isFar = false;
            return;
        }

        //이동
        rb.MovePosition(transform.position + transform.forward * speed * Time.deltaTime);

    }

    public void FriendShip(float intimacy, GameObject player)
    {
        //여기서 하트 이펙트가 튀어나올 예정
        friendShip += intimacy;
        FriendlyBar.value = friendShip;

        GameObject heart = Instantiate(HeartEffect, transform);
        Destroy(heart, 5f);

        //음향  Play(string name, float volume, float delay = 0f, float pitchVariation = 0.1f)
        SFXSoundManager.Instance.Play("friendLevel", 0.5f, 0f, 0.1f);

        if (!isFirend && friendShip >= maxFriendShip/2)
        {
            Friend = player;
            isFirend = true;
            YourFriend();

            //음향
            SFXSoundManager.Instance.Play("friendShip", 0.5f, 0f, 0.1f);

        }

        FriendlyBar.value = friendShip;
    }

    //동물과 대화할 때
    public void TalkAnimal(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        PostProcessVolume camVolume = cam.GetComponent<PostProcessVolume>();
        PlayerInterector interector = player.GetComponent<PlayerInterector>();
        camVolume.profile.TryGetSettings(out DepthOfField camField); //DepthOfField 효과 가져오기
        //DepthOfField camField = cam.GetComponent<DepthOfField>();
        if (cam.transform.parent != TalkCam) //동물과 대화중이 아닌 상태인데 동물과 대화 중이고 싶을 때
        {
            interector.Guides.SetActive(false); //가이드텍스트 끄기
            AnimalInfoCanvas.gameObject.SetActive(false); //캐너스 끄기
            if(interector.heldObject != null)
                interector.heldObject.SetActive(false); //손에 들고있는 물건이 있다면 해당 오브젝트도 끄기
            
            cam.transform.parent = TalkCam;
            camField.active = false;
            // 플레이어 방향으로 회전
            Vector3 lookPos = player.transform.position;
            lookPos.y = transform.position.y; // y축 회전만
            transform.LookAt(lookPos);

            AnimalController animal = gameObject.GetComponent<AnimalController>();
            animal.Talk(true);

            OpenTalkUi();
            playerController.state = PlayerController.State.talk;

            //사운드 추가 재생 (AnimalController 방식)
            if (animal != null && animal.animalSoundSource != null)
            {
                AudioClip talkClip = animal.animalSoundSource.talk != null
                    ? animal.animalSoundSource.talk
                    : animal.animalSoundSource.crying;

                if (talkClip != null)
                {
                    talkAudioSource.clip = talkClip;
                    talkAudioSource.Play();
                }
            }

        }
        else //동물과 대화 중, 대화를 끝내고 싶을 때
        {
            interector.Guides.SetActive(true); //가이드텍스트 켜기
            AnimalInfoCanvas.gameObject.SetActive(true);
            if (interector.heldObject != null)
                interector.heldObject.SetActive(true);
            CameraController camCtl = cam.GetComponent<CameraController>();
            camField.active = true;
            camCtl.ReturnCam();
            CloseTalkUi();
            playerController.state = PlayerController.State.idle;
            animal.Talk(false);

            //사운드 추가 stop (AnimalController 방식)
            if (talkAudioSource.isPlaying)
                talkAudioSource.Stop();


        }
    }
    private void OpenTalkUi()
    {
        UIManager.instance.talkAnimal.SetActive(true);
    }

    private void CloseTalkUi()
    {
        UIManager.instance.talkAnimal.SetActive(false);
    }
    private void YourFriend()
    {
        FriendBox.SetActive(true);
        Image sprite = animalFriendBox.GetComponent<Image>();
        sprite.sprite = animalBasicBox;
        if(hunger != null && !hunger.enabled)
            hunger.enabled = true;
    }

    public void AnimerRest()
    {
        isSit = true;
        animal.animalState = AnimalController.AnimalState.rest;
        AnimalPos[0].SetActive(false);
        AnimalPos[1].SetActive(true);
        transform.parent = RestPoint;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.parent = null;
    }

    public void EndAnimalRest()
    {
        isSit = false;
        AnimalPos[1].SetActive(false);
        AnimalPos[0].SetActive(true);
    }

    public void SummonFriend()
    {

        //StopCoroutine(animal.check);
        animal.isActive = true;
        LastPosition = transform.position;
        FindFriend();
        if (isFar)
        {
            PlayerController player = Friend.GetComponent<PlayerController>();
            transform.position = player.SummonPos.position;
            rb.isKinematic = false;
            isFar = false;
            animal.animalState = AnimalController.AnimalState.idle;
        }
        isSummon = true;
        summonBtn[0].gameObject.SetActive(false);
        summonBtn[1].gameObject.SetActive(true);
        StartCoroutine(CoolTime(summonBtn[1]));
    }        
    
    public void DeSummonFriend()
    {
        transform.position = LastPosition;
        isSummon = false;
        summonBtn[1].gameObject.SetActive(false);
        summonBtn[0].gameObject.SetActive(true);
        StartCoroutine(CoolTime(summonBtn[0]));
    }

    IEnumerator CoolTime(Button btn)
    {
        btn.interactable = false;
        yield return new WaitForSeconds(summonCoolTime);
        btn.interactable = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.CompareTag("RestPoint") && isSummon))
        {
            RestPoint = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && cam.transform.parent == TalkCam)
        {
            TalkAnimal(other.gameObject);
        }
        if (other.transform == RestPoint)
            RestPoint = null;
    }
}
