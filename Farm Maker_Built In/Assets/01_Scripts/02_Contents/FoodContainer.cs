using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FoodContainer : MonoBehaviour
{
    public float maxFeed = 100;
    public float feed = 0;
    private GameObject stock;

    public GameObject InfoBox;
    public Slider FeedBar;
    public TMP_Text StateText;

    private bool isEmpty;

    private void Start()
    {
        FeedBar.maxValue = maxFeed;
        FeedBar.value = feed;
        InfoBox.SetActive(false);
    }

    private void Update()
    {
        if (feed > 0 && isEmpty)
        {
            StateText.text = "통통하게 살 찌우는 중";
            isEmpty = false;
        }
        else if (!isEmpty && feed <= 0)
        {
            StateText.text = "그릇이 텅 비었습니다.";
            isEmpty = true;
        }
    }

    public void FeedCharge() //PlayerInterector에서 F키 눌렀을 때 실행.
    {
        if (feed >= maxFeed)//모이통이 가득 차 있는 경우 실행하지 않는다.
            return; 
        InventoryManager.inventory.FeedCharge(this);  //InventoryManager와 연동해서 모이(feed)를 찾고 모이가 존재할 경우 변수 feed에 100을 더한다.
        if(feed > maxFeed)
            feed = maxFeed; //모이통의 최대치를 넘었을 경우 feed값을 maxFeed값으로 변환시킨다.
        FeedBar.value = feed;
    }

    public void FeedUse(float Used)
    {
        feed -= Used;
        FeedBar.value = feed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StockFarm") && other.gameObject != stock) //모이통이 놓여있는 가축장 기록
        {
            other.GetComponent<StockFarmManager>().feedsCnt = this; //StockFarmManager의 feedsCnt변수에 직접 저장
            stock = other.gameObject;
        }
        else if (other.CompareTag("Player")) //플레이어일 경우
        {
            InfoBox.SetActive(true); //상태창 활성화
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == stock)
        {
            stock = null;
        }
        else if (other.CompareTag("Player"))
        {
            InfoBox.SetActive(false);
        }
    }
}