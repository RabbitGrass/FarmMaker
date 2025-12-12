using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    //싱글톤
    public static InventoryManager inventory;

    private void Awake()
    {
        if (inventory == null) inventory = this;
    }

    public static bool inventoryActivated = false; // 이건 인벤토리 화면창 나오면 카메라가 안움직이게 만드는것 하지만 이거 안씀 !! 

    //필요한 컴포넌트 확인용 !!
    [SerializeField] private GameObject go_Inventory; // 인벤토리창 !! 하지만 uiManager에 tap 부분에 있음!! 확인용!!
    [SerializeField] private GameObject go_SlotsParent;
    private Slot[] slots; // 슬롯들

    [SerializeField] private GameObject FoodInventory;
    [SerializeField] private GameObject FoodSlotsParent;
    private FoodSlot[] foodSlots;
    //private int available; //인벤토리 갯수

    [SerializeField] private UIManager inventoryUI; //UIManager 가져옴!!


    //public string[,] items;
    private void Start()
    {
        if (inventoryUI == null)
            inventoryUI = GetComponent<UIManager>();
        else
            Debug.LogWarning("inventoryUI가 null입니다!");

        slots = go_SlotsParent.GetComponentsInChildren<Slot>();
        //available = 24;
        foodSlots = FoodSlotsParent.GetComponentsInChildren<FoodSlot>();
        //items = new string[4, 6];
    }
   
    //private void OpenInventory()  //열기  나중에 지울꺼
    //{
    //    go_Inventory.SetActive(true);
    //}
    //private void CloseInventory() // 닫기  나중에 지울꺼
    //{
    //    go_Inventory.SetActive(false);
    //}

    public void AcquireItem(Item _item, int _count) 
    {
        if(_item == null || _count <=0) return;

        //  씨앗이면 AddItem만 수행하고 IncreaseCount는 막기
        bool isSeed = _item.itemType == Item.ItemType.Seed;

        if (Item.ItemType.Equipment != _item.itemType)
        {                     
            for (int i = 0; i < slots.Length; i++)  // 아이템이 있으면 개수 증가
            {
                if (slots[i].item != null)
                {
                    if (slots[i].item.itemName == _item.itemName)
                    {                      
                        slots[i].IncreaseCount(_count);                     
                        return;
                    }
                }
            }
        }
        for (int i = 0; i < slots.Length; i++) // 아이템이 없으면 빈곳에 채워짐!!
        {
            if (slots[i].item == null)
            {               
                slots[i].AddItem(_item, _count);

                if (ToInventoryBox.ToInventory != null)  // 10.28 일 추가함 !!
                {
                    if (_item.itemType == Item.ItemType.Ingredient)
                    {
                        ToInventoryBox.ToInventory.AddItemToInventory(_item.itemName, _count);
                        Debug.Log($"[ToInventorySync] 재료 {_item.itemName} +{_count}개 동기화 완료");
                    }
                    else if (_item.itemType == Item.ItemType.Food)
                    {
                        ToInventoryBox.ToInventory.AddFeedToInventory(_item.itemName);
                        Debug.Log($"[ToInventorySync] 먹이 {_item.itemName} +1개 동기화 완료");
                    }
                }

                //  씨앗의 "최초" 추가는 여기서 SeedPocket 동기화
                if (_item.itemType == Item.ItemType.Seed && SeedPocket.Seed != null)
                {
                    string name = _item.itemName;
                    var dict = SeedPocket.Seed.seeds;
                    if (dict.ContainsKey(name)) dict[name] += _count;
                    else dict.Add(name, _count);
                    SeedPocket.Seed.UpdateSeedList(name, dict[name]);
                }

                // [추가] Refrigerator 동기화 (농산물, 축산물, 식재료, 음식 전부)
                if (Refrigerator.Food != null)
                {
                    string name = _item.itemName;
                    bool synced = false;

                    // 1️ 농산물
                    int cropIdx = Refrigerator.Food.cropProductList.FindIndex(x => x.cropName == name);
                    if (cropIdx >= 0)
                    {
                        for (int j = 0; j < _count; j++)
                            Refrigerator.Food.PlusCropItem(cropIdx);
                        Debug.Log($"[RefrigeratorSync] (Crop) {name} +{_count}개 동기화 완료");
                        synced = true;
                    }

                    // 2️ 축산물
                    int liveIdx = Refrigerator.Food.livestockProductList.FindIndex(x => x.stockProductName == name);
                    if (liveIdx >= 0)
                    {
                        for (int j = 0; j < _count; j++)
                            Refrigerator.Food.PlusLivestockItem(liveIdx);
                        Debug.Log($"[RefrigeratorSync] (Livestock) {name} +{_count}개 동기화 완료");
                        synced = true;
                    }

                    // 3️ 식재료
                    int ingIdx = Refrigerator.Food.ingredientList.FindIndex(x => x.mealName == name);
                    if (ingIdx >= 0)
                    {
                        for (int j = 0; j < _count; j++)
                            Refrigerator.Food.PlusIngredient(ingIdx);
                        Debug.Log($"[RefrigeratorSync] (Ingredient) {name} +{_count}개 동기화 완료");
                        synced = true;
                    }

                    // 4️ 음식
                    int foodIdx = Refrigerator.Food.foodList.FindIndex(x => x.foodName == name);
                    if (foodIdx >= 0)
                    {
                        for (int j = 0; j < _count; j++)
                            Refrigerator.Food.PlusFood(foodIdx);  // 내부에서 +1 하니까 여기서 반복
                        Debug.Log($"[RefrigeratorSync] (Food) {name} + {_count}개 동기화 완료"); // 5번
                        synced = true;
                    }

                    if (!synced)
                        Debug.LogWarning($"[RefrigeratorSync] '{name}' 가 Refrigerator 내부 어떤 리스트에도 없습니다. (인스펙터 확인 필요)");
                }
                return;               
            }
        }
    }

    public void NextItem(ItemManager item)
    {
        foreach(Slot slot in slots)
        {
            if (slot.item != null && slot.item == item.item)
            {
                slot.HoldItem();
                break;
            }
        }
    }

    public void FindFood()
    {
        FoodInventory.SetActive(true);
        int j = 0;
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item != null && slots[i].item.itemType == Item.ItemType.Food)
            {
                foodSlots[j].slot = slots[i];
                foodSlots[j].ViewSlot();
                j++;
            }
        }

        for(; j < foodSlots.Length; j++)
        {
            foodSlots[j].ClearSlot();
        }
    }

    public void FeedCharge(FoodContainer feed)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null && slots[i].item.itemType == Item.ItemType.Feed)
            {
                slots[i].SetSlotCount(slots[i].itemCount-1);
                feed.feed += 100;
                break;
            }
        }
    }
    //public void PutInventory()
    //{

    //}
}
