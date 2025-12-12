using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantStartButton : MonoBehaviour
{
    public HarvestManager farmland;

    public int seedNumber = -1;

    public void StartGrowing()
    {
        if(seedNumber >= 0)
        farmland.PlantSeed(seedNumber);
    }

    public void SeedNum(int n)
    {
        seedNumber = n;
        print($"[PlantStartButton] ¼±ÅÃµÈ ¾¾¾Ñ ÀÎµ¦½º: {n}");
    }
}
