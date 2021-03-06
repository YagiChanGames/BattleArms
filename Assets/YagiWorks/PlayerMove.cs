﻿using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }
    public float force = 100.0f;
    public int jumpCount = 0;       // ジャンプした回数
    public const int MAX_JUMP_COUNT = 2;    // ジャンプできる最大回数

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            jumpCount = 0; // ジャンプ回数初期化
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("up")|| Input.GetKey("w"))
        {
            transform.position += transform.forward ;
        }
        if (Input.GetKey("down")|| Input.GetKey("s"))
        {
            transform.position -= transform.forward;
        }

        if (Input.GetKey("right")|| Input.GetKey("e"))
        {
            transform.Rotate(0, 5, 0);
        }
        if (Input.GetKey("left")|| Input.GetKey("q"))
        {
            transform.Rotate(0, -5, 0);
        }
        if (jumpCount < MAX_JUMP_COUNT && Input.GetKey("x"))
        {
            jumpCount++;
            GetComponent<Rigidbody>().AddForce(transform.up * force, ForceMode.Impulse);
        }
    }
}