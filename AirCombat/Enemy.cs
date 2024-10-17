using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    Rigidbody erb;

    [Header("��������Ʈ")]
    public GameObject explosionEffect;

    [Header("��ǥ����")] //Waypoint �����ϴµ� �ʿ��� ������
    [SerializeField] protected Vector3 currentWaypoint;  //������ ��ǥ �̵� ����, Enemy�� currentWaypoint�� �̵��Ѵ�.
    BoxCollider areaCollider; //�ڽ� �ݶ��̴��� waypoint ���� ��ġ ����
    protected float height = 100f;  //���� �������̰� ���� �� ���ų� �� �Ʒ��� ����Ʈ ������ ����� ������ ����
    private bool isAvoid;

    [Header("�̵�")]     //Move�� �ʿ��� ����
    public float speed;  //�̵� �ӵ�
    public float rotateSpeed;
    public float defaulSpeed;
    [SerializeField] float avoidanceDis;  //������, ������ �ε�ġ�� ���� �����ϱ� ���� Waypoint���� �Ÿ�

    [Header("ȸ��")]//Rotate�� �ʿ��� ������
    [SerializeField] protected float turningForce;  //ȸ���ϴ� ��
    protected float turningTime;                    //ȸ���ϴ� �ӵ�, ȸ���ϴ� ���� �ݺ���Ѵ�.

    [Header("Z��ȸ��")] //z�� ȸ���� �ʿ��� ������
    [SerializeField] protected float zRotateMaxThreshold = 0.3f;   
    [SerializeField] protected float zRotateAmount = 135;
    protected float prevRotY;
    protected float currRotY;
    protected float rotateAmount;
    protected float zRotateValue;

    [Header("ȸ��")]
    //ȸ�Ǳ⵿�� Ȯ���� �����ϱ� ���� ����
    [SerializeField] [Range(0, 1)] protected float evasionRate = 0.5f;

    [Header("����")]
    //�÷��̾� �����ϴµ� �ʿ��� ������
    [SerializeField] [Range(0, 1)] protected float playerTrackingRate = 0.5f;   //�÷��̾ ������ Ȯ��
    [SerializeField] protected bool isTracking;           //�÷��̾� ���� ����
    //[SerializeField] protected float trackingTimer;       //�÷��̾� ���� �ð�
    //[SerializeField] protected float trackingTime;        //�÷��̾� ���� Ÿ�̸�
    [SerializeField] protected float newWaypointTime;     //!isTracking�� �� ���ο� ��������Ʈ ���� �ð�
    [SerializeField] protected float newWaypointTimer;    //���ο� ��������Ʈ ���� Ÿ�̸�
    protected bool spawnWaypointEnabed = false;           //���ο� ��������Ʈ ���� �������� üũ�ϴ� �Լ�

    //�÷��̾� ������Ʈ
    protected PlayerCtrl player;
    
    void Start()
    {
        erb = GetComponent<Rigidbody>();

        areaCollider = GameObject.FindGameObjectWithTag("WaypointArea").GetComponent<BoxCollider>();

        player = GameManager.PlayerCtrl;

        turningTime = 1 / turningForce;  //ȸ���ð��� ȸ������ �ݺ���ϰ� �Ͽ� �ʱ�ȭ

        isTracking = false;  //ó������ �÷��̾� ���� ���°� �ƴ�

        CreateWaypoint();    //ó�� waypoint ����
    }

    void FixedUpdate()
    {
            CheckChangeWayPointTime();
            CheckWayPoint();

            ChangeSpeed();

            EnemyMove();
            EnemyRotate();
            ZAxisRotate();
    }

    void CheckChangeWayPointTime()
    {
        if(!isTracking && !isAvoid)
        {
            newWaypointTimer += Time.fixedDeltaTime;

            if(newWaypointTimer >= newWaypointTime && !isAvoid)
            {
                newWaypointTimer = 0;
                spawnWaypointEnabed = true;
            }
        }
    }

    void CheckWayPoint()
    {
        if (isTracking && !isAvoid && player != null)
        {
            currentWaypoint = player.transform.position;

            if(Vector3.Distance(transform.position, currentWaypoint) <= 140f)
            {
                CreateWaypoint();
            }
        }

        if (Vector3.Distance(transform.position, currentWaypoint) <= 10f || spawnWaypointEnabed)
        {
            CreateWaypoint();
            isAvoid = false;
        }
    }


    void CreateWaypoint()
    {
        spawnWaypointEnabed = false;

        float rate = Random.Range(0.0f, 1.0f);

        if (rate < playerTrackingRate)
        {
            isTracking = true;
        }
        else
        {
            isTracking = false;

            currentWaypoint = RandomPointInBounds(areaCollider.bounds);

            HitCheck();
        }
    }

    void HitCheck()//�� �Ʒ��� ����Ʈ�� ����ų� ���� ���� ���̰� ���� �� �� �� ���� �÷���------//
    {
        RaycastHit hit;
        Physics.Raycast(currentWaypoint, Vector3.down, out hit);  //������ ����Ʈ���� �Ʒ��� ���̸� ���.

        if (hit.distance == 0) 
        {
            Physics.Raycast(currentWaypoint, Vector3.up, out hit);  //�ٽ� ���̸� ���� ���� 
            currentWaypoint.y += height + hit.distance;  //���� ���� ���� ��ŭ ���ϰ� minHeight ��ŭ �����ش�
        }

        if (hit.distance <= 50f)  //���� ���� ������ �Ÿ��� 50���϶��
        {
            currentWaypoint.y += height - hit.distance;  //minHieght��ŭ �����ش�.
        }
    }

    Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    void EnemyMove()
    {
        erb.velocity = transform.forward * speed;
    }

    void EnemyRotate()
    {
        Vector3 targetDir = currentWaypoint - transform.position;          //Ÿ���� ���� ����
        Quaternion lookRotation = Quaternion.LookRotation(targetDir);      //Ÿ���� �ٶ󺸱� ���� ȸ���ؾ� �ϴ� ����

        float delta = Quaternion.Angle(transform.rotation, lookRotation);  //��ü ������ ȸ���ؾ� �ϴ� ������ ����
        if (delta > 0f)
        {
            float lerpAmount = Mathf.SmoothDampAngle(delta, 0.0f, ref rotateAmount, turningTime);   //SmoothDampAngle: delta���� 0���� �ε巴�� �̵��ϴµ� delta�� 360���� �����ٸ� 360���� ȸ���ϰ� �Ѵ�
            lerpAmount = 1.0f - (lerpAmount / delta);

            Vector3 eulerAngle = lookRotation.eulerAngles;
            eulerAngle.z += zRotateValue * zRotateAmount;
            lookRotation = Quaternion.Euler(eulerAngle);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lerpAmount);
        }
    }

    void ZAxisRotate() //z�� ȸ��
    {
        currRotY = transform.eulerAngles.y;
        float diff = prevRotY - currRotY;  //���� Y������ ���� Y������ ����

        if (diff > 180) diff -= 360;
        if (diff < -180) diff += 360;

        prevRotY = transform.eulerAngles.y;
        zRotateValue = Mathf.Lerp(zRotateValue, Mathf.Clamp(diff / zRotateMaxThreshold, -1, 1), turningForce * Time.fixedDeltaTime);
    }

    void ChangeSpeed()
    {
        if (CheckRotate())
        {
            speed = Mathf.Lerp(speed, rotateSpeed, 5 * Time.fixedDeltaTime);
        }
        else
            speed = Mathf.Lerp(speed, defaulSpeed, 5 * Time.fixedDeltaTime);
    }

    bool CheckRotate()  //���� ���� ȸ�� ������ ����(z�� ȸ��ũ��� ȸ�� ���� ����)
    {
        if (Mathf.Abs(transform.rotation.eulerAngles.z) >= 15f)
            return true;
        else
            return false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "PlayerMissile")
        {
            OnWarning();
        }

        //���� enemy, ground�� �ε�ġ�� �ʰ�
        if (collision.transform.tag == "Enemy" || collision.transform.tag == "Ground")
        {
            isAvoid = true;

            Vector3 dir = transform.position - collision.transform.position;
            currentWaypoint = new Vector3(transform.position.x + (avoidanceDis * Mathf.Sign(dir.x)), 
                transform.position.y + (avoidanceDis * Mathf.Sign(dir.y)), 
                transform.position.z + (avoidanceDis * Mathf.Sign(dir.z)));  //waypoint�� �ε������� �ϴ� ��ü �ݴ���⿡ ����

            HitCheck();
        }
    }

    void OnWarning()
    {
        float rate = Random.Range(0.0f, 1.0f);
        if (rate <= evasionRate)
        {
            CreateWaypoint();
        }
    }
}
