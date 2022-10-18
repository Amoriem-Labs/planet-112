using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    // public variables
    public int currentStage = 0;
    public int maxStage = 2;

    // serialized
    [SerializeField] Sprite[] spriteArray;
    [SerializeField] float stageTimeMax = 30f; // time until growth to next stage

    // private
    PlayerScript playerScript;
    SpriteRenderer spriteRenderer;
    float stageTimeLeft;


    private void Awake()
    {
        playerScript = Object.FindObjectOfType<PlayerScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stageTimeLeft = stageTimeMax;

    }

    private void Update()
    {
        if (stageTimeLeft > 0)
        {
            stageTimeLeft -= Time.deltaTime;
        }
        else
        {
            Debug.Log("stageTimeLeft is up!");
            IncrementState();
            stageTimeLeft = stageTimeMax;
        }
    }

    public void IncrementState()
    {
        if (currentStage != maxStage)
        {
            currentStage++;
            ChangeSprite();
            Debug.Log("IncrementState() called!");
        }
    }

    void ChangeSprite()
    {
        spriteRenderer.sprite = spriteArray[currentStage];
    }
}
