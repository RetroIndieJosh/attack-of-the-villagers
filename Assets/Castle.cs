using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Castle : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI m_damageTextMesh;

    private Damager m_damager = null;
    private ElapsedTimer m_timer = null;

    private void Awake() {
        m_damager = MiniJamManager.instance.Player.GetComponent<WeaponWielder>().Weapon.GetComponent<Damager>();
        m_timer = GetComponent<ElapsedTimer>();
        UpdateTimer();
    }

    private void OnEnable() {
        UpdateTimer();
        m_timer.ResetTime();
    }

    private void Update() {
        if ( m_timer.HasEnded ) {
            m_damager.Damage += 1.0f;
            UpdateTimer();
        }
    }

    private void UpdateTimer() {
        m_timer.SetStart( 0, 0, 10 + Mathf.FloorToInt( 5 * m_damager.Damage ) );
        m_timer.ResetTime();
        m_damageTextMesh.text = "" + m_damager.Damage;
    }
}
