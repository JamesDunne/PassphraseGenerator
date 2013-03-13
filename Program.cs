using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellDunne
{
    class Program
    {
        static void Main(string[] args)
        {
            var validOpts = PassphrasePolicy.Default;

            for (int i = 0; i < 1000; ++i)
            {
                string pp = PassphraseGenerator.Generate(validOpts);
                Console.WriteLine(pp);
            }
        }
    }
}
