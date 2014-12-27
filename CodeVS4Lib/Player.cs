﻿using System;
using System.Collections.Generic;

namespace CodeVS4
{
    interface IPlayer
    {
        IEnumerable<IOrder> Think(IEnumerable<IUnit> my, IEnumerable<IUnit> en, IEnumerable<Point> resources);
    }

    class Player : IPlayer
    {
        public IEnumerable<IOrder> Think(IEnumerable<IUnit> my, IEnumerable<IUnit> en, IEnumerable<Point> resources)
        {
            throw new NotImplementedException();
        }
    }
}
