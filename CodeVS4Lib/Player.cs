using System;
using System.Collections.Generic;

namespace CodeVS4
{
    public interface IPlayer
    {
        IEnumerable<IOrder> Think(Input input);
        string Name { get; }
    }

    public class Player : IPlayer
    {
        public IEnumerable<IOrder> Think(Input input)
        {
            throw new NotImplementedException();
        }

        public string Name { get; private set; }
    }
}
