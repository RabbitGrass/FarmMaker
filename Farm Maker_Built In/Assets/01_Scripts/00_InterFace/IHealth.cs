using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    void Damaged(float dmg);

    void Heal(float heal);
}
