using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeVS4
{
    interface IPlayer
    {
        IUnit Castle { get; }
        IEnumerable<IOrder> NextStrategy(IEnumerable<IUnit> my, IEnumerable<IUnit> enemies, IEnumerable<Point> resources);
    }

    class Player : IPlayer
    {

        public IUnit Castle
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IOrder> NextStrategy(IEnumerable<IUnit> my, IEnumerable<IUnit> enemies, IEnumerable<Point> resources)
        {
            throw new NotImplementedException();
        }
    }
}
