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
        SetSpeed();     //�÷��̾� ���ǵ� �޾ƿͼ� �Ҵ�
        Invoke("ActiveFalse", lifetime);   //lifetime�� ������ ��Ȱ��ȭ��
    }

    private void FixedUpdate()
    {
        transform.Translate(new Vector3(0, 0, mSpeed * Time.fixedDeltaTime));
    }

    void SetSpeed()
    {
        mSpeed = GameManager.PlayerCtrl.speed + 300f;    //�Ѿ��� ��ü �ڿ��� ����� ����
    }

    void ActiveFalse()
    {
        gameObject.SetActive(false);
    }
}
