using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    private float mSpeed;
    private float lifetime = 5f;
    TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        trailRenderer.Clear();
        SetSpeed();     //플레이어 스피드 받아와서 할당
        Invoke("ActiveFalse", lifetime);   //lifetime이 지나면 비활성화됨
    }

    private void FixedUpdate()
    {
        transform.Translate(new Vector3(0, 0, mSpeed * Time.fixedDeltaTime));
    }

    void SetSpeed()
    {
        mSpeed = GameManager.PlayerCtrl.speed + 300f;    //총알이 기체 뒤에도 생기는 원인
    }

    void ActiveFalse()
    {
        gameObject.SetActive(false);
    }
}
