using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button SaveButton;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 잠그기

        //Application.targetFrameRate = 100;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveButton.onClick.Invoke();
        }
    }
}
