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
    [SerializeField] float stageTimeMax = 30; // time until growth to next stage

    // private
    SpriteRenderer spriteRenderer;

    PlantData plantData;

    public void InitializePlant(PlantData newData)
    {
        //Assign data
        plantData = newData;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        plantData.stageTimeLeft = stageTimeMax;
    }

    private void Update()
    {
        if (plantData.stageTimeLeft > 0)
        {
            plantData.stageTimeLeft -= Time.deltaTime;
        }
        else
        {
            IncrementState();
            plantData.stageTimeLeft = stageTimeMax;
        }
    }

    public void IncrementState()
    {
        if (currentStage != maxStage)
        {
            currentStage++;
            ChangeSprite();
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
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Add(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Remove(this);
        }
    }
}
