using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWielder : MonoBehaviour {
    [SerializeField]
    private MeleeWeapon m_weaponPrefab = null;

    [SerializeField]
    private bool m_useForward = false;

    public MeleeWeapon Weapon {  get { return m_weapon; } }

    private MeleeWeapon m_weapon = null;

    private void Awake() {
        m_weapon = Instantiate( m_weaponPrefab );
    }

    private void LateUpdate() {
        if ( m_useForward ) {
            m_weapon.transform.position = transform.position;
            m_weapon.transform.up = transform.forward;
            m_weapon.transform.position += m_weapon.transform.up;
        } else {
            m_weapon.transform.position = transform.position + Vector3.up;
            m_weapon.transform.rotation = Quaternion.identity;
            m_weapon.transform.RotateAround( transform.position, Vector3.forward, transform.eulerAngles.z );
        }
    }
}
