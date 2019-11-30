using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject snappablePrefab;
    public enum CanSeePlayer { Seen, Unseen }

    public CanSeePlayer snap;

    [Header("Add before start")]
    public float snapColliderRadius = 2.6f;

    GameObject snapArea;

    SphereCollider sphereCollider;

    void Start()
    {
        snap = CanSeePlayer.Unseen;
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = snapColliderRadius;
        gameObject.tag = "Enemy";
    }

    // Update is called once per frame
    void Update()
    {
        if(snap == CanSeePlayer.Seen)
        {
            if(snapArea == null)
            snapArea = Instantiate(snappablePrefab);
           // snapArea.transform.parent = gameObject.transform;
            snapArea.transform.position = transform.position;
        }
        else if(snap == CanSeePlayer.Unseen)
        {
            Destroy(snapArea);
        }
    }
}
