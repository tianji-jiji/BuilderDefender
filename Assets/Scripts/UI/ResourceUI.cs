using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 右上角资源面板
/// </summary>
public class ResourceUI : MonoBehaviour
{
    private ResourceSoList _resourceSoList;

    [SerializeField] private TextMeshProUGUI woodAmountText;
    [SerializeField] private TextMeshProUGUI stoneAmountText;
    [SerializeField] private TextMeshProUGUI goldAmountText;
    
    private void Awake()
    {
        _resourceSoList = Resources.Load<ResourceSoList>("ResourceSoList");
    }

    private void Start()
    {
        ResourceManager.Instance.OnResourceAmountChanged += UpdateAmountText;
    }

    private void UpdateAmountText()
    {
        woodAmountText.text  = ResourceManager.Instance.GetResourceAmount(_resourceSoList.list[0]).ToString();
        stoneAmountText.text  = ResourceManager.Instance.GetResourceAmount(_resourceSoList.list[1]).ToString();
        goldAmountText.text  = ResourceManager.Instance.GetResourceAmount(_resourceSoList.list[2]).ToString();
    }

    private void OnDisable()
    {
        ResourceManager.Instance.OnResourceAmountChanged -= UpdateAmountText;
    }
}
