using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBottle : MonoBehaviour, IToolBag
{
    public bool isFilled = false;
    public GameObject water;
    public Item[] isWater;
    private ItemManager itemManager;

    private void Start()
    {
        itemManager = GetComponent<ItemManager>();
    }

    public void UseTool(PlayerInterector player)
    {
        Debug.Log("유즈툴 실행");
        //Debug.Log(player.target.name);
        if (player.target != null && !isFilled && player.target.layer == LayerMask.NameToLayer("Water"))
        {
            isFilled = true;
            Debug.Log("물통이 채워졌습니다.");
            water.SetActive(true);
            itemManager.item = isWater[1];
        }
        else if(player.target != null && isFilled && player.target.layer == LayerMask.NameToLayer("Farmland"))
        {
            Debug.Log("물 주기 실행");
            HarvestManager farm = player.target.GetComponent<HarvestManager>();
            if (farm.cropState != HarvestManager.CropState.NeedWater)
                return;
            Debug.Log("물 주기 실행 2");


            farm.watering = true;
            isFilled = false;
            water.SetActive(false);
            itemManager.item = isWater[0];
            print("아학 커허헉 크흑!!(식물이 물을 먹는 소리입니다.)");
        }
    }

    public void WorkTool(GameObject player)
    {
        //보류
    }

    public void RestTool(GameObject player)
    {
        // 제작 예정
    }
}
