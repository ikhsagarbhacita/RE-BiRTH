using System;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 30f;
    //public float fireRate = 0.2f;

    private float fireCooldown;
    private PlayerAim aim;
    private InputSystem_Actions actions;

    private void Awake()
    {
        aim = GetComponent<PlayerAim>();
        actions = new InputSystem_Actions();
    }

    private void Update()
    {
        //fireCooldown -= Time.deltaTime;

        if(actions.Player.Attack.IsPressed())
        {
            Shoot();
            //fireCooldown = fireRate;
        }
    }

    private void OnEnable()
    {
        actions.Player.Enable();
    }
    private void OnDisable()
    {
        actions.Player.Disable();
    }

    private void Shoot()
    {
        Vector2 shootDirection = aim.clampedOffset.normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;

        float range = aim.limitOffset.magnitude;
        Destroy(bullet, range / bulletSpeed);
    }
}
