using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingDemolitionButton : MonoBehaviour
{
    [SerializeField] private Building building;
    [SerializeField] private BuildingSo buildingSo;
    [SerializeField] private Button button;

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            // 拆除建筑
            Destroy(building.gameObject);
            // 返回一定金钱
            foreach (var resourceCost in buildingSo.resourceCost)
            {
                ResourceManager.Instance.AddResource(resourceCost.resourceSo,
                    Mathf.FloorToInt(resourceCost.amount * buildingSo.demolishRefundMultiplier));
            }
        });
    }
}