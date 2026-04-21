using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    static public DragSlot instance;

    public Slot dragSlot;
    public bool isDrag;
    //public ChestSlot dragChestSlot;
    // 아이템 이미지;
    [SerializeField] private Image imageItem;

    private void Awake()
    {
        instance = this;
        //startPosition = transform.position;
        if(imageItem == null)
            imageItem = GetComponent<Image>();
        SetColor(0);
        imageItem.raycastTarget = false;
    }

    private void Update()
    {
        if (isDrag)
            transform.position = Input.mousePosition;
    }

    public void DragSetImage(Image _itemImage)
    {
        if (_itemImage == null) return;
        imageItem.sprite = _itemImage.sprite;
        isDrag = true;
        SetColor(1);
    }

    public void SetColor(float _alpha)
    {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;
    }
    public void ClearSlot()
    {
        imageItem.sprite = null;
        isDrag = false;
        dragSlot = null;
        SetColor(0);
    }
}
