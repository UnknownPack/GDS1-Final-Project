using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrail : MonoBehaviour
{

    public float trailLifetime = 0f;
    public float spawnInterval = 0.1f;
    private SpriteRenderer playerSprite;
    private List<GameObject> trailPool = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerSprite = GetComponentInParent<SpriteRenderer>();
        StartCoroutine(SpawnTrail());
    }

    IEnumerator SpawnTrail()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            CreateTrail();
        }
    }

    void CreateTrail()
    {
        GameObject trail = new GameObject("Trail");
        SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = playerSprite.sprite; // Use the sprite of the parent object
        trail.transform.position = transform.position;
        trail.transform.rotation = transform.rotation;
        
        // Start fading the trail
        StartCoroutine(FadeTrail(trailRenderer));
    }

    IEnumerator FadeTrail(SpriteRenderer trailRenderer)
    {
        Color color = trailRenderer.color;
        float time = 0;

        while (time < trailLifetime)
        {
            color.a = Mathf.Lerp(0.1f, 0, time / trailLifetime);  // Fade out over time
            trailRenderer.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(trailRenderer.gameObject); // Destroy the trail after it fades
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
