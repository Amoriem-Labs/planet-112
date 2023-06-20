using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource plantSFX;
    public AudioSource growingSFX;
    public AudioSource collectGenericSFX;
    public AudioSource collectFruitSFX;
    public AudioSource takeDamageSFX;
    public AudioSource[] SFXs;
    public AudioSource[] musics;

    void Start(){
        SFXs = new AudioSource[]{plantSFX, growingSFX, collectGenericSFX, collectFruitSFX, takeDamageSFX};
        musics = new AudioSource[]{}; // fill in with music sounds later
    }

    // Changes music soundtracks' volumes. Is triggered by music slider in settings.
    public void OnMusicVolumeChanged(float newValue){
        foreach (AudioSource music in musics){
            music.volume = newValue;
        }
    }

    // Changes SFX volumes. Is triggered by SFX slider in settings.
    public void OnSFXVolumeChanged(float newValue){
        foreach (AudioSource sfx in SFXs){
            sfx.volume = newValue;
        }
    }
}
