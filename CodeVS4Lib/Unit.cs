using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeVS4
{
    public interface IUnit
    {
        int Id { get; }
        EUnitType Type { get; }
        Point Point { get; }
        int Hp { get; }
    }

    public class Unit : IUnit
    {
        public Unit()
        {

        }

        public Unit(int id, EUnitType type, Point point, int Hp)
        {

        }

        public Unit(int id, EUnitType type, Point point) : this(id, type, point, GameConstant.GetDefaultHp(type)) { }

        public Point Point
        {
            get { throw new NotImplementedException(); }
        }

        public EUnitType Type
        {
            get { throw new NotImplementedException(); }
        }

        public int Id
        {
            get { throw new NotImplementedException(); }
        }

        public int Hp
        {
            get { throw new NotImplementedException(); }
        }
    }
}
