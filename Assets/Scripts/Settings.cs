using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public bool fullScreen;
    public GameObject player;
    public float y_offset;

    void Start(){
        fullScreen = false;
    }

    // This script is to update the position of SettingsCanvas to match player position when settings are loaded.
    public void setPosition()
    {
        var pos = transform.position;
        pos.x = player.transform.position.x;
        pos.y = player.transform.position.y + y_offset;
        transform.position = pos;
    }

    public void setFullScreen(){
        if( fullScreen ) {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            fullScreen = true;
        }
        else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullScreen = false;
        }
    }
}
