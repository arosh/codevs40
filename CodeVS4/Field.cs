using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeVS4
{
    public interface IField
    {
        IEnumerable<IUnit> GetViewFromPlayer(/* 引数をどうするか迷い中 */);
    }

    public class Field : IField
    {
        private const int Size = 100;

        public Field()
        {
        }

        public IEnumerable<IUnit> GetViewFromPlayer()
        {
            throw new NotImplementedException();
        }
    }
}
