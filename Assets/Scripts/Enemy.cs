using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    PlayerMovement player;
    [SerializeField]
    float movementSpeed = .0001f;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position, movementSpeed);
    }

    public void TakeDamage()
    {
        Destroy(gameObject);
    }
}
