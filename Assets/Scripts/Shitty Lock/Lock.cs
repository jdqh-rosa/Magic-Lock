using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    Dial dial;
    const int defaultDialAmount = 1;

    public int GetDialNumeral()
    {
        return dial.numeral;
    }
    public void TurnDialLeft(int moveDialAmount = defaultDialAmount)
    {
        dial.MoveDial(moveDialAmount);
    }

    public void TurnDialRight(int moveDialAmount = defaultDialAmount)
    {
        dial.MoveDial(-moveDialAmount);
    }
}
