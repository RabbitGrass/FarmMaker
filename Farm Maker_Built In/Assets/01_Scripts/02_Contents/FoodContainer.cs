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

    public void FeedCharge()
    {
        InventoryManager.inventory.FeedCharge(this);
        if(feed > maxFeed)
            feed = maxFeed;
        FeedBar.value = feed;
    }

    public void FeedUse(float Used)
    {
        feed -= Used;
        FeedBar.value = feed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StockFarm") && other.gameObject != stock)
        {
            other.GetComponent<StockFarmManager>().feedsCnt = this;
            stock = other.gameObject;
        }
        else if (other.CompareTag("Player"))
        {
            InfoBox.SetActive(true);
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