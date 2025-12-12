using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpController : MonoBehaviour, IHealth
{
    public float MaxHp;
    public float Hp;
    public Slider HpBar;
    private void Start()
    {
        Hp = MaxHp;
        HpBar.maxValue = MaxHp;
        HpBar.value = Hp;
    }

    public void Damaged(float dmg)
    {
        Hp -= dmg;
        HpBar.value = Hp;
        if (Hp > 0)
            return;
    }

    public void LevelUp()
    {
        MaxHp += 5;
        Hp = MaxHp;
        HpBar.maxValue = MaxHp;
        HpBar.value = Hp;
    }

    public void Heal(float heal) //회복
    {
        Hp += heal;

        if (Hp > MaxHp)
            Hp = MaxHp;

        StartCoroutine(Recovery());
    }

    IEnumerator Recovery() //회복 코루틴
    {
        while(HpBar.value < Hp)
        {
            yield return new WaitForSeconds(0.1f);
            HpBar.value += 0.1f;
        }

        if (HpBar.value > Hp)
            HpBar.value = Hp;
    }
}
