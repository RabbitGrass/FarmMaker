using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;

// 만약 게임이 플레이되는 환경이 유니티에디터라면, 난 기꺼이 에디터의 기능을 사용하겠다.
#if UNITY_EDITOR
using UnityEditor;
#endif

# region 냉장고 속 내 사랑하는 밥들
// 농산물
[Serializable]
public class CropData
{
    // 작물의 이름과 수량
    public string cropName;
    public int count;
}

// 축산물
[Serializable]
public class LivestockData
{
    // 축산품의 이름과 수량
    public string stockProductName;
    public int count;
}

// 식재료
[Serializable]
public class IngredientData
{
    // 식재료의 이름과 수량
    public string mealName;
    public int count;
}

// 음식
[Serializable]
public class FoodData
{
    // 음식의 이름과 수량
    public string foodName;
    public int count;
}

#endregion

public class Refrigerator : MonoBehaviour
{
    #region 싱글톤
    public static Refrigerator Food = null;
    private void Awake()
    {
        if (Food == null) Food = this;
    }
    #endregion

    #region 외부 리스트
    // 작물 분류, 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("작물 목록")]
    public List<CropData> cropProductList = new List<CropData>();

    // 축산품 분류 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("축산품 목록")]
    public List<LivestockData> livestockProductList = new List<LivestockData>();

    // 식재료 분류, 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("식재료 목록")]
    public List<IngredientData> ingredientList = new List<IngredientData>();

    // 음식 분류, 하단에 있는 딕셔너리와 연동됩니다. 외장 시스템
    [Header("음식 목록")]
    public List<FoodData> foodList = new List<FoodData>();

    #endregion

    #region 내부 딕셔너리
    [NonSerialized]
    // 작물 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> cropProducts = new Dictionary<string, int>();

    // 축산품 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> livestockProducts = new Dictionary<string, int>();

    // 식재료 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> ingredients = new Dictionary<string, int>();

    // 음식 분류, 상단에 있는 리스트와 연동됩니다. 내장 시스템
    public Dictionary<string, int> foods = new Dictionary<string, int>();
    #endregion

    // Refrigerator 클래스 안, 필드 구역 적당한 위치에 추가
    [Header("인벤토리에 넣을 결과 아이템(이름과 매칭)")]
    public Item[] items;

    private int lastFoodFrame;

    private void Start()
    {
        // 리스트의 값을 딕셔너리로 전달합니다.
        ToInnerDictionary();
    }

    #region 딕셔너리-리스트 상호작용
    private void ToInnerDictionary()  // 내장 시스템으로 이동
    {
        // 저장된 정보의 중복을 방지하기 위해 딕셔너리의 공간을 깨끗하게 정리해줍시다.
        cropProducts.Clear();
        livestockProducts.Clear();
        foods.Clear();

        // 그리고 리스트에 있는 정보를 가져와 작물의 이름에 따라 그 수량을 넣어줍니다.
        foreach (var crop in cropProductList)
        {
            cropProducts[crop.cropName] = crop.count;
        }
        foreach (var livestock in livestockProductList)
        {
            livestockProducts[livestock.stockProductName] = livestock.count;
        }
        foreach (var meal in ingredientList)
        {
            ingredients[meal.mealName] = meal.count;
        }
        foreach (var food in foodList)
        {
            foods[food.foodName] = food.count;
        }

    }

    private void ToOuterList()  // 외장 시스템으로 이동
    {
        // Debug.Log("[Check] ToOuterList() 호출됨");
        // 내부 로직에 따라 딕셔너리의 값이 변동된다면, 리스트에도 이 사실을 알려줘야 합니다.
        foreach (var crop in cropProductList)
        {
            // 딕셔너리 내부에서 리스트에 저장한 작물의 이름이 있다면,
            if (cropProducts.ContainsKey(crop.cropName))
            {
                // 그 수량을 변경된 딕셔너리의 값으로 조절해줍니다.
                crop.count = cropProducts[crop.cropName];
            }
        }
        foreach (var livestock in livestockProductList)
        {
            Debug.Log($"[SyncTest] {livestock.stockProductName} -> {livestockProducts.ContainsKey(livestock.stockProductName)}");

            if (livestockProducts.ContainsKey(livestock.stockProductName))
            {
                livestock.count = livestockProducts[livestock.stockProductName];
            }
        }
        foreach (var meal in ingredientList)
        {
            if (ingredients.ContainsKey(meal.mealName))
            {
                meal.count = ingredients[meal.mealName];
            }
        }
        foreach (var food in foodList)
        {
            if (foods.ContainsKey(food.foodName))
            {
                food.count = foods[food.foodName];
            }
        }
             
    }
    #endregion

    #region 작물 관리 로직
    public void PlusCropItem(int cropIndex)
    {
        // 리스트로부터 작물의 이름을 가져옵니다.
        string name = cropProductList[cropIndex].cropName;

        // 딕셔너리에 저장된 작물의 이름이 없으면 그 작물은 존재하지 않는 겁니다.
        if (!cropProducts.ContainsKey(name)) cropProducts[name] = 0;

        // 딕셔너리에 수확한 작물의 정보를 저장합니다.
        cropProducts[name]++;
        print($"{name} +1 (총 {cropProducts[name]})");

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();
    }

    public bool HasCrop(string cropName, int amount = 1)
    {
        // 작물의 이름 존재 여부와 그 작물이 양이 있는지를 파악합니다.
        return cropProducts.ContainsKey(cropName) && cropProducts[cropName] >= amount;
    }

    public bool UseCrop(string cropName, int amount)
    {
        // 일단 딕셔너리에 해당 작물이 존재하는지와 그 작물의 재고가 있는지 확인합니다.
        if (cropProducts.ContainsKey(cropName) && cropProducts[cropName] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            cropProducts[cropName] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            //  어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{cropName} 읎어요");
        return false;
    }
    #endregion

    #region 축산품 관리 로직
    public void PlusLivestockItem(int livestockIndex)
    {
        Debug.Log($"[Check] PlusLivestockItem() 실행됨 / index = {livestockIndex}");

        // 리스트로부터 축산물의 이름을 가져옵니다.
        string name = livestockProductList[livestockIndex].stockProductName;

        // 딕셔너리에 저장된 축산물의 이름이 없으면 그 축산물은 존재하지 않는 겁니다.
        if (!livestockProducts.ContainsKey(name)) livestockProducts[name] = 0;

        // 딕셔너리에 수확한 축산물의 정보를 저장합니다.
        livestockProducts[name]++;
        print($"{name} +1 (총 {livestockProducts[name]})");  

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public bool HasLivestock(string livestockName, int amount = 1)
    {
        // 이름 존재 여부와 양이 있는지를 파악합니다.
        return livestockProducts.ContainsKey(livestockName) && livestockProducts[livestockName] >= amount;
    }

    public bool UseLivestock(string livestockName, int amount)
    {
        // 일단 딕셔너리에 해당 축산물이 존재하는지와 재고가 있는지 확인합니다.
        if (livestockProducts.ContainsKey(livestockName) && livestockProducts[livestockName] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            livestockProducts[livestockName] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            //  어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{livestockName} 읎어요");
        return false;
    }
    #endregion

    #region 식재료 관리 로직
    public void PlusIngredient(int mealIndex)
    {
        // 리스트로부터 식재료의 이름을 가져옵니다.
        string name = ingredientList[mealIndex].mealName;

        // 딕셔너리에 저장된 식재료의 이름이 없으면 그 식재료는 존재하지 않는 겁니다.
        if (!ingredients.ContainsKey(name)) ingredients[name] = 0;

        // 딕셔너리에 얻은 식재료의 정보를 저장합니다.
        switch (name)
        {
            case "Grain": ingredients[name] += 3; break;
            case "Flour": ingredients[name] += 3; break;
            case "Ketchup": ingredients[name]++; break;
            default: print("냉장고: 음식 아닌 거 절대 사절."); break;
        }

        print($"{name} 추가 완료 (총 {ingredients[name]}");

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();
    }
    public bool HasIngredient(string name, int amount = 1)
    {
        // 식재료의 이름 존재 여부와 그 음식이 양이 있는지를 파악합니다.
        return ingredients.ContainsKey(name) && ingredients[name] >= amount;
    }
    public bool UseIngredient(string name, int amount)
    {
        // 일단 딕셔너리에 해당 식재료가 존재하는지와 재고가 있는지 확인합니다.
        if (ingredients.ContainsKey(name) && ingredients[name] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            ingredients[name] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            //  어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{name} 읎어요");
        return false;
    }
    #endregion

    #region 음식 관리 로직
    public void PlusFood(int foodIndex)
    {       
        // 리스트로부터 음식의 이름을 가져옵니다.
        string name = foodList[foodIndex].foodName;

        // ✅ 중복 호출 방지: 이전 프레임에서 이미 실행되었다면 무시
        if (Time.frameCount == lastFoodFrame) return;
        lastFoodFrame = Time.frameCount;

        // 딕셔너리에 저장된 음식의 이름이 없으면 그 음식은 존재하지 않는 겁니다.
        if (!foods.ContainsKey(name)) foods[name] = 0;

      
        // 딕셔너리에 수확한 음식의 정보를 저장합니다.
        foods[name]++;    
        print($"{name} +1 (총 {foods[name]})");  // 1 번

        // 리스트로 변동된 정보를 보내줍니다.
        ToOuterList();

        // ✅ 인벤토리 동기화 추가
        if (InventoryManager.inventory != null)
        {
            Item itemToAdd = null;

            // 이름과 인스펙터에 연결한 Item을 매칭
            if (name == "Bread") itemToAdd = items[0];
            else if (name == "Omelet") itemToAdd = items[1];
            else if (name == "PumpkinPie") itemToAdd = items[2];

            if (itemToAdd != null)
            {
                Debug.Log($"[Refrigerator] '{name}' 매칭 완료 → {itemToAdd.itemName}");  // 2번
                InventoryManager.inventory.AcquireItem(itemToAdd, 1);               
                Debug.Log($"[Refrigerator] 인벤토리에 '{itemToAdd.itemName}' 추가 완료!");  // 4번
            }
            else
            {
                Debug.LogWarning($"[Refrigerator] '{name}' 대응 Item이 인스펙터에 연결되지 않음.");
            }
        }
        else
        {
            Debug.LogWarning("[Refrigerator] InventoryManager.inventory 가 null 입니다!");
        }

    }

    public bool HasFood(string foodName, int amount = 1)
    {
        // 작물의 이름 존재 여부와 그 음식이 양이 있는지를 파악합니다.
        return foods.ContainsKey(foodName) && foods[foodName] >= amount;
    }

    public bool EatFood(string foodName, int amount)
    {
        // 일단 딕셔너리에 해당 음식이 존재하는지와 재고가 있는지 확인합니다.
        if (foods.ContainsKey(foodName) && foods[foodName] >= amount)
        {
            // 재고 있으면 사용하는 것이니, 빼줍시다.
            foods[foodName] -= amount;

            // 외부 리스트에 이 사실을 알리는 걸 잊으면 안 됩니다.
            ToOuterList();

            //  어찌됐든 승인됐으니 true를 반환해줍니다.
            return true;
        }

        // 만약 위 조건을 만족하지 못하면 아래 메시지를 남겨주고 반려해줍니다.
        print($"{foodName} 읎어요");
        return false;
    }
    #endregion
    public void UpdateItemCount(string name, int newCount)
    {   
        bool updated = false;
        if (cropProducts.ContainsKey(name))
        {
            cropProducts[name] = newCount;
            updated = true;
            Debug.Log($"[Refrigerator] 작물 '{name}' 개수 갱신 → {newCount}");
        }

        if (livestockProducts.ContainsKey(name))
        {
            livestockProducts[name] = newCount;
            updated = true;
            Debug.Log($"[Refrigerator] 축산물 '{name}' 개수 갱신 → {newCount}");
        }

        if (ingredients.ContainsKey(name))
        {
            ingredients[name] = newCount;
            updated = true;
            Debug.Log($"[Refrigerator] 식재료 '{name}' 개수 갱신 → {newCount}");
        }

        if (foods.ContainsKey(name))
        {
            foods[name] = newCount;
            updated = true;
            Debug.Log($"[Refrigerator] 음식 '{name}' 개수 갱신 → {newCount}");   // 3번
        }

        if (updated)
        {
            ToOuterList();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"[Refrigerator] '{name}' 이름의 항목이 어느 분류에도 존재하지 않습니다!");
        }
    } 
}