using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeTools : MonoBehaviour
{
    public GameObject handTools;  // 손에 들 도구용
    public GameObject inventoryItem;  // 인벤토리용

    private GameObject heldObject; // 현재 손에 든 아이템
    public bool IsHandEmpty => heldObject == null;

    // 손에 아이템 넣기
    public void PickUpItem(GameObject item)
    {
        if (item == null) return;

        if (IsHandEmpty)
        {
            heldObject = item;
            handTools.SetActive(true);
            inventoryItem.SetActive(false);

            item.SetActive(true); // tools 태그 활성화
            Debug.Log($"{item.name} 손에 들기!");
        }
        else
        {
            heldObject.transform.SetParent(inventoryItem.transform);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;

            handTools.SetActive(false);
            inventoryItem.SetActive(true);

            heldObject = null;
            Debug.Log("아이템 인벤토리로 이동!");
        }
    }
}
