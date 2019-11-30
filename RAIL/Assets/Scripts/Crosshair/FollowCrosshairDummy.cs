using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCrosshairDummy : MonoBehaviour
{
    public Transform crosshairDummy;

    // Update is called once per frame
    void Update()
    {
        transform.position = crosshairDummy.transform.position;
    }
}
