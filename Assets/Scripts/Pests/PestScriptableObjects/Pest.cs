using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pest", menuName = "Pest")]
// Blueprint for easily designing new pest types
public class Pest : ScriptableObject
{
    // Member variables that define pest properties. To be set by children. They don't have to all be filled! Only fill the parts that are needed for the module.
    public PestName pName;

    // currStageOfLife is the accessing index to everything below. Stage 0 is default, everything builds on this.
    public Sprite[] spriteArray; // Array of sprites per each growth stage
    public float[] maxHealth; // Max HP for each stage

    public PestModuleEnum[] defaultModules; // default modules to this class of pests
    public Vector2[] hitboxSize; // dimension of the 2D box physical collider of this pest
    public Vector2[] hitboxOffset; // offset of the 2D box physical collider of this pest from bottom center

    // Modules can subscribe to these delegates to react to changes when called. 
    public delegate void OnPestStageUpdateDelegate();
    public OnPestStageUpdateDelegate pestStageUpdateDelegate;

    // Module data
    // For SingleTargetProjectileAttackModule
    public float[] singleTargetProjectileAttackRate;
    public float[] singleTargetProjectileDamageAmount;
    public float[] singleTargetProjectileSpeed; // going to assume projectile collider and sprite is fixed. Put the update via diff proj. prefabs if needed
    public float[] singleTargetProjectileAttackRangeRadius;
}
