using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================================
// Role   : 일반 이동/상호작용(Player 맵)과 배틀 입력(Battle 맵) 전환 컨트롤러
// Input  : Unity Input System(생성된 InputSystem_Actions 바인딩 사용)
// Judge  : 배틀 상태에서 레인 입력 → BeatJudgeSystem.HandleInput 위임
// ============================================================================

public class PlayerController : MonoBehaviour,
    InputSystem_Actions.IPlayerActions,
    InputSystem_Actions.IBattleActions
{
    // 배틀 판정기(씬에서 할당)
    public BeatJudgeSystem judge;

    private InputSystem_Actions input;
    private Vector2 moveInput;
    private bool isBattle = false;

    private void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.SetCallbacks(this);
        input.Battle.SetCallbacks(this);
    }

    private void OnEnable()
    {
        EnableCurrentMap();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    // 상태 전환: 배틀
    public void SwitchToBattle()
    {
        isBattle = true;
        EnableCurrentMap();
    }

    // 상태 전환: 일반(레벨)
    public void SwitchToLevel()
    {
        isBattle = false;
        EnableCurrentMap();
    }

    // 현재 상태에 맞는 액션맵 Enable
    private void EnableCurrentMap()
    {
        input.Disable();

        if (isBattle)
        {
            input.Battle.Enable();
        }
        else
        {
            input.Player.Enable();
        }

        // UI 맵은 항상 병행 사용(필요 시 정책 변경)
        input.UI.Enable();
    }

    // ───────────────── Player Actions ─────────────────

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!isBattle)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!isBattle && ctx.performed)
        {
            Debug.Log("Interact!");
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Cancel pressed");
        }
    }

    // ───────────────── Battle Actions ─────────────────

    public void OnLaneA(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
        {
            if (judge != null)
            {
                judge.HandleInput(LaneId.A);
            }
        }
    }

    public void OnLaneB(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
        {
            if (judge != null)
            {
                judge.HandleInput(LaneId.B);
            }
        }
    }

    public void OnLaneC(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
        {
            if (judge != null)
            {
                judge.HandleInput(LaneId.C);
            }
        }
    }

    public void OnLaneD(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
        {
            if (judge != null)
            {
                judge.HandleInput(LaneId.D);
            }
        }
    }

    private void Update()
    {
        if (!isBattle)
        {
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
            transform.Translate(dir * 5f * Time.deltaTime, Space.World);
        }
    }
}
