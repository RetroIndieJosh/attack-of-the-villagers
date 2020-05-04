using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceZeroZ : MonoBehaviour {
    private void LateUpdate() {
        var pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }
}
