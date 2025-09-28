using UnityEngine;

public enum PlayerState
{
    Idle,
    Move
}
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public Transform gunRoot;
    public Vector2 moveRangeX;
    public Vector2 moveRangeY;
    public float moveSpeed;
    public float AttackCD { get => Mathf.Clamp(1f / lv * 1.5f, 0.1f, 1f); }
    public float spawnMonsterIntervalMultiply = 1f;
    public float spawnMonsterQuantityMultiply = 1f;
    public int lv = 1;
    public int BulletQuantity { get => lv; }
    public int LV
    {
        get => lv;
        set => lv = value;
    }
    private PlayerState playerState;
    private PlayerState PlayerState
    {
        get => playerState;
        set
        {
            if (playerState != value)
            {
                playerState = value;
                switch (playerState)
                {
                    case PlayerState.Idle:
                        {
                            PlayAnimation("Idle");
                        }
                        break;
                    case PlayerState.Move:
                        {
                            PlayAnimation("Move");
                        }
                        break;
                }
            }
        }
    }
    private void Awake()
    {
        LV = lv;
    }
    private void Start()
    {
        PlayerState = PlayerState.Idle;
    }
    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        switch (playerState)
        {
            case PlayerState.Idle:
                {
                    if (h != 0 || v != 0) PlayerState = PlayerState.Move;
                }
                break;
            case PlayerState.Move:
                {
                    if (h == 0 && v == 0)
                    {
                        PlayerState = PlayerState.Idle;
                        return;
                    }
                    transform.Translate(moveSpeed * Time.deltaTime * new Vector3(h, v, 0));
                    CheckPositionRange();
                    if (h > 0) transform.localScale = new Vector3(1, 1, 1);
                    else if (h < 0) transform.localScale = new Vector3(-1, 1, 1);
                }
                break;
        }
    }
    private void CheckPositionRange()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, moveRangeX.x, moveRangeX.y);
        pos.y = Mathf.Clamp(pos.y, moveRangeY.x, moveRangeY.y);
        pos.z = 0;
        transform.position = pos;
    }
    public void PlayAnimation(string animationName)
    {
        animator.CrossFadeInFixedTime(animationName, 0.0f);
    }
}