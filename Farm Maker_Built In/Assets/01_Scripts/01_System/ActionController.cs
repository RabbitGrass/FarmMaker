using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{
    [SerializeField] private Text actionText; // 아이템 습득 안내 텍스트

    //private GameObject nearbyItem; // PlayerInterector에서 감지한 아이템
    [SerializeField] private LayerMask layerMask; //아이템 레이어에만 반응

    // [SerializeField] private float range; // 사용안함 확인용으로 만듬
    // private bool pickupActivated = false; canPickUp 이거랑 같은 거 ! 이것도 사용안함 확인용!!
    //private RaycastHit hitInfo; 충돌체 정보 저장 이것도 사용안함 확인용!

    [SerializeField]
    private InventoryManager theInventory;
   
    private void Awake()
    {
        
        if (theInventory == null)
            theInventory = FindObjectOfType<InventoryManager>();
    }

    private void Update()
    {
        // PlayerInterector에서 감지한 근처 아이템 가져오기
        //UpdateNearbyItem();
     
        // 습득 가능 아이템이 있으면 UI 표시     
        //ShowItemUI(nearbyItem);

        //CheckItem(); 확인용!!
        //TryAction(); 확인용!!
    }

    //private void tryaction() 사용안함 확인용!!
    //{
    //    if (input.getkeydown(keycode.e))
    //    {
    //        checkitem();
    //        CanPickUp();
    //    }
    //}
    //private void CanPickUp()  사용안함 확인용 
    //{
    //    if(hitInfo.transform != null)
    //    {
    //        Destroy(hitInfo.transform.gameObject);
    //        InfoDisappear();
    //    }
    //}
    //private void CheckItem()  // 사용안함 확인용
    //{
    //    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitInfo, RangeAttribute, layerMask))
    //    {
    //        if (hitInfo.transform.tag == "Item")
    //        {
    //            ItemInfoAppear();
    //        }
    //    }
    //    InfoDisappear();
    //}
    //private void InfoDisappear()   사용안함 확인용
    //{
    //    pickupActivated = false;
    //    actionText.gameObject.SetActive(false);
    //}
    //private void ItemInfoAppear()  // 사용안함 확인용
    //{
    //    pickupActivated = true;
    //    actionText.gameObject.SetActive(true);
    //    actionText.text = hitInfo.transform.GetComponent<ItemPickUp>().item.name + "획득 ";
    //}

    // PlayerInterector에서 nearbyItem 가져오기
    //private void UpdateNearbyItem()
    //{       
    //    if (inter != null)
    //    {
    //        nearbyItem = inter.GetNearbyItem(); // null 가능성 있음
    //        if (nearbyItem != null && nearbyItem.layer == LayerMask.NameToLayer("Item"))
    //            canPickUp = true;
    //        else
    //            canPickUp = false;
    //    }
    //    else
    //    {
    //        canPickUp = false;
    //    }
    //}

    //  아이템 근처일 때 UI 텍스트 표시
    //private void ShowItemUI(GameObject item)
    //{
    //    if (actionText == null) return;

    //    if(item != null)
    //    {
    //        // 이미 켜져 있지 않다면 UI 활성화
    //        if (!actionText.gameObject.activeSelf)
    //            actionText.gameObject.SetActive(true);

    //        // 보여줄 텍스트 구성 (나중에 item 이름이나 설명 추가 가능)
    //        actionText.text = $"{item.name} 획득 (<color=yellow>F</color>)";
    //    }
    //    else
    //    {
    //        HideItemUI();
    //    }
    //}

    // UI 숨김
    private void HideItemUI()
    {
        if (actionText == null) return;

        if (actionText.gameObject.activeSelf)
            actionText.gameObject.SetActive(false);
    }

    // PlayerInterector에서 F키 입력 시 호출
    public void TryPickUp(GameObject nearbyItem)
    {
        if (nearbyItem == null) return;

        PickUpItem(nearbyItem);
    }

    // 실제 아이템 습득 처리
    private void PickUpItem(GameObject nearbyItem)
    {      
        if (nearbyItem == null) return;

        if (nearbyItem != null)
        {
            ItemManager itemPickUp = nearbyItem.GetComponent<ItemManager>(); 
 
            if (itemPickUp != null && itemPickUp.item != null)
            { // 아이템 타입 확인
                Item item = itemPickUp.item; // 10.22 추가함!! 

                // 🔹 도구 아이템만 도구 인벤토리에 넣기
                bool added = false;

                if (item.itemType == Item.ItemType.Equipment) // ← 도구(장비)인 경우만
                {
                    ToolsInventory toolInv = FindObjectOfType<ToolsInventory>();
                    if (toolInv != null) added = toolInv.addTool(item);

                    // 점검
                    SFXSoundManager.Instance.Play("ItemPickUp", 0.05f);
                } 

                // 도구 인벤토리가 가득 찼을 경우 일반 인벤토리에 추가
                if (!added)
                {
                    //Debug.Log($"{item.itemName} → 도구 인벤토리 가득참 → 일반 인벤토리에 저장");
                    theInventory.AcquireItem(itemPickUp.item, 1);

                    // 점검
                    SFXSoundManager.Instance.Play("ItemPickUp", 0.05f);


                }
                Destroy(nearbyItem);
            }
            else
            {
                Debug.LogWarning($"{nearbyItem.name}에 ItemPickUp 또는 item이 없습니다.");
            }
            HideItemUI();
        }
    }
}
