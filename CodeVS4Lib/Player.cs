using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeVS4
{
    interface IPlayer
    {
        IEnumerable<IOrder> NextStrategy(IEnumerable<IUnit> my, IEnumerable<IUnit> enemies, IEnumerable<Point> resources);
    }

    class Player : IPlayer
    {
        public IEnumerable<IOrder> NextStrategy(IEnumerable<IUnit> my, IEnumerable<IUnit> enemies, IEnumerable<Point> resources)
        {
            throw new NotImplementedException();
        }
    }
}
