using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励卡牌选项组件，负责处理卡牌点击选择和鼠标悬浮交互。
/// </summary>
public class RewardCardOption : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private RewardCardView cardView;
 
    private RewardCardSo _rewardCard;
    private RewardCardController _controller;

    // 缓存卡牌根节点上的按钮引用。
    private void Awake()
    {
        CacheReferences();
    }

    // 注册卡牌点击事件。
    private void OnEnable()
    {
        CacheReferences();

        if (selectButton)
        {
            selectButton.onClick.AddListener(HandleSelectClicked);
        }
    }

    // 取消注册卡牌点击事件。
    private void OnDisable()
    {
        if (selectButton)
        {
            selectButton.onClick.RemoveListener(HandleSelectClicked);
        }
    }

    // 初始化本次实例对应的奖励卡数据和控制器。
    public void Init(RewardCardSo rewardCard, RewardCardController controller)
    {
        _rewardCard = rewardCard;
        _controller = controller;

        if (cardView)
        {
            cardView.SetCard(rewardCard);
        }
    }

    

    // 缓存卡牌选项依赖的组件。
    private void CacheReferences()
    {
        if (!selectButton)
        {
            TryGetComponent(out selectButton);
        }

        if (!cardView)
        {
            TryGetComponent(out cardView);
        }
    }

    // 处理玩家选择当前卡牌的点击行为。
    private void HandleSelectClicked()
    {
        if (!_controller || !_rewardCard)
        {
            return;
        }

        _controller.SelectCard(_rewardCard);
    }
}
