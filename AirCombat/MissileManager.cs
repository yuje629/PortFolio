using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissileManager : MonoBehaviour
{
    /// <summary>
    /// 
    /// 미사일 이름      특징         인덱스
    /// AIM120       공격력이 높음      0
    /// AIM9         공격 속도가 높음   1
    /// 
    /// </summary>

    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;

    [Header("미사일 프리팹과 정보")]    
    public GameObject[] missileList;  //미사일 리스트 
    public WeaponData[] missileInfo;  //미사일 정보가 담긴 ScriptableObject

    List<int> equippedMissile;                //장착가능한 미사일 리스트
    [HideInInspector] public int eqMissileIndex = 0;  //현재 장착 중인 무기 인덱스

    public bool fireEnable;
    
    GameObject missileObjL;    //현재 발사할 수 있는 왼쪽 미사일
    GameObject missileObjR;    //현재 발사할 수 있는 오른쪽 미사일

    [Header("미사일 대미지/장탄수")]
    [HideInInspector] public int[] missileCount;        //현재 남은 미사일 개수
    public int[] damageLevel;
    public int[] maxCountLevel;

    [Header("미사일 발사 위치")]
    public Transform missileFirePosL;     //왼쪽 미사일 발사 위치
    public Transform missileFirePosR;     //오른쪽 미사일 발사 위치

    [Header("미사일 딜레이")]
    [SerializeField] bool[] onDelayL;     //왼쪽 미사일 fire가능한지
    [SerializeField] bool[] onDelayR;     //index 0~1 오른쪽 미사일 fire 가능한지

    [Header("미사일 쿨타임")]
    float[] missileCoolTime;              //미사일 쿨타임 시간 설정
    public float[] missileCoolTimerL;     //왼쪽 미사일 발사 가능까지 남은 시간
    public float[] missileCoolTimerR;     //오른쪽 미사일 발사 가능까지 남은 시간
    //Coroutine timeCoroutine;            //코루틴 실행 변수

    [Header("LockOn")]
    [SerializeField]
    public Transform targeting;
    TargetObject targets;//타겟을 불러오는 변수- 
    public List<GameObject> enemies = new List<GameObject>();
    Missile missileObj;

    private void Awake()
    {
        equippedMissile = new List<int>();

        missileCoolTime = new float[missileList.Length];
        missileCoolTime[0] = missileInfo[0].attackDelay;
        missileCoolTime[1] = missileInfo[1].attackDelay;
    }

    private void Start()
    {
        fireEnable = false;

        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyArray);

        Invoke("FireEnableTrue", 5f);

        //장탄수 할당
        int index;
        for(index = 0; index < missileList.Length; index++)
        {
            missileCount[index] = missileInfo[index].maxCounts[0];
        }

        //장착 중인 미사일 인덱스 초기화
        eqMissileIndex = 0;

        AddMissile(0);
        AddMissile(1);
    }
    
    private void Update()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.AddRange(enemyArray);
        FindCloseEnemy();
        DelayMissileCheck();

        if (missileObjL != null)
            missileObjL.GetComponent<Missile>().target = targeting;
        if (missileObjR != null)
            missileObjR.GetComponent<Missile>().target = targeting;

    }

    void FireEnableTrue()
    {
        fireEnable = true;
    }

    //미사일 발사 조건 체크하는 함수
    public void OnFire(InputValue value)
    {
        if (!GameManager.instance.isStick && value.isPressed && missileCount[eqMissileIndex] > 0 && fireEnable)
        {
            FireMissile(); //미사일 쏘는 함수   
        }
    }

    //미사일 발사 조건 체크하는 함수(스틱)
    void OnFireMissile_Stick(InputValue value)
    {
        if (GameManager.instance.isStick && value.isPressed && missileCount[eqMissileIndex] > 0 && fireEnable)
        {
            FireMissile();
        }
    }

    //무기 교체 버튼 입력받는 함수(패드)
    void OnWeaponChange(InputValue value)
    {
        if(!GameManager.instance.isStick && value.isPressed)
        {
            WeaponChange();
        }
    }

    //무기 교체 버튼 입력받는 함수(스틱)
    void OnWeaponChange_Stick(InputValue value)
    {
        if(GameManager.instance.isStick && value.isPressed)
        {
            WeaponChange();
        }
    }

    //무기를 교체하는 함수
    void WeaponChange()
    {
        if (eqMissileIndex < equippedMissile.Count - 1 && equippedMissile.Contains(eqMissileIndex + 1))
        {
            eqMissileIndex++;  //장착 중인 무기 인덱스 1 증가( 무기 변경)
        }
        else if (eqMissileIndex == equippedMissile.Count - 1 && equippedMissile.Contains(0))
        {
            eqMissileIndex = equippedMissile[0];
        }

        Debug.Log("현재 장착 중인 무기 인덱스 : " + eqMissileIndex);
    }

    //미사일 발사하는 함수
    void FireMissile()
    {
        if (missileCount[eqMissileIndex] > 0)   //미사일을 가지고 있으면
        {
            if (!onDelayL[eqMissileIndex] && missileCount[eqMissileIndex] % 2 == 0)   //무기가 발사가능한 상태이고, 미사일 개수가 짝수라면 (왼쪽 미사일이 발사된다)   
            {
                for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
                    if (PoolManager.instance.prefabs[index] == missileList[eqMissileIndex])
                    {
                        missileObjL = PoolManager.instance.Get(index); 
                        missileObjL.transform.parent = PoolManager.instance.transform.GetChild(index);   
                        missileObjL.transform.position = missileFirePosL.transform.position;    //오브젝트 발사위치 초기화   
                        missileObjL.transform.rotation = transform.rotation;

                        if (targeting = null)
                        {
                            missileObjL.GetComponent<Missile>().target = null;
                        }

                        if (missileObjL.GetComponent<Missile>().angle < missileObjL.GetComponent<Missile>().boresightAngle)
                        {
                            missileObjL.GetComponent<Missile>().target = targeting;
                        }

                        missileCoolTimerL[eqMissileIndex] = 0f;       //발사를 하므로 쿨타임에 0을 대입한다.   
                        onDelayL[eqMissileIndex] = true;              //딜레이 상태가 된다.

                        missileCount[eqMissileIndex]--;               //미사일을 발사했으므로 탄수가 1개 줄어든다.   
                        RumbleManager.instance.RumblePulse(0.25f, 1f, 0.25f);   //진동   
                        break;
                    }
            }


            else if (!onDelayR[eqMissileIndex] && missileCount[eqMissileIndex] % 2 == 1)   
            {
                for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
                    if (PoolManager.instance.prefabs[index] == missileList[eqMissileIndex])
                    {
                        missileObjR = PoolManager.instance.Get(index); 
                        missileObjR.transform.parent = PoolManager.instance.transform.GetChild(index);   
                        missileObjR.transform.position = missileFirePosR.transform.position;    //오브젝트 발사위치 초기화   
                        missileObjR.transform.rotation = transform.rotation;

                        if (targeting = null)
                        {
                            missileObjR.GetComponent<Missile>().target = null;
                        }
                        if (missileObjR.GetComponent<Missile>().angle < missileObjR.GetComponent<Missile>().boresightAngle)
                        {
                            missileObjR.GetComponent<Missile>().target = targeting;
                        }

                        missileCoolTimerR[eqMissileIndex] = 0f;           //발사를 하므로 쿨타임을 0으로 초기화한다.   
                        onDelayR[eqMissileIndex] = true;

                        missileCount[eqMissileIndex]--;     
                        RumbleManager.instance.RumblePulse(0.25f, 1f, 0.25f);   //진동   
                        break;
                    }
            }

        }
    }

    void DelayMissileCheck()
    {
        if (onDelayL[0])
        {
            FireCoolL(0);
        }

        if (onDelayL[1])
        {
            FireCoolL(1);
        }

        if (onDelayR[0])
        {
            FireCoolR(0);
        }

        if (onDelayR[1])
        {
            FireCoolR(1);
        }
    }

    //왼쪽 미사일 딜레이 
    void FireCoolL(int index)
    {
        if (missileCoolTimerL[index] < missileCoolTime[index])  //duration보다 타이머가 작으면 Time.deltaTime을 더한다.    
        {
            missileCoolTimerL[index] += Time.deltaTime;    //fire하고 쿨타임이 돌아가므로 왼쪽 미사일 아이템은 발사가 불가능하다  

            if (missileCoolTimerL[index] >= missileCoolTime[index])     //쿨타임이 지나면  
            {
                onDelayL[index] = false;     //무기는 발사가능한 상태가 된다 
            }
        }
    }

    //오른쪽 미사일 딜레이
    void FireCoolR(int index)
    {
        if (missileCoolTimerR[index] < missileCoolTime[index])  //쿨타임이 안 찼으면    
        {
            missileCoolTimerR[index] += Time.deltaTime;   

            if (missileCoolTimerR[index] >= missileCoolTime[index])   
            {
                onDelayR[index] = false;   
            }
        }
    }

    public void SortMissile()  //무기(미사일) 정렬  , 무기 UI 표시 스크립트에서 접근함
    {
        equippedMissile.Sort();   
    }

    //발사 가능한 미사일 추가
    public void AddMissile(int index) //사용가능한 무기 (인덱스로) 추가 ,GM에서 접근함
    {
        if (!equippedMissile.Contains(index))
        {
            // Debug.Log(index + "무기 추가");
            equippedMissile.Add(index);   

            eqMissileIndex = index;
        }
    }

    public void FindCloseEnemy()
    {
        float closeDistance = Mathf.Infinity;
        Transform closeTarget = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closeDistance)
                {
                    closeDistance = distance;
                    closeTarget = enemy.transform;
                }
            }
        }
        targeting = closeTarget;
    }
}
