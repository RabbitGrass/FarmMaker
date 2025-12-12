using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimer
{
    float TotalTime { get; }
    float CurrentTime { get; }
    bool IsRunning { get; }

    void StartTimer();
    void StopTimer();
    void ResetTimer();
    void Complete();
}
