using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 만약 게임이 플레이되는 환경이 유니티에디터라면, 난 기꺼이 에디터의 기능을 사용하겠다.
#if UNITY_EDITOR
using UnityEditor;
#endif

#region 데이터 기록
[Serializable]
public class ItemData
{
    public string materialName;
    public int count;
}

[Serializable]
public class FeedData
{
    public string feedName;
    public int count;
}
#endregion

public class ToInventoryBox : MonoBehaviour
{
    #region 싱글톤
    public static ToInventoryBox ToInventory = null;
    private void Awake()
    {
        if (ToInventory == null) ToInventory = this;
    }
    #endregion

    #region 리스트

    // 자재 분류, 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("자재 목록")]
    public List<ItemData> materialList = new List<ItemData>();

    // 먹이 분류, 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("먹이 목록")]
    public List<FeedData> feedList = new List<FeedData>();

    #endregion

    #region 딕셔너리
    [NonSerialized]

    // 자재 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> materials = new Dictionary<string, int>();

    // 먹이 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> feeds = new Dictionary<string, int>();

    #endregion

    private void Start()
    {
        ToInnerDictionary();
    }

    #region 딕셔너리-리스트 상호작용
    private void ToInnerDictionary()  // 내장 시스템으로 이동
    {
        // 저장된 정보의 중복을 방지하기 위해 딕셔너리의 공간을 깨끗하게 정리해줍시다.
        materials.Clear();
        feeds.Clear();

        // 그리고 리스트에 있는 정보를 가져와 이름에 따라 그 수량을 넣어줍니다.
        foreach (var item in materialList)
        {
            materials[item.materialName] = item.count;
        }
        foreach (var feed in feedList)
        {
            feeds[feed.feedName] = feed.count;
        }
    }

    private void ToOuterList()  // 외장 시스템으로 이동
    {
        // 내부 로직에 따라 딕셔너리의 값이 변동된다면, 리스트에도 이 사실을 알려줘야 합니다.
        foreach (var item in materialList)
        {
            // 딕셔너리 내부에서 리스트에 저장한 이름이 있다면,
            if (materials.ContainsKey(item.materialName))
            {
                // 그 수량을 변경된 딕셔너리의 값으로 조절해줍니다.
                item.count = materials[item.materialName];
            }
        }
        foreach(var feed in feedList)
        {
            // 딕셔너리 내부에서 리스트에 저장한 이름이 있다면,
            if (feeds.ContainsKey(feed.feedName))
            {
                // 그 수량을 변경된 딕셔너리의 값으로 조절해줍니다.
                feed.count = feeds[feed.feedName];
            }
        }
    }
    #endregion

    #region 자재 관리 로직
    public void AddItemToInventory(string itemName, int delta =1)  // 조금 수정하겠습니다.
    {
        // 리스트로부터 얻은 아이템의 이름을 가져옵니다.
        // string name = materialList[matNumber].materialName;

        // 1. 유효성 검사  추가함!!
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("[AddItemToInventory] itemName이 비어 있습니다!");
            return;
        }

        // 딕셔너리에 저장된 이름이 없으면 그 아이템은 존재하지 않는 겁니다.
        if (!materials.ContainsKey(itemName)) materials[itemName] = 0;

        // 딕셔너리에 수확한 작물의 정보를 저장합니다.
        materials[itemName] += delta;  //  materials[itemName]++  여기 수정했습니다
       
        // 4. 로그 출력 (몇 개 더했는지 표시)
        print($"{itemName} {(delta >= 0 ? "+" : "")}{delta} (총 {materials[itemName]})"); // 여기 추가했습니다.
        //print($"{itemName} +1 (총 {materials[itemName]})");

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public bool HasItemInInventory(string name, int amount = 1)
    {
        // 작물의 이름 존재 여부와 물건이 양이 있는지를 파악합니다.
        return materials.ContainsKey(name) && materials[name] >= amount;
    }

    public bool UseItemAndDelete(string name, int amount)
    {
        // 일단 딕셔너리에 해당 물건이 존재하는지와 그 재고가 있는지 확인합니다.
        if (materials.ContainsKey(name) && materials[name] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            materials[name] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            // 어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{name} 읎어요");
        return false;
    }
    #endregion

    #region 먹이 관리 로직
    public void AddFeedToInventory(string feedName)
    {
        // 딕셔너리에 저장된 이름이 없으면 그 아이템은 존재하지 않는 겁니다.
        if (!feeds.ContainsKey(feedName)) feeds[feedName] = 0;

        // 딕셔너리에 수확한 먹이의 정보를 저장합니다.
        feeds[feedName]++;
        print($"{feedName} +1 (총 {feeds[feedName]})");

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();
    }

    public bool HasFeedInInventory(string name, int amount = 1)
    {
        // 먹이의 이름 존재 여부와 물건이 양이 있는지를 파악합니다.
        return feeds.ContainsKey(name) && feeds[name] >= amount;
    }

    public bool UseFeed(string name, int amount)
    {
        // 일단 딕셔너리에 해당 물건이 존재하는지와 그 재고가 있는지 확인합니다.
        if (feeds.ContainsKey(name) && feeds[name] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            feeds[name] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            // 어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{name} 읎어요");
        return false;
    }
    public void UpdateItemCount(string name, int newCount)
{
    if (string.IsNullOrEmpty(name))
    {
        Debug.LogWarning("[UpdateItemCount] 이름이 비어 있습니다!");
        return;
    }

    // 자재(재료)
    if (materials.ContainsKey(name))
    {
        materials[name] = newCount;
        Debug.Log($"[ToInventoryBox] 자재 '{name}' 개수 갱신 → {newCount}");
    }

    // 먹이
    if (feeds.ContainsKey(name))
    {
        feeds[name] = newCount;
        Debug.Log($"[ToInventoryBox] 먹이 '{name}' 개수 갱신 → {newCount}");
    }

    ToOuterList();

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(this);
#endif
}
    #endregion
}
