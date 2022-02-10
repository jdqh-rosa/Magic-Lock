using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Dial
{
    public int highestValue; //highest value on the dial
    public int numeral{ get; private set; } // the current number the dial is on;
    public void MoveDial(int amount)
    {
        numeral += amount;

        while(numeral < 0)
        {
            numeral = highestValue - numeral;
        }

        while (numeral > highestValue)
        {
            numeral = numeral - highestValue;
        }
    }
}
