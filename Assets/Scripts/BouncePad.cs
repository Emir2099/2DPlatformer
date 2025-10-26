using UnityEngine;
using System.Collections;


public class BouncePad : MonoBehaviour
{
    [Tooltip("Sprites that form the press animation. Order: 0->1->2->3")] 
    public Sprite[] sprites;

    [Tooltip("Seconds per frame for the press animation")]
    public float frameDuration = 0.05f;

    private SpriteRenderer sr;
    private Coroutine anim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    // Called by Player on collision to play the animation
    public void Trigger()
    {
        if (sprites == null || sprites.Length == 0 || sr == null)
            return;

        // If already animating, restart so player presses are responsive
        if (anim != null)
            StopCoroutine(anim);

        anim = StartCoroutine(AnimatePress());
    }

    private IEnumerator AnimatePress()
    {
        int n = sprites.Length;
        if (n == 0) yield break;

        // forward: 0..n-1
        for (int i = 0; i < n; i++)
        {
            sr.sprite = sprites[i];
            yield return new WaitForSeconds(frameDuration);
        }

        // backward: n-2..0 (so the last sprite isn't duplicated twice)
        for (int i = n - 2; i >= 0; i--)
        {
            sr.sprite = sprites[i];
            yield return new WaitForSeconds(frameDuration);
        }

        anim = null;
    }
}
