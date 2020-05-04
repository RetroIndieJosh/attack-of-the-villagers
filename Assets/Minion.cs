using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour {
    [SerializeField]
    private float m_timeBetweenSwings = 1.0f;

    [SerializeField]
    private float m_swingDistance = 1.5f;

    private AiMover3d m_mover = null;
    private WeaponWielder m_weaponWielder = null;

    private float m_timeSinceLastSwing = 0.0f;
    private bool m_targetIsEnemy = false;

    private void Awake() {
        m_mover = GetComponent<AiMover3d>();
        m_weaponWielder = GetComponent<WeaponWielder>();

        var collector = GetComponent<Collector>();
        collector.Counter = Counter.FindByName( "Coins" );
    }

    private void Update() {
        Attack();
        DetermineMoveMode();
        DetermineTarget();
    }

    private void Attack() {
        m_timeSinceLastSwing += Time.deltaTime;

        if ( m_weaponWielder.Weapon == null || m_mover.Target == null ) return;
        if ( Vector3.Distance( transform.position, m_mover.Target.transform.position ) > m_swingDistance ) return;

        if ( m_timeSinceLastSwing >= m_timeBetweenSwings ) {
            m_weaponWielder.Weapon.Attack();
            m_timeSinceLastSwing = 0.0f;
        }
    }

    private void DetermineMoveMode() {
        if ( m_mover.Target == null ) return;

        if ( m_targetIsEnemy ) {
            var distance = Vector3.Distance( transform.position, m_mover.Target.transform.position );
            if ( distance < m_swingDistance ) m_mover.Mode = AiMoveMode.Avoid;
            else if ( distance > 2.0f * m_swingDistance ) m_mover.Mode = AiMoveMode.Seek;
        } else {
            m_mover.Mode = AiMoveMode.Seek;
        }
    }

    private void DetermineTarget() {
        if ( m_mover.Target != null ) return;

        var boundsRect = GetComponent<Bounds>().Rect;
        var closestDistance = Mathf.Infinity;

        var searchList = new List<GameObject>();
        searchList.AddRange( GameObject.FindGameObjectsWithTag( "Villager" ) );
        var collectibleList = new List<GameObject>();
        foreach ( var collectible in FindObjectsOfType<Collectible>() )
            collectibleList.Add( collectible.gameObject );
        searchList.AddRange( collectibleList );

        GameObject closest = null;
        foreach( var target in searchList ) {
            if ( boundsRect.Contains( target.transform.position ) == false ) continue;
            var distance = Vector3.Distance( transform.position, target.transform.position );
            if( distance < closestDistance ) {
                var villager = target.GetComponent<Villager>();
                if( villager != null && villager.Targeted == true ) continue;
                closestDistance = distance;
                closest = target;
            }
        }

        if ( collectibleList.Contains( closest ) ) {
            m_mover.TargetDistance = 0.0f;
            m_targetIsEnemy = false;
        } else if( closest != null ) {
            m_mover.TargetDistance = 1.0f;
            m_targetIsEnemy = true;
            //closest.GetComponent<Villager>().Targeted = true;
        }

        m_mover.Target = closest;
    }
}
