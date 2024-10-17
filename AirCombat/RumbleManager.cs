using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;  //인스턴스화

    private Gamepad pad;  //게임패드

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)//진동 울리게 하는 함수, 매개변수: 작은 진동 빈도수, 큰 진동빈도수, 진동시간
    {
        pad = Gamepad.current;  //현재의 컨트롤러 참조

        if (pad != null)  //게임패드가 있다면
        {
            pad.SetMotorSpeeds(lowFrequency, highFrequency);   //진동 빈도수 설정
            StartCoroutine(StopRumble(duration, pad));  //코루틴 실행
        }
    }

    private IEnumerator StopRumble(float duration, Gamepad pad)  //진동 시간 설정하여 멈추게 하는 함수
    {
        float elapsedTime = 0f;   //진동 종료 타이머
        while(elapsedTime < duration)  //duration보다 타이머가 작으면 Time.deltaTime을 더한다.
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pad.SetMotorSpeeds(0f, 0f); //진동 빈도수 모두 0으로 설정(진동끄기)
    }
}
