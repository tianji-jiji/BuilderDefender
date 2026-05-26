using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuildButtonUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private BuildingSo buildingSo;
    private BuildMenuUI _buildMenuUI;

    private void Awake()
    {
        _buildMenuUI = GetComponentInParent<BuildMenuUI>();
    }

    /// <summary>
    /// 被点击了做什么
    /// </summary>
    public void OnClick()
    {
        _buildMenuUI.SelectButton(this);
        BuildManager.Instance.SetCurrentBuilding(buildingSo);
    }

    /// <summary>
    /// 设置自身高亮
    /// </summary>
    /// <param name="selected"></param>
    public void SetSelectedVisual(bool selected)
    {
        selectedImage.gameObject.SetActive(selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowPriceTooltip(buildingSo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HidePriceTooltip();
    }
}
