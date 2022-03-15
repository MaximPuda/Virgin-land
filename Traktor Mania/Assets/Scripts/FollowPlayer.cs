using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] readonly Vector3 offset = new Vector3(0,0,0);
    void Update()
    {
        transform.position = player.position + offset;
    }
}
