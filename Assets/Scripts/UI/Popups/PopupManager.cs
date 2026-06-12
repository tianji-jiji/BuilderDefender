using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class PopupManager : MonoBehaviour
{
    private const float SCREEN_CENTER_RATIO = 0.5f;
    private const string UPGRADE_WARNING_PREFIX = "升级需要:";

    public static PopupManager Instance;
    [SerializeField] private GameObject goldPopupPrefab;
    [SerializeField] private GameObject stonePopupPrefab;
    [SerializeField] private GameObject woodPopupPrefab;
    [SerializeField] private GameObject upgradeWarningPopupPrefab;
    [SerializeField] private Transform floatingPopupRoot;

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
        harvester.OnResourceHarvestedOneTime += SpawnPopUI;
    }
    
    // 生成资源采集漂浮提示。
    private void SpawnPopUI(int amount, Vector3 position, BuildingSo type)
    {
        GameObject popupUI = SpawnPopup(type.popupUIPrefab, position);

        if (popupUI && popupUI.TryGetComponent(out PopupUI popup))
        {
            popup.SetText($"+{amount}");
        }
    }

    // 在屏幕中央显示升星资源不足提示。
    public void ShowUpgradeResourceWarning(IReadOnlyList<ResourceCost> upgradeCost)
    {
        if (!upgradeWarningPopupPrefab || !floatingPopupRoot)
        {
            return;
        }

        GameObject popupUI = SpawnPopup(upgradeWarningPopupPrefab, GetScreenCenterWorldPosition());

        if (popupUI && popupUI.TryGetComponent(out PopupUI popup))
        {
            popup.SetText(FormatUpgradeResourceWarning(upgradeCost));
        }
    }

    // 生成一个漂浮提示对象。
    private GameObject SpawnPopup(GameObject popupPrefab, Vector3 position)
    {
        if (!popupPrefab)
        {
            return null;
        }

        if (PoolManager.Instance)
        {
            return PoolManager.Instance.Spawn(popupPrefab, position, Quaternion.identity, floatingPopupRoot);
        }

        GameObject popupUI = Instantiate(popupPrefab, floatingPopupRoot);
        popupUI.transform.position = position;
        return popupUI;
    }

    // 获取屏幕中心在漂浮提示画布中的世界坐标。
    private Vector3 GetScreenCenterWorldPosition()
    {
        if (floatingPopupRoot is RectTransform rootRect)
        {
            Vector2 screenCenter = new Vector2(Screen.width * SCREEN_CENTER_RATIO, Screen.height * SCREEN_CENTER_RATIO);
            Canvas canvas = rootRect.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera ? canvas.worldCamera : Camera.main
                : null;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rootRect, screenCenter, uiCamera, out Vector3 worldPosition))
            {
                return worldPosition;
            }
        }

        return floatingPopupRoot.position;
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
