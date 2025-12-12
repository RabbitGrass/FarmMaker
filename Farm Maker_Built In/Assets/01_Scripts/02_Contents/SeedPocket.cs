using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class SeedData
{
    public string seedName;
    public int count;
}

public class SeedPocket : MonoBehaviour
{
    // 싱글톤
    public static SeedPocket Seed = null;
    private void Awake()
    {
        if (Seed == null) Seed = this;
    }

    [Header("씨앗 목록")]
    public List<SeedData> seedList = new List<SeedData>();

    [NonSerialized]

    // 씨앗 분류
    public Dictionary<string, int> seeds = new Dictionary<string, int>();

    private void Start()
    {
        seeds.Clear();
        foreach (var seed in seedList)
        {
            seeds[seed.seedName] = seed.count;
            //print($"{seed.seedName} 초기값: {seed.count}");
        }
    }

    public bool HasSeed(string seedName)
    {
        return seeds.ContainsKey(seedName) && seeds[seedName] > 0;
    }

    public void UseSeed(string seedName)
    {
        if (!HasSeed(seedName))
        {
            print($"{seedName} 씨앗이 없는데요...?");
            return;
        }

        seeds[seedName]--;
        UpdateSeedList(seedName, seeds[seedName]);

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void PlusItem(GameObject other)
    {
        ItemManager item = other.GetComponent<ItemManager>();

        // 만약 그 오브젝트가 없다면?
        if (item == null)
        {
            // NullReferenceException 발생확률 100%이므로 그냥 끝내버린다.
            print("아이템매니저가 없습니다!");
            return;
        }
        
        // 만약 그 오브젝트가 씨앗이라면?
        else if (item.itemType == "Seed")  // 여기 코드 한줄 추가 했습니다.
        {
            string seedName = item.itemName;

            // 딕셔너리 갱신 (Slot.AddItem에서 이미 처리함)
            //if (seeds.ContainsKey(seedName))
            //    seeds[seedName]++;
            //else
            //    seeds.Add(seedName, 1);

            //// 리스트도 갱신
            //UpdateSeedList(seedName, seeds[seedName]);

            // 인벤토리 쪽에서 추가하도록 위임 (자동으로 SeedPocket 갱신됨)
            //InventoryManager.inventory.AcquireItem(item.item, 1);  // 여기 추가했습니다.
           
            // ✅ InventoryManager를 통해 씨앗 추가
            //if (InventoryManager.inventory != null)
            //{
            //    InventoryManager.inventory.AcquireItem(item.item, 1);
            //    Debug.Log($"[SeedSync] {seedName} 씨앗을 InventoryManager를 통해 획득했습니다!");
            //}
            //else
            //{
            //    Debug.LogWarning("[SeedPocket] InventoryManager가 없습니다!");
            //}

            other.SetActive(false);
            Debug.Log($"{seedName} 씨앗을 주웠습니다!");

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        else print("얘는 씨앗 아니에요;");
    }

    public void UpdateSeedList(string seedName, int newCount) // 여기 private 에서 public으로 변경했어요!!
    {
        foreach (var seed in seedList)
        {
            if (seed.seedName == seedName)
            {
                seed.count = newCount;
                return;
            }
        }

        seedList.Add(new SeedData { seedName = seedName, count = newCount });
    }

    public int TotalSeeds()
    {
        int total = 0;
        foreach (var count in seeds.Values)
            total += count;
        return total;
    }
    //public void SyncToInventory(string seedName)
    //{
    //    if (seeds.ContainsKey(seedName))
    //    {
    //        foreach (var slot in InventoryManager.inventory.GetComponentsInChildren<Slot>())
    //        {
    //            if (slot.item != null && slot.item.itemName == seedName)
    //            {
    //                slot.SetSlotCount(seeds[seedName]);
    //                break;
    //            }
    //        }
    //    }
    //}

}
