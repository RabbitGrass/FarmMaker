using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using UnityEngine.VFX;
using UnityEngine.SceneManagement; //  씬 전환용 추가

public class CraftingMenuData
{
    // 미사용 시 제거 예정
}

public class UIManager : MonoBehaviour
{
    public GameObject MenuTab;//메뉴 탭 전체
    public GameObject Option; //옵션 메뉴

    public GameObject[] Menu;

    public Button[] MenuButton;
    public Button CloseBtn;

    public Slider camSetting;
    public Slider MouseSetting;
    public Slider BackgroundSound;
    public Slider EffectSound;

    public HarvestManager currentHarvestManager;
    public GameObject plantMenu;
    public Button plantConfirmButton; // 새로 추가 - "심기" 버튼

    // 선택한 씨앗의 인덱스
    private int selectedSeed = -1;

    // 요리 선택 창과 요리 제작 창
    public GameObject cooker;
    public GameObject[] cookingMenu;
    public GameObject[] cookingMode;

    // 제작 선택 창
    public GameObject craftingTable;
    public GameObject[] craftingMenu;

    //동물과 말하기
    public GameObject talkAnimal;
    public Button[] talkAnimalBtns;
    private bool isTalkBtnActive; //자식 버튼 오브젝트 활성화 여부

    //동물에게 밥주기
    public GameObject FoodInventory;

    //잠자기 버튼 활성화
    public GameObject IsSleepButton;

    public GameObject Chest; //창고 인벤토리

    public GameObject Map; //지도

    // public Slider progressBar;
    private ITimer activeTimer;

    //ui싱글톤
    public static UIManager instance = null;

    private void Awake()
    {
        if (instance == null) instance = this;

        DefaultSetting();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("CamSlide"))
            camSetting.value = PlayerPrefs.GetFloat("CamSlide");

        plantMenu.SetActive(false);
        talkAnimalBtns = new Button[talkAnimal.transform.childCount];

        for(int i = 0; i < talkAnimal.transform.childCount; i++)
        {
            talkAnimalBtns[i] = talkAnimal.transform.GetChild(i).GetComponent<Button>();
        }
    }

    private void Update()
    {
        if(talkAnimal.activeSelf && FoodInventory.activeSelf && isTalkBtnActive)
        {
            foreach(Button btn in talkAnimalBtns)
                btn.gameObject.SetActive(false);
            isTalkBtnActive = false;
        }
        else if(talkAnimal.activeSelf && !FoodInventory.activeSelf && !isTalkBtnActive)
        {
            foreach (Button btn in talkAnimalBtns)
                btn.gameObject.SetActive(true);
            isTalkBtnActive = true;
        }

        if (Cursor.lockState == CursorLockMode.Locked && UIActives())
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None && !UIActives())
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if (!MenuTab.gameObject.activeSelf)
            if (!UIActives() && !Map.activeSelf)
            {
                HandleShortKey(2);
            }
            else
            {
                CloseBtn.onClick.Invoke();
            }

            if (cooker.activeSelf || craftingTable.activeSelf || plantMenu.activeSelf)
            {
                plantMenu.SetActive(false);
                cooker.SetActive(false);
                craftingTable.SetActive(false);
            }
        }
        if (talkAnimal.activeSelf) //임시로 막아둠, talkAnimal활성화시 전체다 막는게 옳은지 아닌지는 회의 예정
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleShortKey(0);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            HandleShortKey(1);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if(!Map.activeSelf)
                Map.SetActive(true);
            else
                Map.SetActive(false);
        }
    }

    public bool UIActives()
    {
        return MenuTab.gameObject.activeSelf
               || talkAnimal.activeSelf
               || plantMenu.activeSelf
               || cooker.activeSelf
               || craftingTable.activeSelf
               || IsSleepButton.activeSelf;
    }

    public void HandleShortKey(int n)
    {
        if (!MenuTab.gameObject.activeSelf)
        {
            MenuTab.SetActive(true);
            MenuSelect(n);
        }
        else if (Menu[n].activeSelf)
        {
            CloseBtn.onClick.Invoke();
        }
        else
            MenuSelect(n);

    }
    public void MenuSelect(int n)
    {
        MenuButton[n].onClick.Invoke();
    }

    public void OpenMenu(int n)
    {
        for(int i = 0; i < Menu.Length; i++)
        {
            MenuButton[i].interactable = true;
            Menu[i].SetActive(false);
        }

        MenuButton[n].interactable = false;
        Menu[n].SetActive(true);
    }

    public void CloseMenu()
    {
        foreach (var btn in MenuButton)
            btn.interactable = true;

        if (MenuTab.activeSelf == true) Resume();
    }
    public void Resume()
    {
        MenuTab.SetActive(false);
    }

    public void CamSetting()
    {
        PlayerPrefs.SetFloat("CamSlide", camSetting.value);
    }

    public void DefaultSetting()
    {
        camSetting.value = camSetting.maxValue / 2;
        MouseSetting.value = MouseSetting.maxValue / 2;
        BackgroundSound.value = BackgroundSound.maxValue / 2;
        EffectSound.value = EffectSound.maxValue / 2;
    }
    public void OpenPlantMenu(HarvestManager target)
    {
        currentHarvestManager = target;
        selectedSeed = -1;

        var buttons = plantMenu.GetComponentsInChildren<UnityEngine.UI.Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners(); // 중복 제거
            buttons[i].onClick.AddListener(() =>
            {
                Selectseed(index, buttons[index]);
            });
        }

        // 심기
        plantConfirmButton.onClick.RemoveAllListeners();
        plantConfirmButton.onClick.AddListener(ConfirmPlanting);
    }

    private void Selectseed(int index, Button selectedBtn)
    {
        selectedSeed = index;
        print($"씨앗 {index}번 선택됨");
    }

    public void ConfirmPlanting()
    {
        if (selectedSeed == -1)
        {
            Debug.Log("씨앗이 선택되지 않았습니다!");
            return;
        }

        Debug.Log($"씨앗 {selectedSeed}번을 심습니다!");
        StartCoroutine(currentHarvestManager.Planting(selectedSeed));

        // 초기화
        selectedSeed = -1;
        currentHarvestManager = null;
        plantMenu.SetActive(false);
    }

    #region 식사를 합시다
    public void OpenCooker()
    {
        cooker.SetActive(true);
        if (Kitchen.cook.cookState == Kitchen.CookingState.Cool)
            cookingMode[0].SetActive(true);
        else if (Kitchen.cook.cookState == Kitchen.CookingState.Empty)
        {
            cookingMode[0].SetActive(false);
            cookingMode[1].SetActive(true);
        }
            Cursor.lockState = CursorLockMode.None;
    }

    public void OpenCookingMode(int n)
    {
        cookingMode[n].SetActive(true);
        switch(n)
        {
            case 0: cookingMode[1].SetActive(false); break;
            case 1: cookingMode[0].SetActive(false); break;
            default: print("이 메시지는 비정상적인 상황에서 발생하는 메시지입니다."); break;
        }
    }

    public void OpenCookingMenu(int recipe)
    {
        cookingMenu[recipe].SetActive(true);
        switch(recipe)
        {
            case 0:
                cookingMenu[1].SetActive(false);
                cookingMenu[2].SetActive(false);
                break;
            case 1:
                cookingMenu[0].SetActive(false);
                cookingMenu[2].SetActive(false);
                break;
            case 2:
                cookingMenu[0].SetActive(false);
                cookingMenu[1].SetActive(false);
                break;

        }
    }

    public void CookingStart(int foodNumber)
    {
        switch(foodNumber)
        {
            case 0: Kitchen.cook.MakeBread(); break;
            case 1: Kitchen.cook.MakeOmelet(); break;
            case 2: Kitchen.cook.MakePumpkinPie(); break;
            default: print("상우의 가마솥: 요리로 할 만할 걸 가져오셈!"); break;
        }

        cooker.SetActive(false);
    }
    #endregion

    #region 물건을 만듭시다
    public void OpenCraftingTable()
    {
        craftingTable.SetActive(true);
        Cursor.lockState= CursorLockMode.None;
    }

    public void OpenCraftingMenu(int product)
    {
        craftingMenu[product].SetActive(true);
        switch (product)
        {
            case 0:
                craftingMenu[1].SetActive(false);
                craftingMenu[2].SetActive(false);
                craftingMenu[3].SetActive(false);
                break;
            case 1:
                craftingMenu[0].SetActive(false);
                craftingMenu[2].SetActive(false);
                craftingMenu[3].SetActive(false);
                break;
            case 2:
                craftingMenu[0].SetActive(false);
                craftingMenu[1].SetActive(false);
                craftingMenu[3].SetActive(false);
                break;
            case 3:
                craftingMenu[0].SetActive(false);
                craftingMenu[1].SetActive(false);
                craftingMenu[2].SetActive(false);
                break;
        }
    }

    public void CraftingStart(int stuffNumber)
    {
        craftingMenu[stuffNumber].SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        switch(stuffNumber)
        {
            case 0: CraftingPlace.craft.MakeGrain(); break;
            case 1: CraftingPlace.craft.MakeFlour(); break;
            case 2: CraftingPlace.craft.MakeStockFeed(); break;
            case 3: CraftingPlace.craft.MakeKetchup(); break;
            default: print("풍차: 아 나가라고"); break;
        }
    }
    #endregion

    //옵션에서 타이틀로 돌아가기
    // 🔹 인트로 씬으로 돌아가는 메서드 추가
    public void ReturnToIntroScene()
    {
        //// 현재 진행 중인 모든 UI 비활성화
        //MenuTab.SetActive(false);
        //cooker.SetActive(false);
        //craftingTable.SetActive(false);
        //plantMenu.SetActive(false);
        //talkAnimal.SetActive(false);

        //// 마우스 커서 복원
        //Cursor.lockState = CursorLockMode.None;

        // 씬 전환
        SceneManager.LoadScene("_IntroScene");
    }

    /*
    public void BineTimer(ITimer timer)
    {
        activeTimer = timer;
        progressBar.gameObject.SetActive(true);
    }

    public void UpdateProgressBar(ITimer timer)
    {
        if (activeTimer == timer)
        {
            progressBar.value = timer.CurrentTime / timer.TotalTime;
        }
    }

    public void UnbindTimer()
    {
        progressBar.gameObject.SetActive(false);
        activeTimer = null;
    }
    */

    public void QuitGame()
    {
#if UNITY_EDITOR //전처리기, 유니티 상에서 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //애플리케이션을 종료
        Application.Quit();
#endif
    }
}
