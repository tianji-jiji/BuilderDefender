using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑建造器，负责建造过程表现、进度更新和建造完成后生成正式建筑。
/// </summary>
public class BuildingConstructor : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> buildingSpriteList;
    [SerializeField] private string progressPropertyName = "_Progress";
    [SerializeField] private float materialProgressUpdateInterval;
    [SerializeField] private GameObject constructionProgressBarPrefab;
    [SerializeField] private Vector3 progressBarOffset;
    [SerializeField] private GameObject placedParticlePrefab;

    private BuildingSo _buildingSo;
    private Action _onConstructionCompleted;
    private SpriteRenderer _activeSpriteRenderer;
    private Image _progressBarImage;
    private Coroutine _constructionCoroutine;
    private MaterialPropertyBlock _materialPropertyBlock;
    private float _lastMaterialProgressUpdateTime;

    // 初始化本次建造的配置和完成回调。
    public void Init(BuildingSo buildingSo, Action onConstructionCompleted = null)
    {
        _buildingSo = buildingSo;
        _onConstructionCompleted = onConstructionCompleted;
        _materialPropertyBlock ??= new MaterialPropertyBlock();

        ShowConstructionSprite();
        CreateProgressBar();
        SetProgress(0f, true);

        if (_constructionCoroutine != null)
        {
            StopCoroutine(_constructionCoroutine);
        }

        _constructionCoroutine = StartCoroutine(Construct());
    }

    // 创建建造进度条并缓存填充图片。
    private void CreateProgressBar()
    {
        _progressBarImage = null;

        if (constructionProgressBarPrefab)
        {
            GameObject progressBar = Instantiate(constructionProgressBarPrefab, transform);
            progressBar.transform.localPosition = progressBarOffset;
            progressBar.SetActive(true);
            _progressBarImage = progressBar.GetComponentInChildren<Image>(true);
            return;
        }

        _progressBarImage = GetComponentInChildren<Image>(true);
    }

    // 显示与当前建筑配置匹配的建造中贴图。
    private void ShowConstructionSprite()
    {
        _activeSpriteRenderer = null;

        foreach (SpriteRenderer buildingSprite in buildingSpriteList)
        {
            bool isTargetSprite = buildingSprite.sprite &&
                                  buildingSprite.sprite.name == _buildingSo.assetName;

            buildingSprite.gameObject.SetActive(isTargetSprite);

            if (isTargetSprite)
            {
                _activeSpriteRenderer = buildingSprite;
            }
        }

        if (!_activeSpriteRenderer && buildingSpriteList.Count > 0)
        {
            _activeSpriteRenderer = buildingSpriteList[0];
            _activeSpriteRenderer.gameObject.SetActive(true);
        }
    }

    // 按建筑配置的建造时间推进进度。
    private IEnumerator Construct()
    {
        float constructionTime = Mathf.Max(0.01f, _buildingSo.constructionTime);
        float timer = 0f;

        while (timer < constructionTime)
        {
            timer += Time.deltaTime;
            SetProgress(timer / constructionTime);
            yield return null;
        }

        SetProgress(1f, true);
        CompleteConstruction();
    }

    // 完成建造，生成正式建筑、播放粒子、通知回调并销毁建造中对象。
    private void CompleteConstruction()
    {
        Instantiate(_buildingSo.prefab, transform.position, Quaternion.identity);
        SpawnPlacedParticles();
        _onConstructionCompleted?.Invoke();
        Destroy(gameObject);
    }

    // 设置建造进度材质参数和进度条填充。
    private void SetProgress(float progress, bool forceUpdateMaterial = false)
    {
        progress = Mathf.Clamp01(progress);

        if (!_activeSpriteRenderer)
        {
            UpdateProgressBar(progress);
            return;
        }

        if (forceUpdateMaterial || materialProgressUpdateInterval <= 0f ||
            Time.time >= _lastMaterialProgressUpdateTime + materialProgressUpdateInterval)
        {
            _activeSpriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(progressPropertyName, progress);
            _activeSpriteRenderer.SetPropertyBlock(_materialPropertyBlock);
            _lastMaterialProgressUpdateTime = Time.time;
        }

        UpdateProgressBar(progress);
    }

    // 更新建造进度条显示。
    private void UpdateProgressBar(float progress)
    {
        if (!_progressBarImage)
        {
            return;
        }

        _progressBarImage.fillAmount = progress;
    }

    // 生成建筑建造完成粒子。
    private void SpawnPlacedParticles()
    {
        if (!placedParticlePrefab)
        {
            return;
        }

        if (PoolManager.Instance)
        {
            PoolManager.Instance.Spawn(placedParticlePrefab, transform.position, Quaternion.identity);
            return;
        }

        Instantiate(placedParticlePrefab, transform.position, Quaternion.identity);
    }
}
