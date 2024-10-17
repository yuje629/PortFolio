using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    Rigidbody erb;

    [Header("폭발이펙트")]
    public GameObject explosionEffect;

    [Header("목표지점")] //Waypoint 생성하는데 필요한 변수들
    [SerializeField] protected Vector3 currentWaypoint;  //현재의 목표 이동 지점, Enemy는 currentWaypoint로 이동한다.
    BoxCollider areaCollider; //박스 콜라이더로 waypoint 생성 위치 제한
    protected float height = 100f;  //땅과 높이차이가 별로 안 나거나 땅 아래에 포인트 지점이 생기면 더해줄 변수
    private bool isAvoid;

    [Header("이동")]     //Move에 필요한 변수
    public float speed;  //이동 속도
    public float rotateSpeed;
    public float defaulSpeed;
    [SerializeField] float avoidanceDis;  //적끼리, 지형에 부딪치는 것을 방지하기 위한 Waypoint생성 거리

    [Header("회전")]//Rotate에 필요한 변수들
    [SerializeField] protected float turningForce;  //회전하는 힘
    protected float turningTime;                    //회전하는 속도, 회전하는 힘에 반비례한다.

    [Header("Z축회전")] //z축 회전에 필요한 변수들
    [SerializeField] protected float zRotateMaxThreshold = 0.3f;   
    [SerializeField] protected float zRotateAmount = 135;
    protected float prevRotY;
    protected float currRotY;
    protected float rotateAmount;
    protected float zRotateValue;

    [Header("회피")]
    //회피기동에 확률을 적용하기 위한 변수
    [SerializeField] [Range(0, 1)] protected float evasionRate = 0.5f;

    [Header("추적")]
    //플레이어 추적하는데 필요한 변수들
    [SerializeField] [Range(0, 1)] protected float playerTrackingRate = 0.5f;   //플레이어를 추적할 확률
    [SerializeField] protected bool isTracking;           //플레이어 추적 상태
    //[SerializeField] protected float trackingTimer;       //플레이어 추적 시간
    //[SerializeField] protected float trackingTime;        //플레이어 추적 타이머
    [SerializeField] protected float newWaypointTime;     //!isTracking일 때 새로운 웨이포인트 생성 시간
    [SerializeField] protected float newWaypointTimer;    //새로운 웨이포인트 생성 타이머
    protected bool spawnWaypointEnabed = false;           //새로운 웨이포인트 생성 가능한지 체크하는 함수

    //플레이어 오브젝트
    protected PlayerCtrl player;
    
    void Start()
    {
        erb = GetComponent<Rigidbody>();

        areaCollider = GameObject.FindGameObjectWithTag("WaypointArea").GetComponent<BoxCollider>();

        player = GameManager.PlayerCtrl;

        turningTime = 1 / turningForce;  //회전시간을 회전힘에 반비례하게 하여 초기화

        isTracking = false;  //처음에는 플레이어 추적 상태가 아님

        CreateWaypoint();    //처음 waypoint 생성
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

    void HitCheck()//땅 아래에 포인트가 생기거나 땅과 높이 차이가 별로 안 날 때 높이 올려줌------//
    {
        RaycastHit hit;
        Physics.Raycast(currentWaypoint, Vector3.down, out hit);  //생성된 포인트에서 아래로 레이를 쏜다.

        if (hit.distance == 0) 
        {
            Physics.Raycast(currentWaypoint, Vector3.up, out hit);  //다시 레이를 위로 쏴서 
            currentWaypoint.y += height + hit.distance;  //땅과 높이 차이 만큼 더하고 minHeight 만큼 더해준다
        }

        if (hit.distance <= 50f)  //만약 쏴서 땅과의 거리가 50이하라면
        {
            currentWaypoint.y += height - hit.distance;  //minHieght만큼 더해준다.
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
        Vector3 targetDir = currentWaypoint - transform.position;          //타겟을 보는 방향
        Quaternion lookRotation = Quaternion.LookRotation(targetDir);      //타겟을 바라보기 위해 회전해야 하는 각도

        float delta = Quaternion.Angle(transform.rotation, lookRotation);  //기체 각도와 회전해야 하는 각도의 차이
        if (delta > 0f)
        {
            float lerpAmount = Mathf.SmoothDampAngle(delta, 0.0f, ref rotateAmount, turningTime);   //SmoothDampAngle: delta에서 0도로 부드럽게 이동하는데 delta가 360도가 가깝다면 360도로 회전하게 한다
            lerpAmount = 1.0f - (lerpAmount / delta);

            Vector3 eulerAngle = lookRotation.eulerAngles;
            eulerAngle.z += zRotateValue * zRotateAmount;
            lookRotation = Quaternion.Euler(eulerAngle);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lerpAmount);
        }
    }

    void ZAxisRotate() //z축 회전
    {
        currRotY = transform.eulerAngles.y;
        float diff = prevRotY - currRotY;  //이전 Y각도와 현재 Y각도의 차이

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

    bool CheckRotate()  //현재 적이 회전 중인지 여부(z축 회전크기로 회전 여부 결정)
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

        //적이 enemy, ground와 부딪치지 않게
        if (collision.transform.tag == "Enemy" || collision.transform.tag == "Ground")
        {
            isAvoid = true;

            Vector3 dir = transform.position - collision.transform.position;
            currentWaypoint = new Vector3(transform.position.x + (avoidanceDis * Mathf.Sign(dir.x)), 
                transform.position.y + (avoidanceDis * Mathf.Sign(dir.y)), 
                transform.position.z + (avoidanceDis * Mathf.Sign(dir.z)));  //waypoint를 부딪히려고 하는 물체 반대방향에 생성

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
