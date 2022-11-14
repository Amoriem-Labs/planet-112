using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Plant module interfaces (can be customized to include new functions)
// Plant modules define a single behavior of a plant type.
public interface IProduce
{
    void Produce();
}

public interface IAttack
{
    void Attack();
}

public interface IDefend
{
    void Defend();
}

public interface ISupport 
{
    void Support();
}
