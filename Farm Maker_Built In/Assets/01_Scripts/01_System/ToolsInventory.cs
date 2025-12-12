using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsInventory : MonoBehaviour
{
    //싱글톤
    public static ToolsInventory toolsInv;
    private void Awake()
    {
        if (toolsInv == null)
            toolsInv = this;
    }
    // ✅ 인스펙터에서 연결할 대상:
    // 오른쪽 도구 인벤토리 슬롯들을 묶어놓은 부모 오브젝트(예: Grid, Slots 등)
    [SerializeField] private GameObject go_slotParent; // 도구 슬롯들이 들어있는 부모 오브젝트
    public Slot[] toolsSlots;                         // 실제 슬롯들 배열

    [SerializeField] private Slot slot1;
    [SerializeField] private Slot slot2;
    [SerializeField] private Slot slot3;
    [SerializeField] private Slot slot4;
    [SerializeField] private Slot slot5;

    private void Start()
    {
        // 부모 오브젝트 하위에 있는 모든 슬롯 가져오기
        toolsSlots = go_slotParent.GetComponentsInChildren<Slot>();
    }
    // 도구 아이템 추가 (스택 없음 , 한 칸에 하나만 들어감)
    public bool addTool(Item item)
    {
        if(item == null) return false;

        // 빈 슬롯을 찾아 도구 추가
        for(int i = 0; i < toolsSlots.Length; i++)
        {
            if(toolsSlots[i].item == null)
            {
                toolsSlots[i].AddItem(item,1); // slot 구조상 count 필요 -> 1 고정
                return true;
            }
        }
        return false; // 슬롯이 전부 찼을 경우
    }

    // 특정 인덱스의 도구 제거
    public void RemoveTool(int index)
    {
        if(index < 0 || index >= toolsSlots.Length) return;

        if (toolsSlots[index].item != null)
            toolsSlots[index].ClearSlot();
    }

    // 인덱스로 슬롯 가져오기 (장착/퀵슬롯 연동용)
    public Slot GetSlot(int index)
    {
        if(index < 0 || index >= toolsSlots.Length) return null;
        return toolsSlots[index];
    }
    public void EquipToolToHand(int index, PlayerInterector player)
    {
        if (player == null) return;

        Slot targetSlot = GetSlot(index);
        if (targetSlot == null || targetSlot.item == null) return;

        targetSlot.ChangeHandItem();
    }

}
