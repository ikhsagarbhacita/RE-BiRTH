using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement movement;
    private Animator anim;
    private SpriteRenderer sprite;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float move = movement.MoveInput;

        anim.SetBool("isRunning", move != 0f);
        anim.SetFloat("animSpeed", movement.Speed / 5f);
        anim.SetBool("isJumping", !movement.isGround);

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
