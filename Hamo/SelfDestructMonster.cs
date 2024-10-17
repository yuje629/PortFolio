using UnityEngine;

public class SelfDestructMonster : Enemy, IDamageable
{
    public float                        moveSpeed;
    public GameObject                   bombPrefab;

    [SerializeField] private float      bombTime;
    [SerializeField] private float      damage;
    [SerializeField] private float      chaseDis;
    
    private float                       dirX;
    private float                       colorGB = 1f;
    private float                       turnDirDis = 3f;
    private float                       bombTimer;
    private float                       colorTime;
    private float                       colorTimer;
    private Rigidbody2D                 rigid;
    private Animator                    anim;
    private SpriteRenderer              spriteRenderer;
    [SerializeField] private EnemyState enemyState;
    private Color                       color = new Color(1,1,1,1);
    private int HP = 1;

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
        target = GetPlayer();
        colSize = GetComponent<BoxCollider2D>().size.x / 2f;
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    void OnEnable()
    {
        HP = 1;
        bombTimer = bombTime;
        colorTime = bombTimer / bombTime;
        colorTimer = colorTime;
        colorGB = 1f;
        color = new Color(1, 1, 1, 1);
        enemyState = EnemyState.Patrol;
        Think();
    }


    private void Update()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        if (dirX != 0)
            spriteRenderer.flipX = dirX < 0;

        switch (enemyState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (Vector2.Distance(transform.position, base.target.transform.position) <= chaseDis)
                    enemyState = EnemyState.Chase;
                if (ObstacleHitCheck(new Vector2(dirX, 0)))
                    Turn();
                    break;
            case EnemyState.Chase:
                CancelInvoke("Think");
                Chase();
                BombWarning();
                break;
            default:
                break;
        }
    }
    private void Patrol()
    {
        //움직임
        if(CheckAround())
        rigid.velocity = new Vector2(dirX * moveSpeed, rigid.velocity.y);

        //바닥체크
        Vector2 frontVec = new Vector2(rigid.position.x + dirX * 1.2f, rigid.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontVec, Vector2.down, 1, groundLayer);
        Debug.DrawRay(frontVec, Vector2.down, new Color(0, 1, 0));

        if (hit.collider == null)
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
        if (Mathf.Abs(base.target.position.x - transform.position.x) >= turnDirDis && CheckAround())
        {
            dirX = base.target.position.x - transform.position.x;
            dirX = (dirX < 0) ? -1 : 1;
        }

        rigid.velocity = new Vector2(dirX * moveSpeed, rigid.velocity.y);
    }

    private bool CheckAround()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - 0.5f), new Vector2(0.9f, 0.2f), 0f, base.groundLayer);
    }

    void BombWarning()
    {
        bombTimer -= Time.deltaTime;
        colorTimer -= Time.deltaTime;
        colorGB = Mathf.Lerp(1, 0, colorTimer / colorTime);
        color.g = colorGB;
        color.b = colorGB;
        spriteRenderer.color = color;

        if (colorTimer <= 0)
        {
            colorGB = 1;
            colorTime = bombTimer / bombTime;
            colorTimer = colorTime;
        }

        if (bombTimer <= 0)
        {
            soundManager.PlaySFX(soundManager.bomb);
            Instantiate(bombPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
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

    void OnDisable()
    {
        onEndHit();
    }

}
