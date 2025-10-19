using UnityEngine;

public class EmotionTester : MonoBehaviour
{
    public BubbleManager bubbleManager;

    void Update()
    {
        if (bubbleManager == null || bubbleManager.Model == null) return;

        // 키보드로 감정 상태 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetEmotion(LoveModel.Emotion.Angry);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetEmotion(LoveModel.Emotion.Sad);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetEmotion(LoveModel.Emotion.Idle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetEmotion(LoveModel.Emotion.Happy);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetEmotion(LoveModel.Emotion.Love);
        }
    }

    // 감정 변경 시 PlayerData와 LoveModel 모두 업데이트
    private void SetEmotion(LoveModel.Emotion emotion)
    {
        // PlayerData 업데이트 (퀘스트 시스템이 감지하도록)
        if (PlayerDataManager.Instance?.CurrentPlayer != null)
        {
            PlayerDataManager.Instance.CurrentPlayer.Npc1Affection = emotion;
            Debug.Log($"🎯 [EMOTION TESTER] PlayerData.Npc1Affection updated to: {emotion}");
        }

        // LoveModel 업데이트 (기존 시스템 호환)
        if (bubbleManager?.Model != null)
        {
            bubbleManager.Model.CurrentEmotion = emotion;
            Debug.Log($"🎯 [EMOTION TESTER] LoveModel.CurrentEmotion updated to: {emotion}");
        }
    }
}