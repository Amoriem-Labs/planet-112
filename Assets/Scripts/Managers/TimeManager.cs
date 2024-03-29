using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class TimeManager : MonoBehaviour
{
    GameStateData gameStateData; // grabbed from PD, automatically by reference
    public static int timeUnit = 1; // 1 second
    public static float gameTimeScale = 1f; // tune this down, game time counts faster. Tune this up, game time counts slower. 
    public CoroutineHandle timerHandle;

    public PersistentData persistentData;
    public GameObject autosaveCanvas; // Canvas that contains the autosave animation and text

    void Awake(){
        DontDestroyOnLoad(autosaveCanvas);
        autosaveCanvas.SetActive(false);
    }

    /* // Start is called before the first frame update
    void Start()
    {
        StartGameTimer();
    } */

    // This function should ONLY be called after save data has been retrieved. 
    // These methods don't need to be static. Can just reference in between not-changing managers.
    public void StartGameTimer()
    {
        gameStateData = PersistentData.GetGameStateData();

        // affected by time scale, cancels when object is destroyed
        timerHandle = Timing.CallPeriodically(float.PositiveInfinity, timeUnit * gameTimeScale, 
            delegate { CountTimeUnit(); Autosave(); }, gameObject);
    }

    void CountTimeUnit()
    {
        if (gameStateData.timePassedSeconds % 10 == 0){
            ////Debug.Log("Current time is: " + gameStateData.timePassedDays + " days, " + gameStateData.timePassedHours + " hours, "
            //+ gameStateData.timePassedMinutes + " minutes, " + gameStateData.timePassedSeconds + " seconds.");
        }

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
    }

    void Autosave(){
        if (gameStateData.timePassedSeconds % 60 == 0){
            persistentData.CreateNewSave(0);
            autosaveCanvas.SetActive(true);
            Invoke("HideAutosaveCanvas",2f);
            print("autosaving");
        }
    }

    void HideAutosaveCanvas(){
        autosaveCanvas.SetActive(false);
    }

    public static void PauseGame() // this method might be called by pause menu or other stuff, just in case static.
    {
        Time.timeScale = 0; // this pauses all coroutines and time-depedent behaviors. Idk if the best solution... 
        // other ideas if the above does work: start a coroutine that keeps all coroutines hostage, use if check statements
        // + isPaused bool for each coroutine (if paused, then wait until paused, a bit more work), or set TimeScale to
        // max float value and pray to god (worst idea). 

        // To ONLY pause this timer: Timing.PauseCoroutines(timerHandle);

        AudioListener.pause = true;
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        // To resume this timer if ONLY paused: Timing.ResumeCoroutines(timerHandle)
    }

    public static bool IsGamePaused()
    {
        return Time.timeScale == 0; 
    }

    // This function should ONLY be called before leaving the current save. For pausing, call the other ones above.
    public void KillGameTimer()
    {
        Timing.KillCoroutines(timerHandle);
    }
}
