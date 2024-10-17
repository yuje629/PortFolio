using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    Rigidbody rb;  //�÷��̾� rigidbody

    public float playerMaxHP;
    public float playerCurHP;
    [SerializeField] public GameObject[] QTETimeLine;
    [SerializeField] public QTEManager QTE;
    private AudioSource audioSource;

    [Header("���� ����Ʈ")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] ParticleSystem damagedExplosion;
    [SerializeField] AudioSource explosionSound;

    [Header("�ı��� UI OFF")]
    [SerializeField] GameObject[] UI = new GameObject[2];

    [Header("��")]
    public float altitude; //��ǥ��

    [Header("�÷��̾� ȸ��")]  //Rotate ������
    public float pitchAmount;   //xȸ�� ũ��
    public float yawAmount;     //y"
    public float rollAmount;    //z"
    public float lerpAmount;    //ȸ�� �󸶳� �ڿ������� ���� ������

    Vector3 rotateValue;        //�÷��̾� �̵� �� �ʿ��� ȸ�� ��
    Vector3 rotateVector;

    public Button missionFailedBtn;

    public Vector3 RotateValue
    {
        get { return rotateValue; }
    }

    [Header("HIGH-G TURN")]
    public float highGPitchFactor = 1;

    [Header("�÷��̾� �̵�")]     //Move ������
    public float speed;           //���� �̵� �ӵ�
    public float maxSpeed;        //�ִ� �ӷ�
    public float minSpeed;        //�ּ� �ӷ�
    public float defaultSpeed;    //�⺻�ӵ�
    public float accelAmount;     //���� ũ��
    public float brakeAmount;     //���� ũ��

    [SerializeField] float gravityFactor;   //����� ���� �ӵ� ������

    //[Header("----Wings----")]
    private SwordMaster.FighterGame.FighterAnimationSystem playerAnimationSystem;

    public enum BoosterState
    {
        None,
        SpeedIncrease,     // �ν��ͷ� ���� �ӵ� ���� ����
        SpeedMaintenance,  // �ν��� �ӵ� ���� ����
        SpeedDecrease,      // �ν��Ͱ� ���� �� �ӵ� ���� ����
        OnDelay
    }

    [Header("�ν���")]
    public float boosterSpeed;       //�ν��� ���ǵ�
    public float boosterAmount;      //�ν��� amount

    public float boosterTime;        //�ν��� ���� �ð�
    private float boosterTimer = 0;  //�ν��� ���� �ð� Ÿ�̸�
    public float boosterDelayTime;   //�ν��� ��ų ������
    public float boosterDelayTimer; //�ν��� ��ų ������ Ÿ�̸�

    public bool onBoosterDelay;      //�ν��� ��ų ������ ����
    private bool isBooster;          //�ν��� ��ų �ߵ� ������ ����
    public BoosterState boosterState;

    [Header("�÷���")]
    public GameObject flarePrefab;
    public Transform flareSpawnPos;
    [SerializeField] private int flareCallCount;
    public int flareRepeatCount;
    public float repeatTime;
    public float flareDelayTime;
    public float flareDelayTimer;
    private bool onFlareDelay;
    WaitForSeconds flareWait;

    //InputAction ������ //
    GameInput gameInput;  //InputSystem ������ ����ִ�?? ��ũ��Ʈ
    InputAction pitch;
    InputAction roll;
    InputAction accelerate;
    InputAction brake;
    InputAction yaw_Stick;
    InputAction accel_Stick;


    //��Ʈ�ѷ� ���� float���� �޾ƿ��� ���� ������ ������
    float pitchValue;  //pitch,x�� ȸ�� ��
    float yawValue;    //yaw, y�� ȸ�� ��
    float rollValue;   //roll, z�� ȸ�� ��
    float accelValue;  //acclerate, ����
    float brakeValue;  //brake, ����

    void Awake()
    {
        //Input ���� �� InputSystem�� �ִ� Actions �ҷ�����//
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
        rb = GetComponent<Rigidbody>();  //rigidbody ������Ʈ ��������
    }

    private void OnEnable()
    {
        //InputAction���� ��//
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

        UpdateControlValue();  //Input��
        RotateFlight();//��ü ȸ��
        Move();
        CheckBoosterState();

        playerAnimationSystem.PlayerWingAnimation(accelValue, yawValue, pitchValue, rollValue, isBooster);  //��ü ���� �ִϸ��̼�
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
        pitchValue = pitch.ReadValue<float>(); //��Ʈ�ѷ� �� �޾ƿ���
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
            (!(transform.rotation.eulerAngles.z > 80 && transform.rotation.eulerAngles.z < 280))) //������ �������� ������ �������Ϸ��� �۵��Ѵ�.
        {
            Autopilot();  //�������Ϸ� ����, z���� ������ �̷�� �ȴ�.
        }
        else
        {
            rotateVector = new Vector3(pitchValue * pitchAmount * highGPitchFactor, (yawValue - 0.5f) * 2f * yawAmount, -rollValue * rollAmount);//(�е� �� * ȸ���� ����)�� rotateVector�� ����
        }

        rotateValue = Vector3.Lerp(rotateValue, rotateVector, lerpAmount * Time.fixedDeltaTime);  //ȸ���ϰ��� �ϴ� ��, Lerp()�� �̿��� �� �� �ε巴�� ȸ���Ѵ�.
        transform.Rotate(rotateValue * Time.fixedDeltaTime);
    }

    void Move()
    {
        if (!GameManager.instance.isStick)
        {
            if (!isBooster)
            { 
                speed += accelValue * accelAmount * Time.fixedDeltaTime;   //accel��ư�� ������ accelAmount��ŭ speed�� �ö󰣴�.
                                                                           //����
                speed -= brakeValue * brakeAmount * Time.fixedDeltaTime; //brake��ư�� ������ brakeAmount��ŭ speed�� ��������.
            }
        }
        else
        {
            if (!isBooster)
                speed += (accelValue - 0.5f) * 2 * accelAmount * Time.fixedDeltaTime;
        }

        //�Ϲ� �ӵ�
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

        rb.velocity = transform.forward * speed; //rigidbody �ӵ�

        //�߷� ���� , ����� ���� gravityFactor��ŭ�� �ӵ��� �����ϰ� �ϰ��� ���� gravityFactor��ŭ�� �ӵ��� �����Ѵ�.
        if (!isBooster)
        {
            float gravityFallByPitch = gravityFactor * Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad);  //Mathf.Sin�� �־��༭ -1 ~ 1 ������ ��ȯ�Ѵ�.
            speed += gravityFallByPitch * Time.fixedDeltaTime;  //���� �ӵ��� �߷°� ����
        }
    }

    void Autopilot()
    {
        rotateVector = -transform.rotation.eulerAngles;  //������ ���߱� ���� -�� ���� ������ ���� �����Ѵ�.
        if (rotateVector.z < -180) rotateVector.z += 360;//-180���� -������ �ƴ� + �������� ���� �ϱ� ���� 360���� �����ش�.
                                                         //�׸��� ������ ȸ���ϱ� ���� x2�� ���ش�.
        rotateVector.x = 0; //z�� ȸ���� �÷��̾ �ϹǷ� ���� ���Ϸ� ���� X
        rotateVector.z = Mathf.Clamp(rotateVector.z * 2, -rollAmount, rollAmount);
        rotateVector.y = 0; //y���� ���ư��� �����̶� �ٲ��� �ʱ� ���� 0�� �Ҵ�
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

    public void TakeDamage(float damage) // ���� �޾�����
    {
        playerCurHP -= damage;

        damagedExplosion.Play();
        explosionSound.Play();

        if (playerCurHP <= 0)
        {
            UI[0].SetActive(false); // HUD
            UI[1].SetActive(false); // Target
            UI[2].SetActive(true); // ����� ���� UI

            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    //  ��ü �ı� ��ƼŬ
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

    private void OnCollisionEnter(Collision collision) // �浹������
    {
        if (collision.transform.tag == "Ground" || collision.transform.tag == "EnemyHP")
        {
            UI[0].SetActive(false); // HUD
            UI[1].SetActive(false); // Target
            UI[2].SetActive(true); // ����� ���� UI

            for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
            {
                if (explosionPrefab == PoolManager.instance.prefabs[index])
                {
                    //  ��ü �ı� ��ƼŬ
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
        //InputAction���� ��//
        pitch.Disable();
        yaw_Stick.Disable();
        roll.Disable();
        accelerate.Disable();
        brake.Disable();
        accel_Stick.Disable();
    }
}