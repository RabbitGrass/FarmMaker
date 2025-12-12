using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AnimalHunger : MonoBehaviour
{
    public enum HungerState
    {
        full,       //배부름
        nomal,      //배고프지는 않음
        hungry,     //배고픔
        veryHungry, //매우 배고품
        starvation //기아상태
    }

    public HungerState hungerState;
    private HungerState lastHungerState;
    public float MaxHunger;
    public float hunger;
    private AnimalHpManager hp;
    public GameObject stateInfo;
    public TMP_Text hungerText;
    private AnimalController cnt;
#nullable enable
    private AnimalFriendManager? friendManager;
#nullable restore

    private void OnEnable()
    {
        //hunger = MaxHunger;
        hunger = MaxHunger / 2;
        hp = GetComponent<AnimalHpManager>();
        lastHungerState = hungerState + 1;
    }

    private void Start()
    {
        friendManager = GetComponent<AnimalFriendManager>();
        //hungerText = stateInfo.GetComponentInChildren<TMP_Text>();
        stateInfo.SetActive(false);
        cnt = GetComponent<AnimalController>();
        this.enabled = false;
    }

    private void Update()
    {
        if (hunger > 0)
        { 
            hunger -= 0.01f * Time.deltaTime;
            //Debug.Log(hunger);
        }   
        else if(hunger <= 0)
        {
            hp.Damaged(0.021f * Time.deltaTime); //대략 2일 뒤 사망
        }

        float ishungry = (hunger / MaxHunger) * 100;
        if(ishungry > 90 && ishungry <= 100)
        {
            hungerState = HungerState.full;
        }
        else if(ishungry > 50)
        {
            hungerState = HungerState.nomal;
        }
        else if(ishungry > 20)
        {
            hungerState = HungerState.hungry;
        }
        else if(ishungry > 0)
        {
            hungerState = HungerState.veryHungry;
        }
        else
        {
            hungerState = HungerState.starvation;
        }

        if (lastHungerState == hungerState)
            return;

        switch (hungerState)
        {
            case HungerState.full:
                hungerText.text = "배부른 상태이다.";
                break;
            case HungerState.nomal:
                hungerText.text = "여전히 건강한 상태이다";
                break;
            case HungerState.hungry:
                hungerText.text = "배고픈 상태이다.";
                break;
            case HungerState.starvation:
                hungerText.text = "굶주린 상태이다.";
                break;
            default:
                hungerText.text = "위험한 상태이다.";
                break;
        }

        lastHungerState = hungerState;
        lastHungerState = hungerState;

    }

    public void EatFood(float food, GameObject player)
    {
        AnimalController animal = GetComponent<AnimalController>();
        animal.animalState = AnimalController.AnimalState.eat;

        //동물 먹이기 음향효과 추가
        SFXSoundManager.Instance.PlayAtPoint("playerEatClip", transform.position);

        hunger += food;
        if (hunger > MaxHunger)
            hunger = MaxHunger;

        if (player == null) //플레이어가 준 것이 아닌 땅에 떨어진거 먹은 거라면
            return;

        AnimalFriendManager friend = gameObject?.GetComponent<AnimalFriendManager>();
        if (friend != null)
        {
            float rnd = Random.Range(0, food);
            friend.FriendShip(rnd, player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AnimalHunger animalHunger = GetComponent<AnimalHunger>();
        if ((friendManager != null && friendManager.isFirend) || animalHunger.enabled)
            return;
        if (other.CompareTag("StockFarm"))
        {
            animalHunger.enabled = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && this.enabled)
        {
            if(cnt.animalState != AnimalController.AnimalState.caught && !stateInfo.activeSelf)
                stateInfo.SetActive(true);
            else if(cnt.animalState == AnimalController.AnimalState.caught && stateInfo.activeSelf)
                stateInfo.SetActive(false);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && this.enabled)
        {
            stateInfo.SetActive(false);
        }
        if (friendManager != null && friendManager.isFirend)
            return;
        if (other.CompareTag("StockFarm"))
        {
            AnimalHunger hunger = GetComponent<AnimalHunger>();
            hunger.enabled = false;
            stateInfo.SetActive(false);
        }
    }
}
