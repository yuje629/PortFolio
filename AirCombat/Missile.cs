using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    Rigidbody rb;

    [Header("��ũ���ͺ� ������Ʈ")]
    public WeaponData missileinfo;

    [Header("�̻��Ͽ�����Ʈ")]
    public GameObject missileObj;
    public ParticleSystem[] particle;

    [Header("�ӵ�")]
    public float speed;         //�̻��� �ӵ�
    public float maxSpeed;      //�̻��� �ְ� �ӵ�
    public float accelAmount;   //�ð����� ��ŭ�� ���ӽ�ų��
    public float accelTime;
    bool addAccel;

    [Header("�����")]
    public float missileDamage; //�̻��� ������

    [Header("������Ÿ��")]
    public float lifetime;      //�̻��� �Ҹ������ �ð�

    [Header("����")]// 
    public Transform target;
    public float turningForce;//ȸ���ϴ� ��(�������� �� ������ ȸ��)
    public float targetSearchSpd;//Ÿ���� ã�� ���ǵ�
    public float lockOnDistance;//���� �Ÿ�
    public float boresightAngle;
    public float angle;
    public float distance;
    bool isDisabled = false;
    Rigidbody targetRigidbody = null;
    public bool isTarget;

    public bool isSpecialWeapon=false;

    [Header("���� ����Ʈ")]
    public GameObject explosionPrefab;

    [Header("�ݶ��̴� ")]
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

    private void OnEnable()     //�ʱ�ȭ
    {
        CancelInvoke("ActiveFalse");  
        CancelInvoke("TrueAddAccel");  

        ActivateObject();
        SetSpeed();  //�÷��̾� �ӵ� ����  
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

    public void SetSpeed()  //�÷��̾� �ӵ��� �޾ƿ�
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

    // �ݶ��̴��� ��Ȱ��ȭ�ϴ� �Լ�
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

        Quaternion lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);//ȸ������ ���
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);//turningForce�� ���� �ε巴�� ȸ���ϰ�
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
