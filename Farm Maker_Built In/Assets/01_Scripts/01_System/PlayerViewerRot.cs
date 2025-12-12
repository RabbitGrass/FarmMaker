using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerViewerRot : MonoBehaviour
{
    public GameObject player;
    PlayerController playercntl;
    public float rotSpeed;
    float rotY;
    float rotX;

    private float slerpSpeed;

    //캠 슬라이드 value
    public Slider CamSlider;

    public float mouseSense;

    public Slider MouseSensBar;

    private void Start()
    {
        mouseSense = MouseSensBar.value;
        playercntl = player.GetComponent<PlayerController>();

        //Vector3 rot = transform.localEulerAngles;

        //rot.y = player.transform.localEulerAngles.y;
        //transform.localEulerAngles = rot;

        rotX = player.transform.eulerAngles.y;
        rotY = player.transform.eulerAngles.x;
    }
    private void LateUpdate()
    {
        if (playercntl.state == PlayerController.State.ready)
            return;

        if ((playercntl.state == PlayerController.State.sleep || playercntl.state == PlayerController.State.KO))
        {
            if(transform.eulerAngles != Vector3.zero)
            {
                rotX = player.transform.eulerAngles.y;
                rotY = player.transform.eulerAngles.x;
            }
            return;
        }
        else if (playercntl.state == PlayerController.State.talk|| playercntl.state == PlayerController.State.useTool && !Input.GetKey(KeyCode.LeftAlt))
        {
            //아래 내용을 실행하지 않는 경우.
        }
        else
        {
            slerpSpeed = CamSlider.value;
            mouseSense = MouseSensBar.value;
            transform.position = player.transform.position;
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            float X = Input.GetAxis("Mouse X") * mouseSense;
            float y = Input.GetAxis("Mouse Y") * mouseSense;

            rotX += rotSpeed * X * Time.deltaTime;
            rotY += rotSpeed * y * Time.deltaTime;

            rotY = Mathf.Clamp(rotY, -90, 10); //범위 제한
        }

        // 목표 회전
        Quaternion targetRot = Quaternion.Euler(-rotY, rotX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * slerpSpeed);
        

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            //rotX = PlayerController.ps.Rot;
            rotX = player.transform.eulerAngles.y;
            //transform.localEulerAngles = playerTrans.localEulerAngles;
        }
        //참고로 Damp는 회전용으로는 적합하지 않다. 회전용은 Slerp를 써야한다.
        //sleping은 Damp를 사용했을 때 반응속도로 사용한 damping(반응속도 변수)와 달리 숫자가 크면 클수록 빠르게 움직인다.

        //transform.eulerAngles = new Vector3(-rotY, rotX, 0); //마우스가 아래로 향하면 시점이 위로 가고 마우스가 위를 향하면 시점이 아래로 향하기 위해 -를 붙임
   
    }
}
