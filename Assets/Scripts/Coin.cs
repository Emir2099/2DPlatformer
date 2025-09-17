using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip coinClip;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.coins += 1;
            player.PlaySFX(coinClip, 0.4f);
            Destroy(gameObject);
        }
    }
}
