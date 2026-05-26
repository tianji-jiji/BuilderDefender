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
        SelectButton(defaultButton);
    }

    /// <summary>
    /// 选择按钮
    /// </summary>
    /// <param name="button"></param>
    public void SelectButton(BuildButtonUI button)
    {
        // 取消上一个按钮的高亮
        if (_currentSelectedButton != null)
        {
            _currentSelectedButton.SetSelectedVisual(false);
        }

        // 保存新的按钮
        _currentSelectedButton = button;

        // 让新的按钮高亮
        _currentSelectedButton.SetSelectedVisual(true);
    }
}
