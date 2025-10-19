using UnityEngine;

public class AffectionTester : MonoBehaviour
{
    // AffectionTester는 현재 사용되지 않음
    // EmotionTester의 Alpha1-5 키로 감정 테스트 가능
    // 추후 확장 시 다른 기능으로 활용 예정

    void Update()
    {
        // 현재 모든 감정 테스트는 EmotionTester (Alpha1-5)로 처리됨
        // 필요시 여기에 다른 테스트 기능 추가 가능
    }

    // 기존 메서드는 유지 (다른 스크립트에서 호출 가능)
    public void SetNpc1Affection(LoveModel.Emotion emotion)
    {
        if (PlayerDataManager.Instance?.CurrentPlayer != null)
        {
            PlayerDataManager.Instance.CurrentPlayer.Npc1Affection = emotion;
            Debug.Log($"PlayerData.Npc1Affection set to: {emotion}");
        }
        else
        {
            Debug.LogError("PlayerDataManager or CurrentPlayer is null!");
        }
    }
}