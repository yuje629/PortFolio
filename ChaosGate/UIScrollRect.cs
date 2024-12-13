using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScrollRect : ScrollRect
{
    // 드래그 시작 방지
    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // 아무 작업도 하지 않음
    }

    // 드래그 방지
    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // 아무 작업도 하지 않음
    }
}
