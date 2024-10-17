using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyPatrolMonster : Enemy,IDamageable
{
    public enum DirectionType
    {
        Horizontal, Vertical
    }

    public enum TurnType
    {
        Hit,                //장애물에 부딪히면 방향 전환
        Reaching            //정해진 거리 이상 가야 방향 전환
    }

    public float                            moveSpeed;
    public float                            turnDistance;

    public DirectionType                    directionType;
    public TurnType                         turnType;

    [SerializeField] private float          damage;
    private Vector3                         moveDirection;
    private Vector3                         originPos;
    private float                           moveAmount = 1f;
    private BoxCollider2D                   collider2D;
    private Animator                        anim;
    private SpriteRenderer                  spriteRenderer;
    private Rigidbody2D                     rigid;
    private int HP = 1;

    private void Awake()
    {
        collider2D = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        myTransform = GetComponent<Transform>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    private void OnEnable()
    {
        slow = GameObject.FindWithTag("Player").GetComponent<SlowMotion>();
        switch (directionType)
        {
            case DirectionType.Horizontal:
                this.colSize = collider2D.size.x / 2;
                break;
            case DirectionType.Vertical:
                this.colSize = collider2D.size.y / 2;
                break;
            default:
                Debug.Log("directionType을 정해주세요.");
                break;
        }

        originPos = transform.position;

        Invoke("ChangeAmount", 2f);
        if (turnType == TurnType.Hit)
        {
            turnDistance = float.MaxValue;
            CancelInvoke("ChangeAmount");
        }

        HP = 1;
    }


    void Update()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        switch (directionType)
        {
            case DirectionType.Horizontal:
                moveDirection = new Vector3(moveAmount, 0, 0);
                spriteRenderer.flipX = moveAmount < 0;
                break;
            case DirectionType.Vertical:
                moveDirection = new Vector3(0, moveAmount, 0);
                break;
            default:
                Debug.Log("directionType을 정해주세요.");
                break;
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        if (ObstacleHitCheck(moveDirection))
        {
            moveAmount *= -1;
        }

    }

    public void TakeDamage(float damage)
    {
        HP--;
        if (HP <= 0)
        {
            slow.StartSlowTrigger();
            Destroy(gameObject);
        }
        onEndHit();
    }

    void ChangeAmount()
    {
        moveAmount *= -1;
        Invoke("ChangeAmount", 3f);
    }

    void OnDisable()
    {
        onEndHit();
    }
}
