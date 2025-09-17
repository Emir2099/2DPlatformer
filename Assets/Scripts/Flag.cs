using UnityEngine;

public class Flag : MonoBehaviour
{
    public GameObject winUI;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            winUI.SetActive(true);
            Time.timeScale = 0f; // Pause the game
        }
    }
}
