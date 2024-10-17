using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;

public class WarningLine : MonoBehaviour
{
    public float m_lineWidth;
    public Vector2 m_startPos;
    public Vector2 m_endPos;
    public Color m_color;
    public float m_duration;
    public LineRenderer lineRenderer;
    private float time = 0.7f;
    private float timer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        m_color = lineRenderer.startColor;
    }

    void OnEnable()
    {
        timer = 0;
        Invoke("SetActiveFalse", m_duration);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float alpha = Mathf.Lerp(0, 0.3f, timer / time);
        m_color.a = alpha;

        lineRenderer.startColor = m_color;
        lineRenderer.endColor = m_color;
        lineRenderer.startWidth = m_lineWidth;
        lineRenderer.endWidth = m_lineWidth;

        Draw2Dray(m_startPos, m_endPos);

        if (timer >= time)
            timer = 0;
    }

    public void LineInit(float lineWidth, Vector2 startPos, Vector2 endPos, float duration)
    {
        m_lineWidth = lineWidth;
        m_startPos = startPos;
        m_endPos = endPos;
        m_duration = duration;
    }

    private void SetActiveFalse()
    {
        gameObject.SetActive(false);
    }

    private void Draw2Dray(Vector2 start, Vector2 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
