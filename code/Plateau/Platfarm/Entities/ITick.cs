﻿using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Entities
{
    interface ITick
    {
        void Tick(int minutesTicked, EntityPlayer player, Area area, World world);
    }
}
