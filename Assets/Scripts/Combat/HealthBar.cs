using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private GameObject bar;
    [SerializeField] private SpriteRenderer[] renderers;

    private bool _isSubscribed;

    // 启用血条时订阅生命变化并立即同步显示状态。
    private void OnEnable()
    {
        SubscribeHealthSystem();
        UpdateBarSize();
    }

    // 禁用血条时取消订阅，避免对象池复用后重复监听。
    private void OnDisable()
    {
        UnsubscribeHealthSystem();
    }

    // 订阅生命系统的变化事件。
    private void SubscribeHealthSystem()
    {
        if (_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnHealthChanged += UpdateBarSize;
        _isSubscribed = true;
    }

    // 取消订阅生命系统的变化事件。
    private void UnsubscribeHealthSystem()
    {
        if (!_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnHealthChanged -= UpdateBarSize;
        _isSubscribed = false;
    }

    // 根据当前生命百分比调整红色血条宽度。
    private void UpdateBarSize()
    {
        if (!healthSystem || !bar)
        {
            return;
        }

        bar.transform.localScale = new Vector3(healthSystem.CurrentHealthNormalized, 1f, 1f);
        UpdateBarVisible();
    }
    
    // 满血时隐藏血条渲染器，受伤时重新显示。
    private void UpdateBarVisible()
    {
        if (!healthSystem)
        {
            SetRenderersVisible(false);
            return;
        }

        SetRenderersVisible(healthSystem.CurrentHealth < healthSystem.MaxHealth);
    }

    // 设置血条相关渲染器的显示状态。
    private void SetRenderersVisible(bool visible)
    {
        if (renderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }
}
