using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理子按钮的选中
/// </summary>
public class BuildMenuUI : MonoBehaviour
{
    // 目前选中的按钮
    private BuildButtonUI _currentSelectedButton;
    [SerializeField] private BuildButtonUI defaultButton;
    
    private void Start()
    {
        if (BuildManager.Instance)
        {
            BuildManager.Instance.OnSelectedBuildingChanged += OnSelectedBuildingChanged;
        }

        SelectButton(defaultButton);
    }

    /// <summary>
    /// 选择按钮
    /// </summary>
    /// <param name="button"></param>
    public void SelectButton(BuildButtonUI button)
    {
        // 取消上一个按钮的高亮
        if (_currentSelectedButton)
        {
            _currentSelectedButton.SetSelectedVisual(false);
        }

        if (!button)
        {
            _currentSelectedButton = null;
            return;
        }

        // 保存新的按钮
        _currentSelectedButton = button;

        // 让新的按钮高亮
        _currentSelectedButton.SetSelectedVisual(true);
    }

    // 当前建筑选择被清空时同步取消按钮高亮。
    private void OnSelectedBuildingChanged(BuildingSo buildingSo)
    {
        if (!buildingSo)
        {
            SelectButton(null);
        }
    }

    // 注销建筑选择事件，避免界面销毁后仍收到回调。
    private void OnDestroy()
    {
        if (BuildManager.Instance)
        {
            BuildManager.Instance.OnSelectedBuildingChanged -= OnSelectedBuildingChanged;
        }
    }
}
