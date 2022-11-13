using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    GameStateData gameStateData; // grabbed from PD, automatically by reference
    public static int timeUnit = 1; // 1 second
    public static float gameTimeScale = 1f; // tune this down, game time counts faster. Tune this up, game time counts slower. 
    public IEnumerator t = null;

    // Start is called before the first frame update
    void Start()
    {
        StartGameTimer();
    }

    // This function should ONLY be called after save data has been retrieved. 
    // These methods don't need to be static. Can just reference in between not-changing managers.
    public void StartGameTimer()
    {
        gameStateData = PersistentData.GetGameStateData();
        t = CountTimeUnit();
        StartCoroutine(t);
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

        Debug.Log("Current time is: " + gameStateData.timePassedDays + " days, " + gameStateData.timePassedHours + " hours, "
        + gameStateData.timePassedMinutes + " minutes, " + gameStateData.timePassedSeconds + " seconds.");

        t = CountTimeUnit();
        StartCoroutine(t); // can store this in a variable, but don't see a need to do so rn so...
    }

    public static void PauseGame() // this method might be called by pause menu or other stuff, just in case static.
    {
        Time.timeScale = 0; // this pauses all coroutines and time-depedent behaviors. Idk if the best solution... 
        // other ideas if the above does work: start a coroutine that keeps all coroutines hostage, use if check statements
        // + isPaused bool for each coroutine (if paused, then wait until paused, a bit more work), or set TimeScale to
        // max float value and pray to god (worst idea). 
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public static bool IsGamePaused()
    {
        return Time.timeScale == 0; 
    }

    // This function should ONLY be called before leaving the current save. For pausing, call the other ones above.
    public void KillGameTimer()
    {
        if(t != null)
        {
            StopCoroutine(t);
            t = null;
        }
    }
}
