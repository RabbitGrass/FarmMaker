using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item", menuName ="New Item/item")]  
public class Item : ScriptableObject   // 
{
    public string itemName;  // 아이템의 이름
    public ItemType itemType; // 아이템의 유형
    public string itemDescrip; //아이템 설명
    public Sprite itemImage; // 아이템 이미지
    public GameObject ItemPrefab; // 아이템 프리팹

    public enum ItemType
    {
        Equipment,   // 장비
        Used,        // 소모품
        Ingredient,  // 재료
        Seed,        // 씨앗 
        Food,        // 음식
        Crop,        // 농산물
        ETC,         // 기타
        Feed         // 동물 음식
    }
    
}
