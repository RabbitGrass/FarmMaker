using System.Collections;
using System.Collections.Generic;
using TMPro;
using Ultrabolt.SkyEngine;
using UnityEngine;

public class SkyController : MonoBehaviour
{
    //public GameObject playerPos;
    public PlayerController playerctl;
    SkyCore skyCore;
    private float skySpeed;
    public float sleepSkySpeed;
    bool isSleepTime = false;
    private int sleepDayCount;
    //Vector3 vector;
    public SphereCollider home;

    public TMP_Text dayCount;
    public TMP_Text dayState;
    private string lastDayState;

    private void Start()
    {
        skyCore = GetComponent<SkyCore>();

        skySpeed = skyCore.timeSpeed; //시간이 흐르는 초기 스피드 구하기

        //playerctl = GetComponent<PlayerController>();
        ThrowDay();
        ChangeDayState();
    }
    private void Update()
    {
        if (lastDayState != skyCore.dayState)
            ChangeDayState();


        //if(skyCore.dayState == "Evening" && home.enabled == false)
        //    home.enabled = true;
        //else if(skyCore.dayState == "Morning" && home.enabled == true)
        //    home.enabled = false;

        if (skyCore.lastTimeState == GameTime.Morning && isSleepTime && sleepDayCount < skyCore.dayCount)
        {
            skyCore.timeSpeed = skySpeed;

            playerctl.WakeUp();
            isSleepTime = false;
        }

        if (isSleepTime)
            return;

        if ((playerctl.state == PlayerController.State.sleep || playerctl.state == PlayerController.State.KO))
        {
            skyCore.timeSpeed = sleepSkySpeed;
            sleepDayCount = skyCore.dayCount;
            isSleepTime = true;
        }
    }

    private void ThrowDay()
    {
        dayCount.text = "Day " + skyCore.dayCount.ToString();
    }

    private void ChangeDayState()
    {
        dayState.text = skyCore.dayState;
        lastDayState = skyCore.dayState;
        if(skyCore.dayState == "Morning")
            ThrowDay();
    }
}
