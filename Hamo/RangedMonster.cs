using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RangedMonster : Enemy, IDamageable
{
    [SerializeField] private GameObject gun;            //적 무기
    [SerializeField] private GameObject bullet;         //적 총알
    [SerializeField] private Transform  firePos;        //발사 위치
    [SerializeField] private float      attackDisX;     //X축 공격범위
    [SerializeField] private float      attackDisY;     //Y축 공격범위(바닥쪽은 공격X)
    private float                       disX;
    private float                       disY;
    private Animator                    anim;
    private Rigidbody2D                 rigid;
    private int                         HP = 1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        target = GetPlayer();
        slow = GameObject.FindWithTag("Player").GetComponent<SlowMotion>();
        soundManager = GameObject.FindWithTag("Sound").GetComponent<SoundManager>();
    }

    void OnEnable()
    {
        HP = 1;
    }

    private void Update()
    {
        CheckGameEnd();

        if (isHooked)
        {
            rigid.velocity = Vector2.zero;
            return;
        }

        if (target != null)
        {
            disX = target.position.x - transform.position.x;
            disY = target.position.y - transform.position.y;

            if (Mathf.Abs(disX) <= attackDisX && disY <= attackDisY && disY >= -0.4f)
            {
                anim.SetBool("IsAttack", true);
                GunRotate();
            }
            else
                anim.SetBool("IsAttack", false);

            transform.localScale = new Vector3(disX < 0 ? -1.2f : 1.2f, 1.2f, 1);
            
        }
    }

    private void ShootBullet()
    {
        float angle = GetAngle(base.target.position, transform.position);
        Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        GameObject temp = PoolManager.instance.GetGo("RangedBullet");
        temp.transform.position = firePos.position;
        temp.transform.rotation = angleAxis;
        SoundManager.instance.PlaySFX(SoundManager.instance.monsterAttack);
    }

    private void GunRotate()
    {
        float angle = GetAngle(base.target.position, transform.position);
        gun.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private float GetAngle(Vector3 start, Vector3 end)
    {
        Vector2 newPos = start - end;
        float angle = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;

        return angle;
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
