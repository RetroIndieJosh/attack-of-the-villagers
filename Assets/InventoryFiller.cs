using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Inventory ) )]
public class InventoryFiller : MonoBehaviour
{
    [SerializeField]
    private GameObject m_itemPrefab;

    [SerializeField]
    private int m_min = 1;

    [SerializeField]
    private int m_max = 1;

    public void SetRange(int a_min, int a_max = -1 ) {
        m_min = a_min;
        if ( a_max == -1 ) m_max = a_min;
        else m_max = a_max;
        FillInventory();
    }

    private void Awake() {
        FillInventory();
    }

    private void FillInventory() {
        var inventory = GetComponent<Inventory>();
        inventory.Clear();

        var quantity = Random.Range( m_min, m_max );
        for ( int i = 0; i < quantity; ++i )
            inventory.Add( m_itemPrefab );
    }
}
