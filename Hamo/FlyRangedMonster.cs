using MovementAlgorithm;
using DataStructure;
using UnityEngine;
using UnityEngine.AI;

public class FlyRangedMonster : Enemy, IDetectable, IDamageable
{
    public Transform                    endLinePoint;

    [SerializeField] protected float    chaseDis;
    [SerializeField] private float      attackDis;
    [SerializeField] private float      stopDis;
    [SerializeField] private GameObject laser;
    private Color                       lineColor   = new Color(0.5f, 0, 255 / 255, 0.5f);
    private float                       lineColorB  = 255f / 255f;
    private Vector2                     m_direction;
    private bool                        isDetect;
    private float                       attackPeriod = 8;
    private float                       colorAmount;
    private float                       widthAmount;
    private LineRenderer                lineRenderer;
    private Animator                    anim;
    private SpriteRenderer              spriteRenderer;
    private float                       lineWidth = 1f;
    private NavMeshAgent                agent;
    private Rigidbody2D                 rigid;
    private int HP = 1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody2D>();
        colorAmount = Time.deltaTime * (lineColorB / attackPeriod);
        widthAmount = Time.deltaTime * (lineWidth / attackPeriod);
    }

    void Start()
    {
        target = GetPlayer();
        slow = GameObject.FindWithTag("Player").GetComponent<SlowMotion>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    void OnEnable()
    {
        lineColor = new Color(0.5f, 0, 255 / 255, 0.5f);
        lineColorB = 255f / 255f;
        attackPeriod = 8;
        lineWidth = 1f;
        HP = 1;
    }

    void FixedUpdate()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            agent.isStopped = true;
            return;
        }

        if (target != null)
        {
            float dis = Vector2.Distance(target.position, transform.position);
            m_direction = (target.position - transform.position).normalized;


            if (isDetect && dis <= chaseDis)
            {
                ShootLaser();

                if (dis > stopDis)
                {
                    Chase();
                }
                else if(dis <= stopDis)
                {
                    spriteRenderer.flipX = (target.position.x - transform.position.x) < 0;
                }
            }
            else
            {
                Patrol();
            }
        }
    }
    public void OnDetect()
    {
        isDetect = true;
        laser.SetActive(true);
    }

    private void Patrol()
    {
        //정찰
        anim.SetTrigger("Idle");
        laser.SetActive(false);
        agent.isStopped = true;
        lineRenderer.enabled = false;
    }

    void ShootLaser()
    {
        lineRenderer.enabled = true;

        Vector3 dir = (endLinePoint.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 100f);
        if (hit.collider == null)
            Draw2Dray(transform.position, transform.position + dir * 100f);
        else
            Draw2Dray(transform.position, hit.point);

        lineColor.b = lineColorB;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineColorB -= colorAmount;
        lineWidth -= widthAmount;

        if (lineColorB <= 0)
        {
            Attack();
            lineColorB = 255 / 255f;
            lineWidth = 1f;
        }
    }

    private void Attack()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, endLinePoint.position - transform.position, 100f);

        if (hit.collider != null && hit.collider.tag == "Player")
            if (target != null)
                target.GetComponent<Player>().TakeDamage(transform.right);
    }

    private void Draw2Dray(Vector2 start, Vector2 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void Chase()
    {
        laser.SetActive(true);
        agent.isStopped = false;

        //이동 애니메이션
        anim.SetTrigger("Chase");
        spriteRenderer.flipX = agent.desiredVelocity.x < 0;

        agent.SetDestination(target.position);
    }

    //장애물 체크
    private bool ObstacleHitCheck()
    {
        if (target != null)
        {
            float dis = Vector2.Distance(target.position, transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, m_direction, dis);

            Debug.DrawRay(transform.position, m_direction * dis);

            if (hit.collider != null)
            {
                if (hit.collider.tag == "Player" || hit.collider.tag == "bullet")
                    return false;
                return true;
            }
        }
        return false;
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
