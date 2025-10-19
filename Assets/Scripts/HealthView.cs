using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private Transform heartContainer;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Debug Info")]
    [SerializeField] private List<Image> heartImages = new List<Image>();

    private void Awake()
    {
        // HeartContainer가 할당되지 않았다면 자동으로 찾기
        if (heartContainer == null)
        {
            heartContainer = transform.Find("HeartContainer");
        }

        // 시작 시 기존 하트들을 리스트에 추가
        CollectExistingHearts();
    }

    // 기존에 만들어진 하트들을 리스트에 수집
    private void CollectExistingHearts()
    {
        heartImages.Clear();

        for (int i = 0; i < heartContainer.childCount; i++)
        {
            Image heartImage = heartContainer.GetChild(i).GetComponent<Image>();
            if (heartImage != null)
            {
                heartImages.Add(heartImage);

                // 기본적으로 가득 찬 하트로 설정
                if (fullHeartSprite != null)
                    heartImage.sprite = fullHeartSprite;
            }
        }

        Debug.Log($"Collected {heartImages.Count} existing hearts");
    }

    // 체력 UI 업데이트 (Model로부터 호출됨)
    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        // 필요한 하트 개수만큼 하트가 있는지 확인
        EnsureHeartCount(maxHealth);

        // 각 하트의 상태 업데이트
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < maxHealth)
            {
                // 최대 체력 범위 내의 하트들 활성화
                heartImages[i].gameObject.SetActive(true);

                // 현재 체력에 따라 하트 스프라이트 변경
                if (i < currentHealth)
                    heartImages[i].sprite = fullHeartSprite;  // 가득 찬 하트
                else
                    heartImages[i].sprite = emptyHeartSprite; // 빈 하트
            }
            else
            {
                // 최대 체력을 초과하는 하트들 비활성화
                heartImages[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"Health UI Updated: {currentHealth}/{maxHealth}");
    }

    // 최대 체력이 변경될 때만 호출 (하트 개수 조정)
    public void OnMaxHealthChanged(int newMaxHealth)
    {
        EnsureHeartCount(newMaxHealth);
        Debug.Log($"Max health changed. Hearts available: {heartImages.Count}");
    }

    // 필요한 하트 개수를 보장하는 메서드
    private void EnsureHeartCount(int requiredCount)
    {
        // 부족한 하트가 있다면 생성
        while (heartImages.Count < requiredCount)
        {
            CreateNewHeart();
        }
    }

    // 새로운 하트 생성
    private void CreateNewHeart()
    {
        if (heartPrefab == null)
        {
            Debug.LogError("Heart Prefab is not assigned!");
            return;
        }

        GameObject newHeartObj = Instantiate(heartPrefab, heartContainer);
        Image newHeartImage = newHeartObj.GetComponent<Image>();

        if (newHeartImage != null)
        {
            heartImages.Add(newHeartImage);

            // 새로 생성된 하트는 기본적으로 가득 찬 상태
            if (fullHeartSprite != null)
                newHeartImage.sprite = fullHeartSprite;

            Debug.Log($"New heart created. Total hearts: {heartImages.Count}");
        }
    }

    // Inspector에서 디버깅용 - 하트 스프라이트 테스트
    [ContextMenu("Test Full Hearts")]
    private void TestFullHearts()
    {
        foreach (var heart in heartImages)
        {
            if (heart.gameObject.activeInHierarchy)
                heart.sprite = fullHeartSprite;
        }
    }

    [ContextMenu("Test Empty Hearts")]
    private void TestEmptyHearts()
    {
        foreach (var heart in heartImages)
        {
            if (heart.gameObject.activeInHierarchy)
                heart.sprite = emptyHeartSprite;
        }
    }
}