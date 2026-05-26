using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private float moveUpDistance = 8f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float fadeDuration = 0.4f; 
    private CanvasGroup _canvasGroup;
    private Vector3 _startPos;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    private void Start()
    {
        _startPos = transform.position;
        PlayAnimation();
    }
    private void PlayAnimation()
    {
        // 缓动上升
        transform.DOMoveY(_startPos.y + moveUpDistance, duration)
            .SetEase(Ease.OutCubic);

        // 缩放弹出
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * 0.04f, 0.3f)
            .SetEase(Ease.OutBack);
        
        // 淡出
        _canvasGroup.DOFade(0, fadeDuration)
            .SetDelay(duration - fadeDuration);

        // 自动销毁
        DOVirtual.DelayedCall(duration, () =>
        {
            Destroy(gameObject);
        });
    }
    public void SetText(string amount)
    {
        amountText.text = amount;
    }
}
