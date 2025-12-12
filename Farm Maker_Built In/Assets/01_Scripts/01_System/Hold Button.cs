using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldButton : MonoBehaviour
{
    private Slot currentSlot;

    //public ActionController pickup;
    // 클릭한 슬롯 정보를 전달
    public void SetSlot(Slot slot)
    {
        currentSlot = slot;
    }
    public void OnClickConfirmHand()
    {
        //if (currentSlot == null) return;
        //// 착용 처리
        //currentSlot.ChangeHandItem();
        if (currentSlot == null || currentSlot.item == null) return;

        PlayerInterector playerInterector = currentSlot.playerInterector;
        //if (playerInterector == null)
        //{
        //    Debug.LogWarning("[HoldButton] PlayerInterector를 찾을 수 없습니다!");
        //    return;
        //}

        // 🔸 손에 이미 아이템이 있다면 → 빈 슬롯으로 옮기기
        //if (playerInterector.heldObject != null) //싹다 필요없는 코드들
        //{
            //ItemManager heldManager = playerInterector.heldObject.GetComponent<ItemManager>();
            //if (heldManager != null)
            //{
            //    // 빈 슬롯 찾아서 손 아이템 이동
            //    //for (int i = 0; i < ToolsInventory.toolsInv.toolsSlots.Length; i++)
            //    //{
            //    //    Slot emptySlot = ToolsInventory.toolsInv.GetSlot(i);
            //    //    if (emptySlot != null && emptySlot.item == null)
            //    //    {
            //    //        emptySlot.AddItem(heldManager.item, 1);
            //    //        Destroy(playerInterector.heldObject);
            //    //        playerInterector.heldObject = null;
            //    //        break;
            //    //    }
            //    //}
                
            //}
        //}
        // 🔸 현재 슬롯의 아이템에서 1개만 손으로 옮기기
        GameObject itemObj = Instantiate(currentSlot.item.ItemPrefab);

        // 슬롯 개수 감소 (1개만 사용)
        currentSlot.SetSlotCount(currentSlot.itemCount - 1);

        // 손에 장착
        playerInterector.HandleItem(itemObj);

        // 버튼 2개 다 숨김
        //Debug.Log($"[HoldButton] {currentSlot.item.itemName} 착용 완료 (남은 개수: {currentSlot.itemCount})");
    }

    public void EatFood()
    {
        FoodManager food = Instantiate(currentSlot.item.ItemPrefab).GetComponent<FoodManager>();
        food.EatMe(currentSlot.playerInterector.gameObject);

        currentSlot.SetSlotCount(currentSlot.itemCount - 1);
    }
}
