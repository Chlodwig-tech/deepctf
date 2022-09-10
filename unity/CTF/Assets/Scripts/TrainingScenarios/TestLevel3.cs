using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel3 : TestLevel
{
    public override (bool, bool, bool, bool) Run()
    {
        bool W = false;
        bool S = false;
        bool A = false;
        bool D = false;
        return (W, S, A, D);
    }
}