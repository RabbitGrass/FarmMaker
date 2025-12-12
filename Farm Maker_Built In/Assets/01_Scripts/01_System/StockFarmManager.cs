using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockFarmManager : MonoBehaviour
{
    public FoodContainer feedsCnt;
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
        AnimalHunger hunger = other.GetComponent<AnimalHunger>();
        if (hunger != null)
            animals.Add(hunger);
    }
}
