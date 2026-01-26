using System;

namespace TestProject.Enums
{
    [Flags]
    public enum StatType : long
    {
        HP = 1,
        MP = 2,
        ATK = 4,
        DEF = 8,
    }
}
