using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour {
    public bool Targeted = false;

    private void Start() {
        var wave = MiniJamManager.instance.Wave;

        var health = GetComponent<Health>();
        health.SetRange( 0, 0.5f + wave * 0.5f );
        health.ResetToMaximum();

        GetComponent<AiMover3d>().Speed = 1.0f + ( wave - 1 )* 0.1f;
        GetComponent<Damager>().Damage = 1.0f + ( wave - 1 ) * 0.1f;

        //var minCoins = Mathf.FloorToInt( Mathf.Max( 1, wave * 0.5f ) );
        //var maxCoins = Mathf.FloorToInt( Mathf.Max( 1, wave * 0.75f ) );
        GetComponent<InventoryFiller>().SetRange( 1, Mathf.FloorToInt( health.Maximum ) );
    }
}
