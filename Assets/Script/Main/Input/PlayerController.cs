using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public BeatJudgeSystem judge;

    public void OnLaneA(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) judge.HandleInput(LaneId.A);
    }
    public void OnLaneB(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) judge.HandleInput(LaneId.B);
    }
    public void OnLaneC(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) judge.HandleInput(LaneId.C);
    }
    public void OnLaneD(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) judge.HandleInput(LaneId.D);
    }
}