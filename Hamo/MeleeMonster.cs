using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Pool;

public enum EnemyState
{
    Patrol, Chase
}

public class MeleeMonster : Enemy, IDamageable
{
    [SerializeField] private float damage;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float chaseDis;
    [SerializeField] private float jumpPower;
    [SerializeField] private float patrolDis;
    [SerializeField] private float attackDis;
    private float dirX;
    private EnemyState enemyState;
    private Rigidbody2D rigid;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float jumpCoolTime = 1f;
    private float jumpCoolTimer = 0f;
    private bool isJumpEnabled = true;
    private float turnDirDis = 2f;
    int HP = 1;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.myTransform = GetComponent<Transform>();
    }

    private void Start()
    {
        slow = GameObject.FindWithTag("Player").GetComponent<SlowMotion>();
        target = base.GetPlayer();
        this.colSize = GetComponent<BoxCollider2D>().size.x / 2;
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    void OnEnable()
    {
        HP = 1;
        enemyState = EnemyState.Patrol;
        rigid.velocity = Vector3.zero;
        Think();
    }

    private void FixedUpdate()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        if (rigid.velocity.x != 0)
            spriteRenderer.flipX = rigid.velocity.x < 0;

        if (ObstacleHitCheck(new Vector2(dirX, 0)))
            dirX *= -1;

        if (!isJumpEnabled)
        {
            jumpCoolTimer += Time.fixedDeltaTime;

            if (jumpCoolTimer >= jumpCoolTime)
            {
                jumpCoolTimer = 0;
                isJumpEnabled = true;
            }
        }

        float dis = Vector2.Distance(target.transform.position, transform.position);

        switch (enemyState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (target != null)
                {
                    if (dis <= chaseDis)
                        enemyState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (target != null)
                {
                    CancelInvoke("Think");
                    Chase();
                    if (dis >= patrolDis)
                        enemyState = EnemyState.Patrol;
                    else if (Mathf.Abs(target.position.x - transform.position.x) <= 3f && target.position.y - transform.position.y >= 1f && isJumpEnabled)
                    {
                        OscillateJump();
                        isJumpEnabled = false;
                    }
                }
                break;
        }
    }

    private void Patrol()
    {
        //움직임
        if (CheckAround())
            rigid.velocity = new Vector2(dirX * moveSpeed, rigid.velocity.y);

        //바닥체크
        Vector2 frontVec = new Vector2(rigid.position.x + dirX * 1.2f, rigid.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontVec, Vector2.down, 1, base.groundLayer);
        Debug.DrawRay(frontVec, Vector2.down, new Color(0, 1, 0));

        if (hit.collider == null && CheckAround())
        {
            Turn();
            CancelInvoke("Think");
            Invoke("Think", 5);
        }
    }

    private void Think()
    {
        //랜덤 이동 방향
        dirX = Random.Range(-1, 2);

        //다음 행동 준비
        float nextThinkTime = Random.Range(2f, 3.5f);
        Invoke("Think", nextThinkTime);
    }

    private void Turn()
    {
        dirX *= -1;
    }

    private void Chase()
    {
        if (target != null)
        {
            anim.SetInteger("WalkSpeed", (int)dirX);

            if (CheckAround())
            {
                if (Mathf.Abs(base.target.position.x - transform.position.x) >= turnDirDis)
                {
                    dirX = target.transform.position.x - transform.position.x;
                    dirX = (dirX < 0) ? -1 : 1;
                }
            }

            rigid.velocity = new Vector2(dirX * moveSpeed, rigid.velocity.y);
        }
    }

    private void OscillateJump()
    {
        int rand = Random.Range(0, 10);
        if (CheckAround())
        {
            soundManager.PlaySFX(soundManager.meleeJump);
            rigid.velocity = new Vector2(dirX * moveSpeed, jumpPower);
        }
    }

    private bool CheckAround()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.5f), new Vector2(0.9f, 0.2f), 0f, base.groundLayer);
    }

    public void TakeDamage(float damage)
    {
        //데미지 입는 이벤트
        HP--;
        if (HP <= 0)
        {
            slow.StartSlowTrigger();
            Destroy(gameObject);
        }
        onEndHit();
    }

    void OnDisable()
    {
        onEndHit();
    }
}
