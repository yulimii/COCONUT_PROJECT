using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVector;
    public float speed = 5f;
    public Rigidbody2D rigid;
    public Animator animator;
    private Vector2 _moveDir;

    [Header("Dialogue System")]
    [SerializeField] private DialogueView dialogueView;

    // 게임 오브젝트가 메모리에 올라올 때 최초 1회 호출됩니다. (= 생성자)
    private void Awake()
    {
        //rigid = GetComponent<Rigidbody2D>();
        //만약 같은 오브젝트에 Animator 컴포넌트가 붙어있다면 가져온다
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // DialogueView 자동 찾기 (Inspector에서 할당하지 않은 경우)
        if (dialogueView == null)
        {
            dialogueView = FindFirstObjectByType<DialogueView>();
            if (dialogueView == null)
            {
                Debug.LogWarning("DialogueView not found! Player movement won't be blocked during dialogue.");
            }
        }
    }

    // 매프레임마다 한번씩 호출 되는 메서드
    public void Update()
    {
        // 대화 중일 때는 이동 불가
        if (IsDialogueActive())
        {
            // 대화 중에는 애니메이션을 정지 상태로 설정
            animator.SetBool("isMove", false);
            return; // 이동 처리 건너뛰기
        }

        // 입력 감지
        Vector2 _moveDir = Vector2.zero; //(0,0);

        if (Input.GetKey(KeyCode.DownArrow))
            _moveDir = Vector2.down; //(0,-1)
        else if (Input.GetKey(KeyCode.UpArrow))
            _moveDir = Vector2.up; //(0,1)
        else if (Input.GetKey(KeyCode.LeftArrow))
            _moveDir = Vector2.left; //(-1,0)
        else if (Input.GetKey(KeyCode.RightArrow))
            _moveDir = Vector2.right; //(1,0)

        // 이동 처리 (0,0)
        if (_moveDir != Vector2.zero)
        {
            animator.SetBool("isMove", true);
            transform.Translate(_moveDir * (speed * Time.deltaTime));

            // 애니메이터 파라미터에 현재 방향값을 전달
            //float 실수형 자료형이라는 뜻이에여.
            animator.SetFloat("xDir", _moveDir.x);
            animator.SetFloat("yDir", _moveDir.y);
        }
        else
        {
            animator.SetBool("isMove", false);
        }
    }

    // === 대화 상태 확인 메서드 ===
    private bool IsDialogueActive()
    {
        return dialogueView != null && dialogueView.IsDialogueActive;
    }
}