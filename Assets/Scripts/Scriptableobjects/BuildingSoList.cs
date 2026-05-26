using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Scriptablebojects/BuildingSoList")]
public class BuildingSoList : ScriptableObject
{
    public List<BuildingSo> list;
}
