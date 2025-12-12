using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropButton : MonoBehaviour
{
    private Slot currentSlot;
    //public TMP_InputField inputCount;

    //private void Start()
    //{
    //    // inputfield가 존재하면 엔터(submit) 이벤트 등록
    //    if (inputCount != null)
    //        inputCount.onSubmit.AddListener(OnSubmitInput);
    //}

    // 클릭한 슬롯 정보를 전달
    public void SetSlot(Slot slot)
    {
        currentSlot = slot;
    }

    // 수량 입력 후 확인 버튼 클릭
    public void OnClickConfirmDrop()
    {
        if (currentSlot == null) return;

        //// InputField 먼저 활성화
        //if (inputCount != null)
        //{
        //    inputCount.gameObject.SetActive(true);
        //    inputCount.Select();              // 입력 포커스
        //    inputCount.ActivateInputField();  // 키보드 활성화
        //}
        int count = int.Parse(currentSlot.itemCountText.text);
        currentSlot.ConfirmDrop(count);
    }

    public void ItemCount(int n)
    {
        currentSlot.UsedItemCount(n);

    }
    //private void OnSubmitInput(string text)
    //{
    //    Debug.Log($"[DropButton] OnSubmitInput 호출됨! 입력값: {text}");
    //    if (currentSlot == null)
    //    {
    //        Debug.LogWarning("currenSlot이 null");
    //        return;
    //    }

    //    int count = 1;
    //    if (int.TryParse(text, out int result))
    //        count = Mathf.Max(1, result);

    //    Debug.Log($"[DropButton] ConfirmDrop 실행: {count}");
    //    currentSlot.ConfirmDrop(count);
    //    inputCount.gameObject.SetActive(false); // 입력 끝나면 닫기
    //    // 버튼도 같이 닫기                                      
    //    if (currentSlot != null)
    //    {
    //        if (currentSlot.dropButton != null)
    //            currentSlot.dropButton.SetActive(false);

    //        if (currentSlot.holdButton != null)
    //            currentSlot.holdButton.SetActive(false);
    //    }
    //} 
}
