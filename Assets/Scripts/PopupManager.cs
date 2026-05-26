using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
    [SerializeField] private GameObject goldPopupPrefab;
    [SerializeField] private GameObject stonePopupPrefab;
    [SerializeField] private GameObject woodPopupPrefab;
    [SerializeField] private Transform floatingPopupRoot;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        var harvesters = FindObjectsOfType<ResourceHarvester>();

        foreach (var h in harvesters)
        {
            Register(h);
        }
    }
    
    private void OnEnable()
    {
        ResourceHarvester.OnHarvesterCreated += Register;
    }
    
    private void Register(ResourceHarvester harvester)
    {
        harvester.OnResourceHarvestedOneTime += SpawnPopUI;
    }
    
    private void SpawnPopUI(int amount, Vector3 position, BuildingSo type)
    {
        // 生成在世界坐标
        var popupUI = Instantiate(type.popupUIPrefab, floatingPopupRoot);

        // 放到世界位置
        popupUI.transform.position = position;

        popupUI.GetComponent<PopupUI>().SetText($"+{amount}");
    }
}