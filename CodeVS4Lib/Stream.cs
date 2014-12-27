using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CodeVS4
{
    public class Input
    {
        public int RemainMs { get; private set; }
        public int Stage { get; private set; }
        public int Turn { get; private set; }
        public int Resource { get; private set; }
        public IEnumerable<IUnit> MyUnits { get; private set; }
        public IEnumerable<IUnit> EnUnits { get; private set; }
        public IEnumerable<Point> ResourcePoints { get; private set; }
        public Input(int remainMs, int stage, int turn, int resource, IEnumerable<IUnit> myUnits, IEnumerable<IUnit> enUnits, IEnumerable<Point> resourcePoints)
        {
            RemainMs = remainMs;
            Stage = stage;
            Turn = turn;
            Resource = resource;
            MyUnits = myUnits;
            EnUnits = enUnits;
            ResourcePoints = resourcePoints;
        }
    }

    public class Stream
    {
        private TextReader In = Console.In;
        private TextWriter Out = Console.Out;

        public Stream(string name)
        {
            Out.WriteLine(name);
            Out.Flush();
        }

        public Input Read()
        {
            int remainMs = int.Parse(In.ReadLine());
            int stage = int.Parse(In.ReadLine());
            int turn = int.Parse(In.ReadLine());
            int resource = int.Parse(In.ReadLine());

            int n;

            n = int.Parse(In.ReadLine());
            var myUnits = new IUnit[n];
            for (int i = 0; i < n; i++)
            {
                myUnits[i] = StringToIUnit(In.ReadLine());
            }

            n = int.Parse(In.ReadLine());
            var enUnits = new IUnit[n];
            for (int i = 0; i < n; i++)
            {
                enUnits[i] = StringToIUnit(In.ReadLine());
            }

            n = int.Parse(In.ReadLine());
            var resourcePoints = new Point[n];
            for (int i = 0; i < n; i++)
            {
                resourcePoints[i] = StringToPoint(In.ReadLine());
            }

            return new Input(remainMs, stage, turn, resource, myUnits, enUnits, resourcePoints);
        }

        public void Write(IEnumerable<IOrder> orders)
        {
            Out.WriteLine(orders.Count());
            Debug.Assert(orders.Count() == orders.Select(order => order.UnitId).Distinct().Count());
            foreach (var order in orders)
            {
                Out.WriteLine(OrderToString(order));
            }
            Out.Flush();
        }

        public static IUnit StringToIUnit(string s)
        {
            int[] a = StringToIntArray(s);
            Debug.Assert(a.Length == 5);
            return new Unit(a[0], UnitTypeIntToEnum(a[4]), new Point(a[2], a[1]), a[3]);
        }

        public static Point StringToPoint(string s)
        {
            int[] a = StringToIntArray(s);
            Debug.Assert(a.Length == 2);
            return new Point(a[1], a[0]);
        }

        public static string OrderToString(IOrder order)
        {
            char[] orderChar = { 'U', 'D', 'L', 'R', '0', '1', '2', '3', '5', '6' };
            return string.Format("{0} {1}", order.UnitId, orderChar[(int)order.Type]);
        }

        public static int[] StringToIntArray(string s)
        {
            var delimiters = new[] { ' ' };
            return s.Split(delimiters).Select(value => int.Parse(value)).ToArray();
        }

        public static EUnitType UnitTypeIntToEnum(int i)
        {
            Debug.Assert(0 <= i && i <= 6);
            return (EUnitType)i;
        }

        public static int UnitTypeEnumToInt(EUnitType unitType)
        {
            return (int)unitType;
        }
    }
}
