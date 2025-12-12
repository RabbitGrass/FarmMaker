using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSpeed : MonoBehaviour
{
    private Animator anim;
    private readonly int hashSpeed = Animator.StringToHash("Speed");

    private void Start()
    {
        // Animator 컴포넌트 할당
        anim = GetComponent<Animator>();
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 2f));
    }
}
