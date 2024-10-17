using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    Rigidbody rb;  //플레이어 rigidbody

    public float playerMaxHP;
    public float playerCurHP;
    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;
    private AudioSource audioSource;

    [Header("폭발 이펙트")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] ParticleSystem damagedExplosion;
    [SerializeField] AudioSource explosionSound;

    [Header("파괴시 UI OFF")]
    [SerializeField] GameObject[] UI = new GameObject[2];

    [Header("고도")]
    public float altitude; //고도표시

    [Header("플레이어 회전")]  //Rotate 변수들
    public float pitchAmount;   //x회전 크기
    public float yawAmount;     //y"
    public float rollAmount;    //z"
    public float lerpAmount;    //회전 얼마나 자연스럽게 빨리 꺾을지

    Vector3 rotateValue;        //플레이어 이동 시 필요한 회전 값
    Vector3 rotateVector;

    public Button missionFailedBtn;

    public Vector3 RotateValue
    {
        get { return rotateValue; }
    }

    [Header("HIGH-G TURN")]
    public float highGPitchFactor = 1;

    [Header("플레이어 이동")]     //Move 변수들
    public float speed;           //현재 이동 속도
    public float maxSpeed;        //최대 속력
    public float minSpeed;        //최소 속력
    public float defaultSpeed;    //기본속도
    public float accelAmount;     //가속 크기
    public float brakeAmount;     //감속 크기

    [SerializeField] float gravityFactor;   //기수에 따른 속도 가감값

    //[Header("----Wings----")]
    private SwordMaster.FighterGame.FighterAnimationSystem playerAnimationSystem;

    public enum BoosterState
    {
        None,
        SpeedIncrease,     // 부스터로 인한 속도 증가 상태
        SpeedMaintenance,  // 부스터 속도 유지 상태
        SpeedDecrease,      // 부스터가 끝난 후 속도 감소 상태
        OnDelay
    }

    [Header("부스터")]
    public float boosterSpeed;       //부스터 스피드
    public float boosterAmount;      //부스터 amount

    public float boosterTime;        //부스터 유지 시간
    private float boosterTimer = 0;  //부스터 유지 시간 타이머
    public float boosterDelayTime;   //부스터 스킬 딜레이
    public float boosterDelayTimer; //부스터 스킬 딜레이 타이머

    public bool onBoosterDelay;      //부스터 스킬 딜레이 여부
    private bool isBooster;          //부스터 스킬 발동 중인지 여부
    public BoosterState boosterState;

    [Header("플레어")]
    public GameObject flarePrefab;
    public Transform flareSpawnPos;
    [SerializeField] private int flareCallCount;
    public int flareRepeatCount;
    public float repeatTime;
    public float flareDelayTime;
    public float flareDelayTimer;
    private bool onFlareDelay;
    WaitForSeconds flareWait;

    //InputAction 변수들 //
    GameInput gameInput;  //InputSystem 정보가 담겨있는?? 스크립트
    InputAction pitch;
    InputAction roll;
    InputAction accelerate;
    InputAction brake;
    InputAction yaw_Stick;
    InputAction accel_Stick;


    //컨트롤러 값을 float으로 받아오기 위해 선언한 변수들
    float pitchValue;  //pitch,x축 회전 값
    float yawValue;    //yaw, y축 회전 값
    float rollValue;   //roll, z축 회전 값
    float accelValue;  //acclerate, 가속
    float brakeValue;  //brake, 감속

    void Awake()
    {
        //Input 생성 및 InputSystem에 있는 Actions 불러오기//
        gameInput = new GameInput();
        pitch = gameInput.Player.Pitch;
        yaw_Stick = gameInput.Player.Yaw_Stick;
        roll = gameInput.Player.Roll;
        accelerate = gameInput.Player.Accelerate;
        brake = gameInput.Player.Brake;
        accel_Stick = gameInput.Player.Accel_Stick;

        playerAnimationSystem = GetComponent<SwordMaster.FighterGame.FighterAnimationSystem>();

        flareWait = new WaitForSeconds(flareDelayTime);
    }

    void Start()
    {
        StopAllCoroutines();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();  //rigidbody 컴포넌트 가져오기
    }

    private void OnEnable()
    {
        //InputAction들을 켬//
        pitch.Enable();
        yaw_Stick.Enable();
        roll.Enable();
        accelerate.Enable();
        brake.Enable();
        accel_Stick.Enable();
    }

    void OnFlare(InputValue value)
    {
        if (!GameManager.instance.isStick && value.isPressed && !onFlareDelay)
        {
            UseFlareSkill();
            StartCoroutine(FlareCoolTimer());
        }
    }

    void OnFlare_Stick(InputValue value)
    {
        if (GameManager.instance.isStick && value.isPressed && !onFlareDelay)
        {
            UseFlareSkill();
            StartCoroutine(FlareCoolTimer());
        }
    }

    void UseFlareSkill()
    {
        flareCallCount++;

        for (int index = 0; index < GameManager.PoolManager.prefabs.Length; index++)
            if (GameManager.PoolManager.prefabs[index] == flarePrefab)
            {
                GameObject flare = PoolManager.instance.Get(index);
                flare.transform.parent = GameManager.PoolManager.transform.GetChild(index);
                flare.transform.position = flareSpawnPos.position;
                flare.transform.rotation = transform.rotation;
                break;
            }

        if (flareCallCount < flareRepeatCount)
            Invoke("UseFlareSkill", repeatTime);
    }

    IEnumerator FlareCoolTimer()
    {
        onFlareDelay = true;

        while (true)
        {
            flareDelayTimer += Time.deltaTime;

            if (flareDelayTimer >= flareDelayTime)
            {
                flareDelayTimer = 0;
                flareCallCount = 0;
                onFlareDelay = false;
                break;
            }

            yield return null;
        }
    }

    void OnBooster(InputValue value)
    {
        if (!GameManager.instance.isStick)
            if (value.isPressed && !isBooster && !onBoosterDelay)
            {
                boosterState = BoosterState.SpeedIncrease;
            }
    }

    void OnBooster_Stick(InputValue value)
    {
        if (GameManager.instance.isStick)
            if (value.isPressed && !isBooster && !onBoosterDelay)
            {
                boosterState = BoosterState.SpeedIncrease;
            }
    }

    void FixedUpdate()
    {
        audioSource.volume = Mathf.Clamp(-0.2f + (rb.velocity.magnitude * 0.0075f), 0f, 1f);

        UpdateControlValue();  //Input값
        RotateFlight();//기체 회전
        Move();
        CheckBoosterState();

        playerAnimationSystem.PlayerWingAnimation(accelValue, yawValue, pitchValue, rollValue, isBooster);  //기체 날개 애니메이션
        altitude = (int)gameObject.transform.position.y;
    }

    void CheckBoosterState()
    {
        switch (boosterState)
        {
            case BoosterState.None:
                break;

            case BoosterState.SpeedIncrease:
                {
                    isBooster = true;
                    if (speed < boosterSpeed)
                    {
                        speed += boosterAmount * Time.fixedDeltaTime;
                    }

                    if (speed >= boosterSpeed)
                    {
                        boosterTimer = 0;
                        boosterState = BoosterState.SpeedMaintenance;
                    }
                }
                break;

            case BoosterState.SpeedMaintenance:
                {
                    isBooster = true;

                    boosterTimer += Time.fixedDeltaTime;

                    if (boosterTimer >= boosterTime)
                        boosterState = BoosterState.SpeedDecrease;
                }
                break;

            case BoosterState.SpeedDecrease:
                {
                    isBooster = true;

                    if (speed > defaultSpeed)
                    {
                        speed -= boosterAmount * Time.fixedDeltaTime;

                        if (speed <= defaultSpeed)
                        {
                            boosterDelayTimer = 0;
                            boosterState = BoosterState.OnDelay;
                        }
                    }
                }
                break;

            case BoosterState.OnDelay:
                {
                    isBooster = false;
                    if (boosterDelayTimer < boosterDelayTime)
                    {
                        onBoosterDelay = true;
                        boosterDelayTimer += Time.fixedDeltaTime;
                    }

                    if (boosterDelayTimer >= boosterDelayTime)
                    {
                        boosterState = BoosterState.None;
                        onBoosterDelay = false;
                    }
                    break;
                }
        }
    }

    void UpdateControlValue()
    {
        pitchValue = pitch.ReadValue<float>(); //컨트롤러 값 받아오기
        rollValue = roll.ReadValue<float>();

        if (!GameManager.instance.isStick)
        {
            yawValue = 0.5f;

            if (!isBooster)
            {
                accelValue = accelerate.ReadValue<float>();
                brakeValue = brake.ReadValue<float>();
            }
        }
        else
        {
            yawValue = yaw_Stick.ReadValue<float>();

            if (!isBooster)
                accelValue = accel_Stick.ReadValue<float>();
        }
    }

    void RotateFlight()
    {
        if (!GameManager.instance.isStick)
            CheckHighGTurn();

        if (Mathf.Approximately(pitchValue, 0) && Mathf.Approximately(rollValue, 0) && Mathf.Approximately(yawValue, 0.5f) &&
            (!(transform.rotation.eulerAngles.z > 80 && transform.rotation.eulerAngles.z < 280))) //방향을 조정하지 않으면 오토파일럿이 작동한다.
        {
            Autopilot();  //오토파일럿 실행, z축이 수평을 이루게 된다.
        }
        else
        {
            rotateVector = new Vector3(pitchValue * pitchAmount * highGPitchFactor, (yawValue - 0.5f) * 2f * yawAmount, -rollValue * rollAmount);//(패드 값 * 회전할 각도)를 rotateVector에 대입
        }

        rotateValue = Vector3.Lerp(rotateValue, rotateVector, lerpAmount * Time.fixedDeltaTime);  //회전하고자 하는 값, Lerp()를 이용해 좀 더 부드럽게 회전한다.
        transform.Rotate(rotateValue * Time.fixedDeltaTime);
    }

    void Move()
    {
        if (!GameManager.instance.isStick)
        {
            if (!isBooster)
            { 
                speed += accelValue * accelAmount * Time.fixedDeltaTime;   //accel버튼을 누르면 accelAmount만큼 speed가 올라간다.
                                                                           //감속
                speed -= brakeValue * brakeAmount * Time.fixedDeltaTime; //brake버튼을 누르면 brakeAmount만큼 speed가 내려간다.
            }
        }
        else
        {
            if (!isBooster)
                speed += (accelValue - 0.5f) * 2 * accelAmount * Time.fixedDeltaTime;
        }

        //일반 속도
        if (!isBooster)
        {
            if (speed <= minSpeed)
                speed = minSpeed;

            if (speed >= maxSpeed)
                speed = maxSpeed;
        }
        else
        {
            if (speed <= minSpeed)
                speed = minSpeed;

            if (speed >= boosterSpeed)
                speed = boosterSpeed;
        }

        rb.velocity = transform.forward * speed; //rigidbody 속도

        //중력 영향 , 상승할 때는 gravityFactor만큼씩 속도가 증가하고 하강할 때는 gravityFactor만큼씩 속도가 감소한다.
        if (!isBooster)
        {
            float gravityFallByPitch = gravityFactor * Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad);  //Mathf.Sin에 넣어줘서 -1 ~ 1 값으로 변환한다.
            speed += gravityFallByPitch * Time.fixedDeltaTime;  //현재 속도에 중력값 적용
        }
    }

    void Autopilot()
    {
        rotateVector = -transform.rotation.eulerAngles;  //수평을 맞추기 위해 -를 곱한 각도로 값을 대입한다.
        if (rotateVector.z < -180) rotateVector.z += 360;//-180도면 -방향이 아닌 + 방향으로 가게 하기 위해 360도를 더해준다.
                                                         //그리고 빠르게 회전하기 위해 x2를 해준다.
        rotateVector.x = 0; //z축 회전은 플레이어가 하므로 오토 파일럿 설정 X
        rotateVector.z = Mathf.Clamp(rotateVector.z * 2, -rollAmount, rollAmount);
        rotateVector.y = 0; //y값은 나아가는 방향이라서 바꾸지 않기 위해 0을 할당
    }

    void CheckHighGTurn()
    {
        if (accelValue == 1 && brakeValue == 1)
        {
            accelAmount = 0f;
            brakeAmount = 100f;
            highGPitchFactor = 1.5f;
        }
        else
        {
            accelAmount = 50f;
            brakeAmount = 80f;
            highGPitchFactor = 1f;
        }
    }

    public void TakeDamage(float damage) // 공격 받았을때
    {
        playerCurHP -= damage;

        damagedExplosion.Play();
        explosionSound.Play();

        if (playerCurHP <= 0)
        {
            UI[0].SetActive(false); // HUD
            UI[1].SetActive(false); // Target
            UI[2].SetActive(true); // 종료시 나올 UI

            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    //  기체 파괴 파티클
                    GameObject explosion = PoolManager.instance.Get(index);
                    explosion.transform.parent = PoolManager.instance.transform.GetChild(index);
                    explosion.transform.position = transform.position;
                    explosion.transform.rotation = transform.rotation;
                    missionFailedBtn.Select();
                    break;
                }
            }
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) // 충돌했을때
    {
        if (collision.transform.tag == "Ground" || collision.transform.tag == "EnemyHP")
        {
            UI[0].SetActive(false); // HUD
            UI[1].SetActive(false); // Target
            UI[2].SetActive(true); // 종료시 나올 UI

            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    //  기체 파괴 파티클
                    GameObject explosion = PoolManager.instance.Get(index);
                    explosion.transform.parent = PoolManager.instance.transform.GetChild(index);
                    explosion.transform.position = transform.position;
                    explosion.transform.rotation = transform.rotation;
                    missionFailedBtn.Select();
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    private void OnDisable()
    {
        //InputAction들을 끔//
        pitch.Disable();
        yaw_Stick.Disable();
        roll.Disable();
        accelerate.Disable();
        brake.Disable();
        accel_Stick.Disable();
    }
}