using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Building/BuildingSoList")]
public class BuildingSoList : ScriptableObject
{
    public List<BuildingSo> list;
}
