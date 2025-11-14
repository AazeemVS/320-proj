using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    [SerializeField] GameObject AudioManagerPrefab;
    public List<GameObject> AudioList;
    public AudioManager am;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = AudioList.Count - 1; i >= 0; i--) {
            if (!AudioList[i].GetComponent<AudioSource>().isPlaying) {
                Destroy(AudioList[i]);
                AudioList.RemoveAt(i);
            }
        }
        
    }

    public void PlayClip(SoundID soundID) {
        GameObject newAudio = Instantiate(AudioManagerPrefab);
        AudioList.Add(newAudio);
        am = newAudio.GetComponent<AudioManager>();
        //cant use the actual audio manager play sound function because unity throws an unassigned error (even though I use the manager right here). Maybe something to do with instantiation order but idk
        newAudio.GetComponent<AudioSource>().PlayOneShot(am.sfxList[(int)soundID]);
        
    }
}
