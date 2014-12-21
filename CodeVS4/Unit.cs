using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeVS4
{
    public enum EUnitType
    {
        Castle, Village, Base, Worker, Knight, Fighter, Assassin
    }

    public interface IUnit
    {
        IPoint Point { get; }
        EUnitType Type { get; }
        int Id { get; }
        int Hp { get; }
    }

    public class Unit : IUnit
    {
        public const int WorkerHp = 2000;
        public const int KnightHp = 5000;
        public const int FighterHp = 5000;
        public const int AssassinHp = 5000;
        public const int CastleHp = 50000;
        public const int VillageHp = 20000;
        public const int BaseHp = 20000;

        public const int WorkerAttack = 2;
        public const int KnightAttack = 2;
        public const int FighterAttack = 2;
        public const int AssassinAttack = 2;
        public const int CastleAttack = 10;
        public const int VillageAttack = 2;
        public const int BaseAttack = 2;

        public const int WorkerView = 4;
        public const int KnightView = 4;
        public const int FighterView = 4;
        public const int AssassinView = 4;
        public const int CastleView = 10;
        public const int VillageView = 10;
        public const int BaseView = 4;

        public const int WorkerCost = 40;
        public const int KnightCost = 20;
        public const int FighterCost = 40;
        public const int AssassinCost = 60;
        public const int CastleCost = 0;
        public const int VillageCost = 100;
        public const int BaseCost = 500;

        public IPoint Point
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
