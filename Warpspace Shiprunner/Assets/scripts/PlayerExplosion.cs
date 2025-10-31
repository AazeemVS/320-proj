using UnityEngine;

public class PlayerExplosion : Bullet
{
    private float lifetime;
    public float lifeTimeMax;
    public float size;
    SpriteRenderer spriteRenderer;
    Color c = Color.white;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set piercing to max so we can rely on base bullet code for collision without rewriting it
        piercing = 100;
        lifetime = lifeTimeMax;
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("new explosion just showed up");
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        c.a = lifetime / lifeTimeMax;
        spriteRenderer.color = c;
        if(lifetime <= 0) { Destroy(gameObject); }

    }
}
