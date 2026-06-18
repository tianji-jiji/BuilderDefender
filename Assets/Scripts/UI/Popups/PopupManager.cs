using System.Collections.Generic;
using System.Text;
using MoreMountains.Feedbacks;
using UnityEngine;


public class PopupManager : MonoBehaviour
{
    private const float SCREEN_CENTER_RATIO = 0.5f;
    private const string UPGRADE_WARNING_PREFIX = "升级需要:";
    private const float DEFAULT_INTENSITY = 1f;
    private const float UPGRADE_WARNING_INTENSITY = 1f;
    private const int WOOD_POPUP_CHANNEL = 1;
    private const int STONE_POPUP_CHANNEL = 2;
    private const int GOLD_POPUP_CHANNEL = 3;
    private const int UPGRADE_WARNING_CHANNEL = 4;

    public static PopupManager Instance;
    [SerializeField] private ResourceSo woodResourceSo;
    [SerializeField] private ResourceSo stoneResourceSo;
    [SerializeField] private ResourceSo goldResourceSo;

    // 初始化漂浮提示管理器单例。
    private void Awake()
    {
        Instance = this;
    }
    
    // 注册场景中已存在的资源采集器。
    private void Start()
    {
        var harvesters = FindObjectsByType<ResourceHarvester>(FindObjectsSortMode.None);

        foreach (var h in harvesters)
        {
            Register(h);
        }
    }
    
    // 订阅后续创建的资源采集器。
    private void OnEnable()
    {
        ResourceHarvester.OnHarvesterCreated += Register;
    }
    
    // 注册资源采集完成时的漂浮提示回调。
    private void Register(ResourceHarvester harvester)
    {
        harvester.OnResourceHarvestedOneTime += ShowResourcePopup;
    }
    
    // 生成资源采集浮动文字。
    private void ShowResourcePopup(int amount, Vector3 position, ResourceSo resourceSo)
    {
        if (!TryGetResourcePopupChannel(resourceSo, out int channel))
        {
            return;
        }

        TriggerFloatingText(channel, position, $"+{amount}", DEFAULT_INTENSITY);
    }

    // 在屏幕中央显示升星资源不足提示。
    public void ShowUpgradeResourceWarning(IReadOnlyList<ResourceCost> upgradeCost)
    {
        TriggerFloatingText(
            UPGRADE_WARNING_CHANNEL,
            GetScreenCenterWorldPosition(),
            FormatUpgradeResourceWarning(upgradeCost),
            UPGRADE_WARNING_INTENSITY);
    }

    // 获取资源浮动文字频道。
    private bool TryGetResourcePopupChannel(ResourceSo resourceSo, out int channel)
    {
        if (resourceSo == woodResourceSo)
        {
            channel = WOOD_POPUP_CHANNEL;
            return true;
        }

        if (resourceSo == stoneResourceSo)
        {
            channel = STONE_POPUP_CHANNEL;
            return true;
        }

        if (resourceSo == goldResourceSo)
        {
            channel = GOLD_POPUP_CHANNEL;
            return true;
        }

        channel = 0;
        return false;
    }

    // 获取屏幕中心在漂浮提示画布中的世界坐标。
    private Vector3 GetScreenCenterWorldPosition()
    {
        Camera mainCamera = Camera.main;

        if (!mainCamera)
        {
            return Vector3.zero;
        }

        Vector3 screenCenter = new Vector3(
            Screen.width * SCREEN_CENTER_RATIO,
            Screen.height * SCREEN_CENTER_RATIO,
            Mathf.Abs(mainCamera.transform.position.z));

        return mainCamera.ScreenToWorldPoint(screenCenter);
    }

    // 触发 More Mountains 浮动文字生成事件。
    private static void TriggerFloatingText(int channel, Vector3 position, string text, float intensity)
    {
        MMFloatingTextSpawnEvent.Trigger(
            new MMChannelData(MMChannelModes.Int, channel, null),
            position,
            text,
            Vector3.up,
            intensity);
    }

    // 格式化升星消耗提示文本。
    private string FormatUpgradeResourceWarning(IReadOnlyList<ResourceCost> upgradeCost)
    {
        StringBuilder builder = new StringBuilder(UPGRADE_WARNING_PREFIX);

        foreach (var resourceCost in upgradeCost)
        {
            if (resourceCost == null || !resourceCost.resourceSo)
            {
                continue;
            }

            if (builder.Length > UPGRADE_WARNING_PREFIX.Length)
            {
                builder.Append(" / ");
            }

            builder.Append(resourceCost.resourceSo.resourceName);
            builder.Append(' ');
            builder.Append(resourceCost.amount);
        }

        return builder.ToString();
    }
}
