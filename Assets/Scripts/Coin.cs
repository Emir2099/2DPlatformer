using UnityEngine;
using TMPro;
public class Coin : MonoBehaviour
{
    public AudioClip coinClip;
    private TextMeshProUGUI coinText;

    private void Start()
    {
        coinText = GameObject.Find("CoinText").GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.coins += 1;
            player.PlaySFX(coinClip, 0.4f);
            coinText.text = player.coins.ToString();
            Destroy(gameObject);
        }
    }
}
