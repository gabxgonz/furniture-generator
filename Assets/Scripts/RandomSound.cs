using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    [SerializeField] private List<AudioClip> sounds;
    [SerializeField] private int concurrentSounds = 1;
    private List<AudioSource> sources;
    private System.Random random;

    void Start()
    {
        sources = new List<AudioSource>();
        random = new System.Random();
        InitAudioSources();
        PlaySound();
    }

    public void PlaySound()
    {
        for (int i = 0; i < sources.Count; i++)
        {
            int randomIndex = random.Next(sounds.Count);
            sources[i].clip = sounds[randomIndex];
            sources[i].Play();
        }
    }

    private void InitAudioSources()
    {
        for (int i = 0; i < concurrentSounds; i++)
        {
            sources.Add(gameObject.AddComponent<AudioSource>());
        }
    }
}
