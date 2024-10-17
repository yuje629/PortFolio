using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    Rigidbody rb;

    [Header("스크립터블 오브젝트")]
    public WeaponData missileinfo;

    [Header("미사일오브젝트")]
    public GameObject missileObj;
    public ParticleSystem[] particle;

    [Header("속도")]
    public float speed;         //미사일 속도
    public float maxSpeed;      //미사일 최고 속도
    public float accelAmount;   //시간마다 얼만큼씩 가속시킬지
    public float accelTime;
    bool addAccel;

    [Header("대미지")]
    public float missileDamage; //미사일 데미지

    [Header("라이프타임")]
    public float lifetime;      //미사일 소멸까지의 시간

    [Header("락온")]// 
    public Transform target;
    public float turningForce;//회전하는 힘(높을수록 더 빠르게 회전)
    public float targetSearchSpd;//타겟을 찾는 스피드
    public float lockOnDistance;//락온 거리
    public float boresightAngle;
    public float angle;
    public float distance;
    bool isDisabled = false;
    Rigidbody targetRigidbody = null;
    public bool isTarget;

    public bool isSpecialWeapon=false;

    [Header("폭발 이펙트")]
    public GameObject explosionPrefab;

    [Header("콜라이더 ")]
    private Collider missileColl;
    public float colliderDis;

    private void Start()
    {
        if (missileColl != null)
        {
            missileColl.enabled = false;
        }
        
        if(target != null)
        distance = Vector3.Distance(target.position, this.transform.position);
        
        if(distance<1000)
        {
            isTarget = true; 
        }

        missileDamage = missileinfo.damages[0];

        missileColl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()     //초기화
    {
        CancelInvoke("ActiveFalse");  
        CancelInvoke("TrueAddAccel");  

        ActivateObject();
        SetSpeed();  //플레이어 속도 적용  
        addAccel = false;

        Invoke("ActiveFalse", lifetime);  
        Invoke("TrueAddAccel", accelTime);   
    }

    void FixedUpdate()
    {
        Fire(); 

        if (addAccel)
        {
            AddAcceleration();
        }

        if (target != null)
        {
            Vector3 targetDir = target.position - transform.position;
            angle = Vector3.Angle(targetDir, transform.forward);
        }

        if (angle > boresightAngle)
        {
            target = null;
            return;
        }

        if(isTarget==true)
        { 
            LookAtTarget();
        }
    }

    public void SetSpeed()  //플레이어 속도를 받아옴
    {
        float playerSpeed = GameManager.PlayerCtrl.speed ;   
        speed = playerSpeed;  /
    }

    private void Fire()
    {
        rb.velocity = transform.forward * speed;
    }

    void TrueAddAccel()
    {
        addAccel = true;
    }

    void AddAcceleration()
    {
        if (speed < maxSpeed)  // 
        {
            speed += accelAmount * Time.fixedDeltaTime;  
        }
    }

    public void EnableCollider()
    {
        if (missileColl != null)
        {
            missileColl.enabled = true;
        }
    }

    // 콜라이더를 비활성화하는 함수
    public void DisableCollider()
    {
        if (missileColl != null)
        {
            missileColl.enabled = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "EnemyHP" || collision.transform.tag == "Ground")
        {
            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    GameObject explosion = PoolManager.instance.Get(index);
                    explosion.transform.parent = PoolManager.instance.transform.GetChild(index);
                    explosion.transform.position = transform.position;
                    explosion.transform.rotation = transform.rotation;
                    break;
                }
            }

            DisableObject();
        }

        if(collision.transform.tag == "EnemyHP")
        {
            DisableObject();
            collision.transform.GetComponent<EnemyHPManager>().EnemyTakeDamage(missileDamage);
        }
    }

    public void LookAtTarget() 
    {
        if (target == null)
        {
            transform.Translate(Vector3.forward, Space.Self);
            return;
        }
        Vector3 targetDir = target.position - transform.position;
        float distance = Vector3.Distance(target.position, transform.position); 
        angle = Vector3.Angle(targetDir, transform.forward);

        if(angle>boresightAngle)
        {
            target = null;
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);//회전방향 계산
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);//turningForce에 따라 부드럽게 회전하게
    }

    void ActivateObject()
    {
        for (int index = 0; index < particle.Length; index++)
        {
            particle[index].Play();
        }

        missileObj.gameObject.SetActive(true);
    }

    void DisableObject()
    {
        for (int index = 0; index < particle.Length; index++)
        {
            particle[index].Stop();
        }

        missileObj.gameObject.SetActive(false);
    }

    void ActiveFalse()  // 
    {
        gameObject.SetActive(false);
    }
}
