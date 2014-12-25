using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeVS4
{
    enum EOrder
    {
        Move, Build, Attack
    }

    interface IOrder
    {
        EOrder Type { get; }
        EUnitType Target { get; }
    }

    class Order : IOrder
    {
        public EOrder Type
        {
            get { throw new NotImplementedException(); }
        }

        public EUnitType Target
        {
            get { throw new NotImplementedException(); }
        }
    }
}
