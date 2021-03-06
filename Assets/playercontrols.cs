﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class playercontrols : NetworkBehaviour
{
    // Start is called before the first frame update
    public float sensitivity = 2f;
    private float speed = 5f;
    private float jumpSpeed = 8.0f;
    private float gravity = 0.4f;
    private Transform cam;
    private CharacterController control;
    private Vector3 moveDirection = Vector3.zero;
    private NetworkManager net;
    public ParticleSystem muzzle;
    private float fireTimer = 0f;
    [SyncVar]
    public float hp = 100;
    public int kills = 0;
    public int deaths = 0;
    void Start()
    {
        if(isLocalPlayer) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameObject.Find("overheadCam").SetActive(false);
            GetComponent<Renderer>().material.color = new Color(200, 100, 0);
            cam = transform.Find("Main Camera");
            control = GetComponent<CharacterController>();
            net = GameObject.Find("Terrain").GetComponent<NetworkManager>();
        } else {
            transform.Find("Main Camera").GetComponent<Camera>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer) {
            float move = Input.GetAxis("Mouse X") * sensitivity;
            transform.Rotate(0f, move*sensitivity, 0f);
            float cmove = -Input.GetAxis("Mouse Y") * sensitivity;
            cam.Rotate(cmove, 0f, 0f);
            if((cam.localRotation.eulerAngles.x > 180f) && (cam.localRotation.eulerAngles.x < 300f)) {
                cam.localRotation = Quaternion.Euler(300f, 0f, 0f);
            } else if((cam.rotation.eulerAngles.x < 180f) && (cam.localRotation.eulerAngles.x > 80f)) {
                cam.localRotation = Quaternion.Euler(80f, 0f, 0f);
            }
            if(control.isGrounded) {
                moveDirection = Input.GetAxis("Vertical") * transform.forward * speed;
                moveDirection += Input.GetAxis("Horizontal") * transform.right * speed;
                if (Input.GetButton("Jump")) {
                    moveDirection.y = jumpSpeed;
                }
            }
            if(Input.GetButton("Fire1")) {
                if(fireTimer == 0) {
                    muzzle.Play();
                    fireTimer = 0.2f;
                    RaycastHit hit;
                    if(Physics.Raycast(transform.Find("Main Camera").position, transform.Find("Main Camera").TransformDirection(Vector3.forward), out hit, Mathf.Infinity)) {
                        if(hit.transform.tag == "Player") {
                            hit.transform.GetComponent<playercontrols>().CmdDamage(10);
                            //Debug.Log(hit.transform.tag);
                        }
                    }
                }
            }
            if(fireTimer > 0) {
                fireTimer -= Time.deltaTime;
            } else {
                fireTimer = 0;
            }
            moveDirection.y -= gravity;
            control.Move(moveDirection * Time.deltaTime);
            if(transform.position.y < -15) {
                CmdDeath();
            }
            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }
            if(hp <= 0f) {
                CmdDeath();
            }
        }
    }
    /*
    public void die() {
        deaths++;
        Transform pos = net.GetStartPosition();
        transform.position = pos.position;
        hp = 100;
    }
    */
    [ClientRpc]
    void RpcDamage(float dmg)
    {
        hp -= dmg;
       
    }
    [Command]
    void CmdDamage(float dmg)
    {
        RpcDamage(dmg);
    }
    [ClientRpc]
    void RpcDeath()
    {
        deaths++;
        Transform pos = net.GetStartPosition();
        transform.position = pos.position;
        hp = 100;
    }
    [Command]
    void CmdDeath()
    {
        RpcDeath();
    }
}
