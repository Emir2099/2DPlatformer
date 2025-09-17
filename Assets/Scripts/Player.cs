using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int coins;
    public int health = 100;
    public float jumpForce = 10f;
    public float moveSpeed = 5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Image healthImage;

    public AudioClip jumpClip;
    public AudioClip hurtClip;

    private Rigidbody2D rb;
    private bool isGrounded;

    private AudioSource audioSource;
    private Animator animator;

    private SpriteRenderer spriteRenderer;

    public int extraJumpValues = 1;
    private int extraJumps;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        extraJumps = extraJumpValues;

        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        Debug.Log(isGrounded);

        if (isGrounded)
        {
            extraJumps = extraJumpValues;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // Apply jump by setting the upward velocity
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                PlaySFX(jumpClip);
            }
            else if (extraJumps > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                extraJumps--;
                PlaySFX(jumpClip);
            }
        }

        SetAnimations(moveInput);
        healthImage.fillAmount = health / 100f;
    }

    private void FixedUpdate()
    {
        // Update the field (do not shadow it with a local variable)
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = false;
        }
    }

    private void SetAnimations(float moveInput)
    {
        if (isGrounded)
        {
            if (moveInput == 0)
            {
                animator.Play("Player_Idle");
            }
            else
            {
                animator.Play("Player_Run");
            }
        }
        else
        {
            if (rb.velocity.y > 0)
            {
                animator.Play("Player_Jump");
            }
            else
            {
                animator.Play("Player_Fall");
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            PlaySFX(hurtClip);
            health -= 25;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            StartCoroutine(FlashRed());
            if (health <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1f)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Strawberry")
        {
            health += 20;
            if (health > 100) health = 100;
            Destroy(collision.gameObject);
        }
    }
}

// create -> 2D -> Tile Palette -> Rectangle 
// Click on window -> 2D -> Tile Palette
// Drag and drop the platform combined sprite to the Tile Palette
// In hierarchy create empty game object -> name it "Ground" 
// Add 2D as child of Ground -> Tilemap -> Rectangular
// Select the Tile Palette window -> select the brush tool -> select the platform sprite -> paint on the Tilemap in the scene view