using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveFoodBtn : MonoBehaviour
{
    private FoodSlot currentSlot;

    public void SetSlot(FoodSlot slot)
    {
        currentSlot = slot;
    }

    public void OnClick()
    {
        currentSlot.GiveFood();
    }
}
