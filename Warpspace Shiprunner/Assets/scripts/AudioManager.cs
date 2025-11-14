using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum SoundID {
    PlayerShoot = 0,
    EnemyShoot = 1,
    Explosion = 2,
    EnemyDeath = 3,
    PlayerHit = 4,
    EnemyHit = 5,
    PlayerIFrame = 6,
    PlayerDash = 7,
    LaserCharge = 8,
    LaserFire = 9
}
public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] public List<AudioClip> sfxList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(SoundID sound) {
        audioSource.PlayOneShot(sfxList[(int)sound]);
    }
}
