using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement movement;
    private Animator anim;
    private SpriteRenderer sprite;
    private bool hasFlyParam; 

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        // Cek parameter "isFlying" supaya tidak spam warning kalau belum dibuat di Animator
        if (anim.runtimeAnimatorController != null)
        {
            foreach (var p in anim.parameters)
                if (p.name == "isFlying") hasFlyParam = true;
        }
    }

    private void Update()
    {
        float move = movement.MoveInput;

        anim.SetBool("isRunning", move != 0f);
        anim.SetFloat("animSpeed", movement.Speed / 7f);
        anim.SetBool("isJumping", !movement.isGround);
        if (hasFlyParam) anim.SetBool("isFlying", movement.isFlying); 

        if (move > 0)
        {
            sprite.flipX = false;
        }
        if (move < 0)
        {
            sprite.flipX = true;
        }
    }
}
