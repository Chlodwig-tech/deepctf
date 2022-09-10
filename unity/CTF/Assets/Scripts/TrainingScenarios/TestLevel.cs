using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TestLevel
{
    public abstract (bool, bool, bool, bool) Run();
    public virtual void Spawn() {}
}
