using UnityEngine;

public class PlayerExplosion : Bullet
{
    private float lifetime;
    public float lifeTimeMax;
    public float size;
    SpriteRenderer spriteRenderer;
    Color c = Color.white;
    protected AudioManager audioManager;
    bool playedSound = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set piercing to max so we can rely on base bullet code for collision without rewriting it
        piercing = 100;
        lifetime = lifeTimeMax;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioManager = GetComponentInChildren<AudioManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playedSound) {
            audioManager.PlaySound(SoundID.Explosion);
            playedSound = true;
        }
        lifetime -= Time.deltaTime;
        c.a = lifetime / lifeTimeMax;
        spriteRenderer.color = c;
        if(lifetime <= 0) { Destroy(gameObject); }

    }
}
