using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Hack to serialize a Dictionary in Inspector: Use a list of structures and then populate a dictionary in Awake with the variables of the structure.
    // The reason why we do this over serializing a list in the Inspector is because now we don't have to worry about matching indices when referencing the list.
    [Serializable]
    public struct SoundtrackStruct {
        public string name;
        public AudioSource soundtrack;
    }
    public SoundtrackStruct[] soundtracksInit;

    [Serializable]
    public struct SFXStruct {
        public string name;
        public AudioSource SFX;
    }
    public SFXStruct[] sfxsInit;

    public static Dictionary<string, AudioSource> soundtracks;
    public static Dictionary<string, AudioSource> SFXs;

    public float volumeBGM;
    public float volumeSFX;

    void Awake() 
    {
        DontDestroyOnLoad(gameObject);
        soundtracks = new Dictionary<string, AudioSource>();
        SFXs = new Dictionary<string, AudioSource>();
        
        // Populating soundtrack and SFX dictionaries according to hack
        foreach (SoundtrackStruct soundtrackStruct in soundtracksInit){
            soundtracks[soundtrackStruct.name] = soundtrackStruct.soundtrack;
        }
        foreach (SFXStruct sfxStruct in sfxsInit){
            SFXs[sfxStruct.name] = sfxStruct.SFX;
        }
    }

    // Changes music soundtracks' volumes. Is triggered by music slider in settings.
    public void OnMusicVolumeChanged(float newValue){
        foreach (KeyValuePair<string, AudioSource> entry in soundtracks){
            entry.Value.volume = newValue;
        }
        volumeBGM = newValue;
    }

    // Changes SFX volumes. Is triggered by SFX slider in settings.
    public void OnSFXVolumeChanged(float newValue){
        foreach (KeyValuePair<string, AudioSource> entry in SFXs){
            entry.Value.volume = newValue;
        }
        volumeSFX = newValue;
    }

    public static AudioSource GetSoundtrack(string soundtrackName){
        return soundtracks[soundtrackName];
    }

    public static AudioSource GetSFX(string SFXName)
    {
        return SFXs[SFXName];
    } 
}
