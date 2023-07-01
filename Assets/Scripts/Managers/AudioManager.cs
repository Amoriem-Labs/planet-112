using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource plainsSoundtrack;

    public AudioSource plantSFX;
    public AudioSource growingSFX;
    public AudioSource collectGenericSFX;
    public AudioSource collectFruitSFX;
    public AudioSource takeDamageSFX;

    public AudioSource[] soundtracks;
    public AudioSource[] SFXs;

    public float volumeBGM;
    public float volumeSFX;

    void Start(){
        soundtracks = new AudioSource[]{plainsSoundtrack};
        SFXs = new AudioSource[]{plantSFX, growingSFX, collectGenericSFX, collectFruitSFX, takeDamageSFX};
    }

    // Changes music soundtracks' volumes. Is triggered by music slider in settings.
    public void OnMusicVolumeChanged(float newValue){
        foreach (AudioSource music in soundtracks){
            music.volume = newValue;
        }
        volumeBGM = newValue;
    }

    // Changes SFX volumes. Is triggered by SFX slider in settings.
    public void OnSFXVolumeChanged(float newValue){
        foreach (AudioSource sfx in SFXs){
            sfx.volume = newValue;
        }
        volumeSFX = newValue;
    }
}
