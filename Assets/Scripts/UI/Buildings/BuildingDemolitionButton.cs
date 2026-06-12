using UnityEngine;
using UnityEngine.UI;

public class BuildingDemolitionButton : MonoBehaviour
{
    [SerializeField] private Building building;
    [SerializeField] private BuildingSo buildingSo;
    [SerializeField] private Button button;
    [SerializeField] private GameObject confirmRoot;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public bool IsAwaitingConfirmation => confirmRoot && confirmRoot.activeSelf;

    // 初始化拆除按钮和确认按钮监听。
    private void Start()
    {
        if (button)
        {
            button.onClick.AddListener(ShowConfirmation);
        }

        if (confirmButton)
        {
            confirmButton.onClick.AddListener(ConfirmDemolition);
        }

        if (cancelButton)
        {
            cancelButton.onClick.AddListener(CancelDemolition);
        }

        HideConfirmation();
    }

    // 显示拆除确认按钮组。
    private void ShowConfirmation()
    {
        if (confirmRoot)
        {
            confirmRoot.SetActive(true);
        }
    }

    // 确认拆除建筑，并返还配置中的部分资源。
    private void ConfirmDemolition()
    {
        if (!building || !buildingSo)
        {
            return;
        }

        HideConfirmation();

        foreach (ResourceCost resourceCost in buildingSo.resourceCost)
        {
            ResourceManager.Instance.AddResource(
                resourceCost.resourceSo,
                Mathf.FloorToInt(resourceCost.amount * buildingSo.demolishRefundMultiplier)
            );
        }

        Destroy(building.gameObject);
    }

    // 取消拆除确认并收起当前拆除 UI。
    private void CancelDemolition()
    {
        HideConfirmation();
        gameObject.SetActive(false);
    }

    // 隐藏拆除确认按钮组。
    private void HideConfirmation()
    {
        if (confirmRoot)
        {
            confirmRoot.SetActive(false);
        }
    }

    // 启用拆除 UI 时先回到未确认状态。
    private void OnEnable()
    {
        HideConfirmation();
    }

    // 注销按钮监听，避免对象销毁后继续持有回调。
    private void OnDestroy()
    {
        if (button)
        {
            button.onClick.RemoveListener(ShowConfirmation);
        }

        if (confirmButton)
        {
            confirmButton.onClick.RemoveListener(ConfirmDemolition);
        }

        if (cancelButton)
        {
            cancelButton.onClick.RemoveListener(CancelDemolition);
        }
    }
}
