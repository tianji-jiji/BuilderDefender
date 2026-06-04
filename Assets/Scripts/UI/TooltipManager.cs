using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示工具
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [SerializeField] private RectTransform priceTooltip;
    [SerializeField] private RectTransform placementTooltip;
    [SerializeField] private RectTransform efficiencyTooltip;
    [SerializeField] private TMP_Text priceTooltipText;
    [SerializeField] private TMP_Text placementTooltipText;
    [SerializeField] private TMP_Text efficiencyTooltipText;
    [SerializeField] private Image efficiencyTooltipIcon;

    private void Awake()
    {
        Instance = this;

        HidePriceTooltip();
        HidePlacementTooltip();
        HideEfficiencyTooltip();
    }

    private void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        if (priceTooltip.gameObject.activeSelf)
            priceTooltip.position = mousePos + new Vector3(0f, 15f);

        if (placementTooltip.gameObject.activeSelf)
            placementTooltip.position = mousePos + new Vector3(0f, 40f);

        if (efficiencyTooltip.gameObject.activeSelf)
            efficiencyTooltip.position = mousePos + new Vector3(0f, 65f);
    }

    public void ShowPriceTooltip(BuildingSo buildingSo)
    {
        if (!buildingSo) return;
        priceTooltip.gameObject.SetActive(true);

        string text = $"{buildingSo.assetName} Cost:\n";

        foreach (var resourceCost in buildingSo.resourceCost)
        {
            if (resourceCost.resourceSo == null) continue;
            int adjustedAmount = RewardBonusManager.GetAdjustedBuildCostAmount(buildingSo, resourceCost);
            text += $"{resourceCost.resourceSo.resourceName}:{adjustedAmount} ";
        }

        priceTooltipText.text = text;
    }

    public void HidePriceTooltip()
    {
        priceTooltip.gameObject.SetActive(false);
    }

    public void ShowPlacementTooltip(string info)
    {
        placementTooltip.gameObject.SetActive(true);
        placementTooltipText.text = info;
    }

    public void HidePlacementTooltip()
    {
        placementTooltip.gameObject.SetActive(false);
    }

    public void ShowEfficiencyTooltip(float efficiency, Sprite icon)
    {
        if (!icon) return;
        efficiencyTooltip.gameObject.SetActive(true);
        int percent = Mathf.RoundToInt(efficiency * 100f);
        efficiencyTooltipIcon.sprite = icon;
        efficiencyTooltipText.text = $"Efficiency : {percent}%";
    }

    public void HideEfficiencyTooltip()
    {
        efficiencyTooltip.gameObject.SetActive(false);
    }
}
