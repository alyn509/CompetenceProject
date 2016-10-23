using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicMovement : MonoBehaviour
{
    public float moveSpeed = 6.0F;
    public float rotateSpeed = 100.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 rotateDirection = Vector3.zero;
    CharacterController controller;
    public GameObject gunObject;
    public bool hasGun = false;
    public GameObject gotGunParticles;
    public int Score;
    public int life = 3;
    public List<GameObject> bodyParts;
    List<Color> normalColours = new List<Color>();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        foreach (GameObject model in bodyParts)
        {
            normalColours.Add(model.GetComponent<Renderer>().material.color);
        }
    }

    void Update()
    {
        controller.Move(moveDirection * Time.deltaTime);
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0);
    }

    public void PickupItem(ItemType item)
    {

        //get music manager, play pick-up sound.

        if (item == ItemType.Gun)
        {
            hasGun = true;//allow shooting animation
            ActivateGun();
        }
    }

    public void addPoints(int points)
    {

        //get music manager, play points given sound.
        Score += points;  
    }

    public void Hit()
    {
        life--;
        if (life > 0) {
            StartCoroutine(Flasher());

            //hit back?
        } else
        {
            //play death animation/pausing, etc.
        }

    }

    IEnumerator Flasher()
    {
        for (int i = 0; i < 3; i++)
        {

            for (int k = 0; k < bodyParts.Count; k++) {
                bodyParts[k].GetComponent<Renderer>().material.color = Color.red;
            }

            yield return new WaitForSeconds(.15f);

            for (int k = 0; k < bodyParts.Count; k++)
            {
                bodyParts[k].GetComponent<Renderer>().material.color = normalColours[k];
            }
        }
    }

    public void ActivateGun()
    {
        if (hasGun)
        {
            gunObject.SetActive(true);
            gotGunParticles.SetActive(true);
            StartCoroutine(LateCall());
        }
    }

    IEnumerator LateCall()
    {
        yield return new WaitForSeconds(5f);
        gotGunParticles.SetActive(false);
    }
}