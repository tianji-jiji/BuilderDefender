using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 难度系统
/// </summary>
public class DifficultySystem
{
    public int WaveIndex;
    public float SpawnCountMultiplier => 1f + WaveIndex * 0.15f;
    public float SpawnInterval => Mathf.Clamp(0.6f - WaveIndex * 0.02f, 0.05f, 0.6f);
}