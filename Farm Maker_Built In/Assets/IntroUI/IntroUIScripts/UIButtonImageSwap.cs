using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonImageSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Start, Option, Exit, Return, Reset }
    public ButtonType buttonType;

    public Image buttonImage;
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite pressedSprite;

    void Start()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        buttonImage.sprite = normalSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = hoverSprite;
        IntroUISoundManager.Instance?.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = normalSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = pressedSprite;
        IntroUISoundManager.Instance?.PlayClick(0.1f);

        switch (buttonType)
        {
            case ButtonType.Start:
                Debug.Log(" 게임 시작");
                IntroUIManager.Instance.OnClickStart();
                break;

            case ButtonType.Option:
                ToggleOptionPanel(true);
                break;

            case ButtonType.Return:
                ToggleOptionPanel(false);
                break;

            case ButtonType.Exit:
                Debug.Log(" 종료");
                Application.Quit();
#if UNITY_EDITOR
                // 에디터 환경에서는 실제 종료가 안되니까 콘솔에서 멈춤
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    void ToggleOptionPanel(bool open)
    {
        // Canvas 내에서 직접 탐색 (비활성화 포함)
        Transform canvas = GameObject.Find("Canvas").transform;

        Transform subTitle = canvas.Find("SubTitle_button");
        Transform optionPanel = canvas.Find("OptionPanel");

        if (subTitle != null)
            subTitle.gameObject.SetActive(!open); // 열면 끄고, 닫으면 켜고

        if (optionPanel != null)
            optionPanel.gameObject.SetActive(open); // 열면 켜고, 닫으면 끔

        Debug.Log(open ? "옵션 열기" : "옵션 닫기");
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.sprite = hoverSprite;
    }
}