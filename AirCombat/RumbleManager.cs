using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;  //�ν��Ͻ�ȭ

    private Gamepad pad;  //�����е�

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)//���� �︮�� �ϴ� �Լ�, �Ű�����: ���� ���� �󵵼�, ū �����󵵼�, �����ð�
    {
        pad = Gamepad.current;  //������ ��Ʈ�ѷ� ����

        if (pad != null)  //�����е尡 �ִٸ�
        {
            pad.SetMotorSpeeds(lowFrequency, highFrequency);   //���� �󵵼� ����
            StartCoroutine(StopRumble(duration, pad));  //�ڷ�ƾ ����
        }
    }

    private IEnumerator StopRumble(float duration, Gamepad pad)  //���� �ð� �����Ͽ� ���߰� �ϴ� �Լ�
    {
        float elapsedTime = 0f;   //���� ���� Ÿ�̸�
        while(elapsedTime < duration)  //duration���� Ÿ�̸Ӱ� ������ Time.deltaTime�� ���Ѵ�.
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pad.SetMotorSpeeds(0f, 0f); //���� �󵵼� ��� 0���� ����(��������)
    }
}
