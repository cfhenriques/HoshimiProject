using System;
using System.Collections.Generic;
using System.Text;

namespace Tutorials
{
    [PH.Common.Characteristics(
        ContainerCapacity = 100,
        CollectTransfertSpeed = 0,
        Scan = 0,
        MaxDamage = 5,
        DefenseDistance = 10,
        Constitution = 35)
    ]
    class Needle : PH.Common.NanoNeedle
    {
    }
}
