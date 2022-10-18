using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    PlayerScript playerScript;
    [SerializeField] Sprite[] spriteArray;
    
    SpriteRenderer spriteRenderer;
    public int currentStage = 0;
    int maxStage = 2;

    private void Awake()
    {
        playerScript = Object.FindObjectOfType<PlayerScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerScript.plantScript = this;
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
