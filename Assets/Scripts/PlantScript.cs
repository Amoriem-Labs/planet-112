using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    // public variables
    public int currentStage = 0;
    public int maxStage = 2;
    public int attackers = 0;
    public int maxAttackers = 3;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Replace with gameManager static singleton for simplicity
            //Actually use FindObjectOfType of gameObject, are we using a game manager?
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Add(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Replace with gameManager static singleton for simplicity
            //Actually use FindObjectOfType of gameObject, are we using a game manager?
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Remove(this);
        }
    }
}
