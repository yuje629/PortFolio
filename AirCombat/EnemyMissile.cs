using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    enum EnemyMissileState
    {
        LockOn,
        Straight,
    }

    Rigidbody emrb;

    [Header("�̻��Ͽ�����Ʈ")]
    public GameObject missileObj;
    public ParticleSystem[] particle;

    [Header("�̻��� �ӵ�")]
    [SerializeField] float speed;
    [SerializeField] float maxSpeed;
    [SerializeField] float accelAmount;
    [SerializeField] float accelerationDelay;   //�ӵ� �����ϴµ� �ɸ��� �ð�
    private bool addAccel;   //�ӵ� ���� ����

    [Header("�̻��� ����")]
    [SerializeField] float damage;
    [SerializeField] float boresightAngle;  //���� ������ ����
    public bool isPlayerWarning;            //�� �̻����� Ÿ���� �÷��̾��� ��

    Transform target;
    public float turningForce;

    [SerializeField] EnemyMissileState eMissileState;

    [Header("������")]
    [SerializeField] GameObject playerFlarePrefab;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject TimeLine;

    [Header("�̻��� ��Ȱ��ȭ")]
    [SerializeField] float deactivationDelay;

    void Start()
    {
        emrb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        isPlayerWarning = true;

        eMissileState = EnemyMissileState.LockOn;

        target = GameManager.PlayerCtrl.transform;

        addAccel = false;

        CancelInvoke("ActiveFalse");   
        CancelInvoke("TrueAddAccel");   

        ActivateObject();

        Invoke("ActiveFalse", deactivationDelay);   
        Invoke("TrueAddAccel", accelerationDelay);   
    }

    void FixedUpdate()
    {
        switch (eMissileState)
        {
            case EnemyMissileState.LockOn:
                {
                    Fire();          //�̻��� ����
                    LookAtTarget();  //�̻����� �÷��̾ ���� ȸ��
                    if (addAccel)
                    {
                        AddAcceleration();
                    }
                    break;
                }
        }
    }

    private void Fire()
    {
        emrb.velocity = transform.forward * speed;
    }

    void TrueAddAccel()
    {
        addAccel = true;
    }

    void AddAcceleration()
    {
        if (speed < maxSpeed)
        {
            speed += accelAmount * Time.deltaTime;
        }
    }

    public void EnemyMissileSetSpeed(float enemySpeed)
    {
        speed = enemySpeed;
    }

    void LookAtTarget()
    {
        FlareCheck();

        if (target == null)
        {
            isPlayerWarning = false;
            return;
        }

        Vector3 targetDir = target.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        if (angle > boresightAngle)
        {
            target = null;
            return;
        }

        isPlayerWarning = true;
        Quaternion lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);//ȸ������ ���
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.fixedDeltaTime);//turningForce�� ���� �ε巴�� ȸ��
        
    }

    //�÷��̾ �÷�� ����ߴ��� üũ�Ѵ�.
    void FlareCheck()
    {
        Vector3 flareDir;
        float angle;

        for(int index = 0; index < PoolManager.instance.prefabs.Length; index++)
        {
            if(PoolManager.instance.prefabs[index] == playerFlarePrefab)
            {
                foreach(var flareObj in PoolManager.instance.pools[index])
                {
                    if (flareObj.gameObject.activeSelf == true)
                    {
                        flareDir = flareObj.transform.position - transform.position;
                        angle = Vector3.Angle(flareDir, transform.forward);

                        if (angle < boresightAngle)
                        {
                            target = flareObj.transform;
                            isPlayerWarning = false;
                        }
                        break;
                    }
                }
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (TimeLine != null && TimeLine.activeSelf == true)
            {
                other.transform.GetComponent<PlayerCtrl>().TakeDamage(10);

            }
            else if (TimeLine == null)
            {
                other.transform.GetComponent<PlayerCtrl>().TakeDamage(damage);
            }
            DisableObject();
        }

        if (other.CompareTag("Ground"))
        {
            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    //  ��ü �ı� ��ƼŬ
                    GameObject explosion = PoolManager.instance.Get(index);
                    explosion.transform.parent = PoolManager.instance.transform.GetChild(index);
                    explosion.transform.position = transform.position;
                    explosion.transform.rotation = transform.rotation;
                    break;
                }
            }
            DisableObject();
        }
    }

    void ActivateObject()
    {
        isPlayerWarning = true;

        for (int index = 0; index < particle.Length; index++)
        {
            particle[index].Play();
        }

        missileObj.gameObject.SetActive(true);
    }

    void DisableObject()
    {
        isPlayerWarning = false;
        for (int index = 0; index < particle.Length; index++)
        {
            particle[index].Stop();
        }

        missileObj.gameObject.SetActive(false);
    }

    void ActiveFalse()
    {
        isPlayerWarning = false;
        gameObject.SetActive(false);
    }
}
