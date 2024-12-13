using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform player;            //현재 플레이어 위치
    public Transform villageSpawnPos;   //마을 스폰 위치
    public Transform fieldSpawnPos;     //필드 스폰 위치

    public CameraManager cameraManager; 
    public UIManager uiManager;
    public spawner1 waveManager;

    public Map currentMap;

    SoundManager soundManager;

    public enum Map
    {
        Village,
        Field,
    }

    private void Start()
    {
        if (currentMap == Map.Village)
            uiManager.SetUIActive(uiManager.fieldUI, false);
        else if (currentMap == Map.Field)
            uiManager.SetUIActive(uiManager.fieldUI, true);
        soundManager = SoundManager.instance;
    }

    public void OnVillage()
    {
        //현재 맵으로
        currentMap = Map.Village;
        //텀

        //캐릭터 위치 이동
        player.SetPositionAndRotation(villageSpawnPos.position, villageSpawnPos.rotation);
        //카메라 전환
        cameraManager.ShowVillageView();
        //WAVE UI 끄기
        uiManager.SetUIActive(uiManager.fieldUI, false);
        //WaveManager 끄기
        // waveManager.gameObject.SetActive(false);
        //사운드
        soundManager.PlayMusic(1, 0.3f);
    }

    public void OnField()
    {
        //현재 맵으로
        currentMap = Map.Field;
        //텀

        //캐릭터 위치 이동
        player.SetPositionAndRotation(fieldSpawnPos.position, fieldSpawnPos.rotation);
        //카메라 전환
        cameraManager.ShowFieldView();
        //WAVE UI 켜기
        uiManager.SetUIActive(uiManager.fieldUI, true);
        //WaveManager 켜기
        waveManager.gameObject.SetActive(true);
        waveManager.increaseStage();
        //사운드
        soundManager.PlayMusic(2, 0.3f);
    }
}
