using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SleepController : MonoBehaviour
{
    public GameObject sleepUi;
    public GameObject sleepText;
    public GameObject choiceSleep;
    private Button[] ChoiceButton;
    public SphereCollider col;
    public PlayerController player;
    public PlayerInterector playerInter;

    private void Start()
    {
        ChoiceButton = choiceSleep.GetComponentsInChildren<Button>();
    }

    private void Update()
    {
        if (!choiceSleep.activeSelf || playerInter == null)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            ChoiceButton[0].onClick.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChoiceButton[1].onClick.Invoke();
        }
    }
    public void PressF() //기본값
    {
        choiceSleep.SetActive(false);
        sleepText.SetActive(true);

        if(Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void IsChoice() //선택값
    {
        if (col.enabled == false)
            return;

        sleepText.SetActive(false);
        choiceSleep.SetActive(true);
        IsSleepChoice(true);
        Cursor.lockState = CursorLockMode.None;
    }

    public void Sleeping()
    {
        sleepUi.SetActive(false); //자는거 선택했을 때 유아이 끔
        player.SpawnPoint = transform; // 스폰지점 변경 가능성 대비 및 스폰지점 저장
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void IsSleepChoice(bool isSleep)
    {
        if (playerInter == null)
            return;

        playerInter.isSleepChoice = isSleep;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sleepUi.SetActive(true); //플레이어일때 슬립 유아이 활성화
            player = other.GetComponent<PlayerController>(); //멀티 가능성 대비
            playerInter = other.GetComponent<PlayerInterector>();
            PressF();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sleepUi.SetActive(false);
            IsSleepChoice(false);
        }
    }
}
