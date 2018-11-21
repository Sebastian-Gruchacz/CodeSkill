using System;

namespace CodeSkill17.TestApp
{
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            //var deklaracje = new NodeDesc[]
            //{
            //    new NodeDesc(name: "a" , parent: null, value: null),
            //    new NodeDesc(name: "b", value: 1, parent: "a"),
            //    new NodeDesc(name: "bb", parent: "a", value: null),
            //    new NodeDesc(name: "c", value: 1, parent: "b"),
            //    new NodeDesc(name: "bbb", value: 3, parent: "a")
            //};

            //var obj = JsonHelper.Construct(deklaracje);
            //Console.WriteLine(obj.ToString());



            var tabs = new[]
            {
                new string[] {"100", "222", "3"},
                new string[] {"100", "111", "1111"},
                new string[] {"100", "111", "2222"},
                new string[] {"100", "111", "333", "333"},
                new string[] {"200", "555", "2222"},
                new string[] {"200", "555", "333"},
                new string[] {"777"}
            };

            var stack = new string[tabs.Select(t => t.Length).Max()];
            foreach (var tab in tabs)
            {
                for (int i = 0; i < tab.Length; i++)
                {
                    var s = tab[i];

                    if (stack[i] != s)
                    {
                        stack[i] = s;
                    }
                    else
                    {
                        continue;
                    }

                    Console.WriteLine(new string('-', i * 3) + s);
                }

                stack[tab.Length - 1] = null;
            }


            Console.WriteLine("Press ENTER");
            Console.ReadLine();
        }
    }
}
