using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    GameStateData gameStateData; // grabbed from PD, automatically by reference
    public static int timeUnit = 1; // 1 second
    public static float gameTimeScale = 1f; // tune this down, game time counts faster. Tune this up, game time counts slower.

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // This function should ONLY be called after save data has been retrieved. 
    public void StartGameTimer()
    {
        gameStateData = PersistentData.GetGameStateData();
        StartCoroutine(CountTimeUnit());
    }

    IEnumerator CountTimeUnit()
    {
        yield return new WaitForSeconds(timeUnit * gameTimeScale); // affected by time scale

        gameStateData.timePassedSeconds += timeUnit;
        if(gameStateData.timePassedSeconds >= 60)
        {
            gameStateData.timePassedMinutes += 1;
            gameStateData.timePassedSeconds -= 60; // same thing as mod
            if (gameStateData.timePassedMinutes >= 60)
            {
                gameStateData.timePassedHours += 1;
                gameStateData.timePassedMinutes -= 60;
                if (gameStateData.timePassedHours >= 24)
                {
                    gameStateData.timePassedDays += 1;
                    gameStateData.timePassedHours -= 24;
                }
            }
        }

        StartCoroutine(CountTimeUnit()); // can store this in a variable, but don't see a need to do so rn so...
    }

    

    
}
