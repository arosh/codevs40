using CodeVS4;
using CodeVS4.Ofuton;

namespace CodeVS4Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            IPlayer ai = new Ofuton();
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
