using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandsUp
{
    void InHandTransform();

    void HandDownTransform();

    void Throw(GameObject player, float ThrowPower);
}
