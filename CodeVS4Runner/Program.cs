using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeVS4;

namespace CodeVS4Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            IPlayer ai = new SampleAI();
            var stream = new Stream(ai.Name);
            while (true)
            {
                var input = stream.Read();
                if (input == null)
                {
                    break;
                }

                var output = ai.Think(input);
                stream.Write(output);
            }
        }
    }
}
