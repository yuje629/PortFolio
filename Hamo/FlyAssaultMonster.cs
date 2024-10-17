using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FlyAssaultMonster : Enemy,IDamageable
{
    private enum FlyAssaultState
    {
        Patrol, Chase
    }

    [SerializeField] private float      assaultSpeed;
    [SerializeField] private float      chaseDis = 20;
    [SerializeField] private float      assaultDis = 5;
    private float                       assaultDuration = 5f;
    private float                       assaultDuTimer = 0f;
    private float                       attackDelayTimer = 0f;
    private float                       attackDelayTime = 3f;
    private bool                        attackenabled = true;
    private bool                        isAssault;
    private Rigidbody2D                 rigid;
    private Animator                    anim;
    private SpriteRenderer              spriteRenderer;
    private FlyAssaultState             enemyState;
    private Vector2                     direction;
    private NavMeshAgent                agent;
    private float                       assaultTime = 0.1f;
    private int HP = 1;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        myTransform = transform;
    }

    private void Start()
    {
        target = GetPlayer();
        slow = GameObject.FindWithTag("Player").GetComponent<SlowMotion>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    void OnEnable()
    {
        enemyState = FlyAssaultState.Patrol;
        assaultDuTimer = 0;
        HP = 1;
    }

    private void FixedUpdate()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            if (agent != null)
                agent.isStopped = true;
            return;
        }

        if (!attackenabled)
        {
            attackDelayTimer += Time.fixedDeltaTime;

            if (attackDelayTimer >= attackDelayTime)
            {
                attackDelayTimer = 0;
                attackenabled = true;
            }
        }

        switch (enemyState)
        {
            case FlyAssaultState.Patrol:
                if (Vector2.Distance(myTransform.position, target.position) <= chaseDis)
                    enemyState = FlyAssaultState.Chase;
                else
                    Patrol();
                break;
            case FlyAssaultState.Chase:
                if (Vector2.Distance(myTransform.position, target.position) > chaseDis)
                    enemyState = FlyAssaultState.Patrol;
                else
                {
                    Chase();
                    assaultDuTimer += Time.deltaTime;

                    if (assaultDuTimer >= assaultDuration)
                    {
                        assaultDuTimer = 0;
                        if (!isAssault && Vector2.Distance(myTransform.position, target.position) >= assaultDis)
                            StartCoroutine(Assault());
                    }
                }
                break;
            default:
                break;
        }
    }

    private void Patrol()
    {
        if(agent != null)
        agent.isStopped = true;
    }

    private void Chase()
    {
        spriteRenderer.flipX = agent.desiredVelocity.x < 0;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    IEnumerator Assault()
    {
        isAssault = true;

        direction = (target.position - transform.position).normalized;
        rigid.velocity = direction * assaultSpeed;

        yield return YieldCache.WaitForSeconds(assaultTime);

        rigid.velocity = Vector2.zero;
        isAssault = false;
    }

    private void Attack()
    {
        spriteRenderer.flipX = target.position.x - transform.position.x < 0;
        anim.SetTrigger("doAttack");
        soundManager.PlaySFX(soundManager.monsterAttack);
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            if (attackenabled)
            {
                attackenabled = false;
                Attack();
            }
        }
    }

    void OnDisable()
    {
        onEndHit();
    }
}
