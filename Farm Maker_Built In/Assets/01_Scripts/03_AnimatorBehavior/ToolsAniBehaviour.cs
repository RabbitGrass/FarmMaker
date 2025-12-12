using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsAniBehaviour : StateMachineBehaviour
{
    // 스테이트에서 나올 때 호출됨 (애니메이션 끝날 때)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("애니메이션 끝남!");
        // 원하는 코드 실행
    }
}
