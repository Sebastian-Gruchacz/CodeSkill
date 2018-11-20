﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CodeSkill117.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var deklaracje = new NodeDesc[]
            {
                new NodeDesc(name: "a" , parent: null, value: null),
                new NodeDesc(name: "b", value: 1, parent: "a"),
                new NodeDesc(name: "bb", parent: "a", value: null),
                new NodeDesc(name: "c", value: 1, parent: "b"),
                new NodeDesc(name: "bbb", value: 3, parent: "a")
            };

            var obj = JsonHelper.Construct(deklaracje);
            Console.WriteLine(obj.ToString());

            Console.WriteLine("Hello World!");
        }
    }

    public static class JsonHelper
    {
        public static JObject Construct(IEnumerable<NodeDesc> descriptors)
        {
            var allNodes = BuildTree(descriptors);

            // Bieżemy wszystkie rootty z kolekcji na alementy kolekcji najwyzszego poziomu.
            var roots = allNodes.Values.Where(v => v.Parent == null).ToDictionary(r => r.Name, r => r);

            // w zasadzie to już można by zwrócić bo drzewo jest OK, chociaż zaśmiecone w stosunku do oczekiwanego JSONa ;-)

            // konwersja drzewa na JObject

            return JObject.FromObject(roots); //TODO: custom printer - need a bit special behavior on naming nodes and hiding elements
        }

        private static Dictionary<string, Node> BuildTree(IEnumerable<NodeDesc> descriptors)
        {
            // Ze względu na sposób budowania drzewa (odwołanie do rodzica w globalnej przestrzeni) - wszystkie węzły muszą mieć unikalne nazwy...
            // Korzystajac z tego faktu, - aby uniknąć ciągłego przeszukiwania drzewa użyję słownika (mapy) do szybkiej budowy relacji
            // przy okazji - zakładam porównywanie case-sensitive. Jak coś - można użyć innych struktur

            Dictionary<string, Node> allNodes = new Dictionary<string, Node>();

            // Hashset do wykluczania dubli... Pewnie można by inaczej
            HashSet<string> declaredNodeNames = new HashSet<string>();

            // budowanie drzewa z wejścia
            foreach (var desc in descriptors)
            {
                if (declaredNodeNames.Contains(desc.Name))
                {
                    throw new ArgumentException($"Element '{desc.Name}' został zdefiniowany więcej niż raz.");
                }

                declaredNodeNames.Add(desc.Name);

                if (desc.Parent == null)
                {
                    allNodes.Add(desc.Name,
                        new Node {Name = desc.Name, Parent = null, Children = new List<Node>(), Value = null});
                }
                else
                {
                    // kolejność węzłów może być z czapy - czyli w danym momencie rodzica może jeszcze nie być.
                    // dlatego można by go stworzyć z automatu (i dzięki temu kolejność deklaracji nie jest istotna ;-)):
                    var parent = GetOrCreateNode(allNodes, desc.Parent);

                    if (allNodes.TryGetValue(desc.Name, out var existingNode))
                    {
                        // właśnie natknęliśmy się na wcześniej automatycznie zbudowanego rodzica (duplikat odrzuciliśmy wcześniej),
                        // uzupełnijmy go o inne parametry...
                        existingNode.Parent = parent;
                        existingNode.Value = desc.Value;
                        existingNode.Children.Add(parent);
                    }
                    else
                    {
                        var node = new Node
                        {
                            Name = desc.Name,
                            Parent = parent,
                            Children = new List<Node>(),
                            Value = desc.Value
                        };

                        allNodes.Add(node.Name, node);
                        parent.Children.Add(node);
                    }
                }
            }

            return allNodes;
        }

        private static Node GetOrCreateNode(Dictionary<string, Node> allNodes, string nodeName)
        {
            if (!allNodes.TryGetValue(nodeName, out var node))
            {
                node = new Node
                {
                    Name = nodeName,
                    Parent = null,
                    Children = new List<Node>(),
                    Value = null
                };

                allNodes.Add(node.Name, node);
            }

            return node;
        }
    }

    public class Node
    {
        public string Name;
        [JsonIgnore]
        public Node Parent;
        public List<Node> Children;
        public int? Value;
    }

    public struct NodeDesc
    {
        public string Name;
        public string Parent;
        public int? Value;

        public NodeDesc(string name, string parent, int? value)
        {
            Name = name;
            Parent = parent;
            Value = value;
        }
    }
}
