using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockFarmManager : MonoBehaviour
{
    public FoodContainer feedsCnt; //FoodContainer가 존재할경우 OnTriggerEnter를 통해 저장 예정
    private HashSet<AnimalHunger> animals = new HashSet<AnimalHunger>();

    private void Update()
    {
        if (animals.Count == 0 || feedsCnt == null)
            return;

        foreach(var hunger in animals)
        {
            if(hunger.hungerState >= AnimalHunger.HungerState.hungry && feedsCnt.feed > 0)
            {
                float foodCnt = hunger.MaxHunger - hunger.hunger;

                if (feedsCnt.feed - foodCnt < 0)
                {
                    foodCnt += (feedsCnt.feed - foodCnt);
                }

                hunger.hunger += foodCnt;
                feedsCnt.feed -= foodCnt;
                feedsCnt.FeedBar.value = feedsCnt.feed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AnimalHunger hunger = other.GetComponent<AnimalHunger>(); //배고픔이 존재하는 동물이 들어와있을경우
        if (hunger != null)
            animals.Add(hunger); //HashSet에 가축 저장
    }
}
