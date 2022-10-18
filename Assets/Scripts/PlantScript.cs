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

    // private
    PlayerScript playerScript;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerScript = Object.FindObjectOfType<PlayerScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
