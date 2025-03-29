using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerTrail : MonoBehaviour
{
    public string noHitboxTag = "Player";
    public string hitboxTag = "MovingPlatform";
    public float trailLifetime = 0f;
    public float spawnInterval = 0.1f;
    public float startOpacity = 0.5f;
    private SpriteRenderer playerSprite;
    private List<GameObject> trailPool = new List<GameObject>();
    private List<TrailObject> objectsWithTrails = new List<TrailObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindTrailObjects();
        playerSprite = GetComponentInParent<SpriteRenderer>();
        StartCoroutine(SpawnTrails());
    }

        void FindTrailObjects()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(noHitboxTag);
        foreach (GameObject obj in objects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                objectsWithTrails.Add(new TrailObject(sr));
            }
        }

        objects = GameObject.FindGameObjectsWithTag(hitboxTag);
        foreach (GameObject obj in objects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            BoxCollider2D bc = obj.GetComponent<BoxCollider2D>();
            if (sr != null)
            {
                objectsWithTrails.Add(new TrailObject(sr, bc));
            }
        }
    }

    IEnumerator SpawnTrails()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            foreach (TrailObject trailObject in objectsWithTrails)
            {
                if (trailObject != null)
                {
                    CreateTrail(trailObject);
                }
            }
        }
    }

    void CreateTrail(TrailObject original)
    {
        GameObject trail = new GameObject("Trail");
        trail.transform.SetParent(gameObject.transform);
        SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
        trailRenderer.sortingOrder = -1;
        trailRenderer.sprite = original.spriteRenderer.sprite;
        trailRenderer.color = new Color(original.spriteRenderer.color.r, original.spriteRenderer.color.g, original.spriteRenderer.color.b, startOpacity);
        trail.transform.position = original.spriteRenderer.transform.position;
        trail.transform.rotation = original.spriteRenderer.transform.rotation;
        if (original.collider != null) {
            BoxCollider2D collider = trail.AddComponent<BoxCollider2D>();
            collider.size = original.collider.size;
            collider.offset = original.collider.offset;
        }

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

    private class TrailObject
    {
        public SpriteRenderer spriteRenderer;
        public BoxCollider2D collider;

        public TrailObject(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
        }

        public TrailObject(SpriteRenderer spriteRenderer, BoxCollider2D collider)
        {
            this.spriteRenderer = spriteRenderer;
            this.collider = collider;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


