using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
class Purchase
{
    public string name = "Item";
    public int price = 0;
    public float priceMult = 1.5f;
    public int basePrice = 0;
    public UnityEvent onPurchase = new UnityEvent();
}

public class Shop : MonoBehaviour {
    [SerializeField]
    Counter m_currencySource = null;

    [SerializeField]
    List<Purchase> m_purchaseList = new List<Purchase>();

    public int GetPrice( string a_name ) {
        foreach ( var purchase in m_purchaseList ) {
            if ( purchase.name == a_name ) return purchase.price;
        }

        return -1;
    }

    // returns amount spent, or -1 if failed
    public int Purchase( string a_name ) {
        foreach ( var purchase in m_purchaseList ) {
            if ( purchase.name == a_name ) {
                if ( m_currencySource.CountAsInt < purchase.price )
                    return -1;
                m_currencySource.Add( -purchase.price );
                purchase.onPurchase.Invoke();

                var spent = purchase.price;
                purchase.price = Mathf.FloorToInt( purchase.price * purchase.priceMult );
                return spent;
            }
        }

        Debug.LogErrorFormat( "[Shop] No item to purchase with name '{0}'", a_name );
        return -1;
    }

    public void ResetPrices() {
        foreach ( var purchase in m_purchaseList )
            purchase.price = purchase.basePrice;
    }

    private void Awake() {
        foreach ( var purchase in m_purchaseList )
            purchase.basePrice = purchase.price;
        ResetPrices();
    }
}
