using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rob01Ctrl : MonoBehaviour
{
    public float speed = 1.0f;
    public float battleSpeed = 2f;
    public Transform bulletSpawnPointLeft;
    public Transform bulletSpawnPointRight;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5;
    public float particleDalay = 0.5f;

    public float moveSpeed = 3f;
    public float rotateSpeed = 180f;

    public float mouseSensitivity = 200f;

    public Transform cameraPivot;

    private float xRotation = 0f;

    Animator anim;
    CharacterController controller;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        anim.SetFloat("speedMultiplier", speed);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        anim.SetBool("shoot", false);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetAxis("Horizontal") > 0.1f)
            {
                anim.SetBool("turnLeft", true);
                anim.SetBool("turnRight", false);
            }
            if (Input.GetAxis("Horizontal") < -0.1f)
            {
                anim.SetBool("turnRight", true);
                anim.SetBool("turnLeft", false);
            }
        }
        else
        {
            anim.SetBool("turnLeft", false);
            anim.SetBool("turnRight", false);

            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                anim.SetFloat("Turns", Input.GetAxis("Horizontal"));
            }
            else
            {
                anim.SetFloat("Turns", 0);
            }

            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
            {
                anim.SetFloat("Speed", Input.GetAxis("Vertical"));
            }
            else
            {
                anim.SetFloat("Speed", 0);
            }


            if (Input.GetMouseButtonDown(0))
            {
                anim.SetBool("shoot", true);
                StartCoroutine(StartDelay());
            }

            if ((Input.GetKeyDown("p"))) //death
            {
                anim.SetBool("die", true);
                this.enabled = false;
            } 

            if ((Input.GetKeyDown("u"))) //look around
            {
                anim.SetBool("look1", true);
            } else { anim.SetBool("look1", false); }

            if ((Input.GetKeyDown("o"))) //take damage 1
            {
                anim.SetBool("hitLeft", true);
            } else { anim.SetBool("hitLeft", false); }
           
            if ((Input.GetKeyDown("i"))) //take damage 2
            {
                anim.SetBool("hitRight", true);
            } else { anim.SetBool("hitRight", false); }
        }

        Vector3 move = transform.forward * vertical +
               transform.right * horizontal;

        controller.Move(move * moveSpeed * Time.deltaTime);

        transform.Rotate(Vector3.up * mouseX * mouseSensitivity * Time.deltaTime);

        xRotation -= mouseY * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(particleDalay);
        var bulletL = Instantiate(bulletPrefab, bulletSpawnPointLeft.position, bulletSpawnPointLeft.rotation);
        bulletL.GetComponent<Rigidbody>().velocity = bulletSpawnPointLeft.forward * bulletSpeed;
        var bulletR = Instantiate(bulletPrefab, bulletSpawnPointRight.position, bulletSpawnPointRight.rotation);
        bulletR.GetComponent<Rigidbody>().velocity = bulletSpawnPointRight.forward * bulletSpeed;
    }
}

