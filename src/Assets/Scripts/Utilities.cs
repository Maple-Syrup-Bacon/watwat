using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static int NumberOfPlayers = 2;
    public static int WinScore = 5;

    public enum PowerupType
    {
        ExplodingFireball,
        Invincibility,
        SuperStrength,
        SuperSpeed
    }
}
