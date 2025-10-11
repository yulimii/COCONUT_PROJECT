using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IBattleActions
{
    public BeatJudgeSystem judge;   // 배틀 판정기
    private InputSystem_Actions input;
    private Vector2 moveInput;
    private bool isBattle = false;

    void Awake()
    {
        input = new InputSystem_Actions();
        input.Player.SetCallbacks(this);
        input.Battle.SetCallbacks(this);
    }

    void OnEnable()  => EnableCurrentMap();
    void OnDisable() => input.Disable();

    // 상태 전환
    public void SwitchToBattle()
    {
        isBattle = true;
        EnableCurrentMap();
    }
    public void SwitchToLevel()
    {
        isBattle = false;
        EnableCurrentMap();
    }
    private void EnableCurrentMap()
    {
        input.Disable();
        if (isBattle)
            input.Battle.Enable();
        else
            input.Player.Enable();
            
        input.UI.Enable(); 
    }

    // ── Player Action ──
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!isBattle) 
            moveInput = ctx.ReadValue<Vector2>();
    }
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!isBattle && ctx.performed)
            Debug.Log("Interact!");
    }
    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) 
            Debug.Log("Cancel pressed");
    }

    // ── Battle Action ──
    public void OnLaneA(InputAction.CallbackContext ctx)
    { 
        if (isBattle && ctx.performed) 
            judge?.HandleInput(LaneId.A); 
    }
    public void OnLaneB(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
            judge?.HandleInput(LaneId.B); 
    }
    public void OnLaneC(InputAction.CallbackContext ctx)
    {
        if (isBattle && ctx.performed)
            judge?.HandleInput(LaneId.C); 
    }
    public void OnLaneD(InputAction.CallbackContext ctx)
    { 
        if (isBattle && ctx.performed) 
            judge?.HandleInput(LaneId.D); 
    }

    void Update()
    {
        if (!isBattle)
        {
            Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y);
            transform.Translate(dir * 5f * Time.deltaTime);
        }
    }
}
