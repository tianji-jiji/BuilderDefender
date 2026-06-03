using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑建造器
/// </summary>
public class BuildingConstructor : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> buildingSpriteList;
    [SerializeField] private string progressPropertyName = "_Progress";
    [SerializeField] private float materialProgressUpdateInterval;
    [SerializeField] private GameObject constructionProgressBarPrefab;
    [SerializeField] private Vector3 progressBarOffset;

    private BuildingSo _buildingSo;
    private Action _onConstructionCompleted;
    private SpriteRenderer _activeSpriteRenderer;
    private Image _progressBarImage;
    private Coroutine _constructionCoroutine;
    private MaterialPropertyBlock _materialPropertyBlock;
    private float _lastMaterialProgressUpdateTime;
    private GameObject particlePrefab;

    private void Awake()
    {
        particlePrefab = Resources.Load<GameObject>("Particles/BuildingPlacedParticles");
    }

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
        Instantiate(_buildingSo.prefab, transform.position, Quaternion.identity);
        SpawnPlacedParticles();
        _onConstructionCompleted?.Invoke();
        Destroy(gameObject);
    }

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

    private void UpdateProgressBar(float progress)
    {
        if (!_progressBarImage)
            return;

        _progressBarImage.fillAmount = progress;
    }

    // 生成建筑建造完成粒子。
    private void SpawnPlacedParticles()
    {
        if (!particlePrefab)
        {
            return;
        }

        if (PoolManager.Instance)
        {
            PoolManager.Instance.Spawn(particlePrefab, transform.position, Quaternion.identity);
            return;
        }

        Instantiate(particlePrefab, transform.position, Quaternion.identity);
    }
}
