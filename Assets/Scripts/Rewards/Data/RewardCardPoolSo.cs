using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励卡池数据资产，负责保存候选卡牌与单次抽取配置。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardPoolSo")]
public class RewardCardPoolSo : ScriptableObject
{
    private const int DEFAULT_CHOICE_COUNT = 3;

    [SerializeField] private List<RewardCardSo> rewardCardList = new();
    [SerializeField] private int choiceCount = DEFAULT_CHOICE_COUNT;
    [SerializeField] private bool allowDuplicate;

    public IReadOnlyList<RewardCardSo> RewardCardList => rewardCardList;
    public int ChoiceCount => Mathf.Max(1, choiceCount);
    public bool AllowDuplicate => allowDuplicate;
}
