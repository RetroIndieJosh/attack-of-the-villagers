using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

enum TimerStyle
{
    CountDown,
    CountUp
}

public class ElapsedTimer : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI m_displayTextMesh = null;

    [SerializeField]
    private TimerStyle m_style = TimerStyle.CountUp;

    [SerializeField]
    private int m_timerStartHour = 0;

    [SerializeField]
    private int m_timerStartMinute = 0;

    [SerializeField]
    private int m_timerStartSec = 0;

    public bool HasEnded { get { return m_style == TimerStyle.CountDown && m_timeElapsedSec < Mathf.Epsilon; } }

    private float m_timeElapsedSec = 0.0f;

    public void ResetTime() {
        m_timeElapsedSec = m_timerStartHour * 3600 + m_timerStartMinute * 60 + m_timerStartSec;
    }

    public void SetStart(int m_hour, int m_minute, int m_second ) {
        m_timerStartHour = m_hour;
        m_timerStartMinute = m_minute;
        m_timerStartSec = m_second;
    }

    private void Start() {
        ResetTime();
    }

    private void Update() {
        if ( m_timeElapsedSec < Mathf.Epsilon ) return;

        if( m_style == TimerStyle.CountDown )
            m_timeElapsedSec -= Time.deltaTime;
        else m_timeElapsedSec += Time.deltaTime;

        var remaining = m_timeElapsedSec;
        var hours = Mathf.Max( 0, Mathf.FloorToInt( remaining / 3600.0f ) );
        remaining -= hours * 3600.0f;
        var minutes = Mathf.Max( 0, Mathf.FloorToInt( remaining / 60.0f ) );
        remaining -= minutes * 60.0f;
        var seconds = Mathf.Max( 0, Mathf.FloorToInt( remaining ) );

        //m_displayTextMesh.text = string.Format( "{0}:{1}:{2}", hours.ToString( "D2" ), minutes.ToString( "D2" ), seconds.ToString( "D2" ) );
        m_displayTextMesh.text = string.Format( "{0}s", seconds.ToString( "D2" ) );
    }
}
