using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSkill18
{
    public class Anagramy
    {
        // analizując przykąłdowe dane z zadania mam nadzieję, ze zrozumiałem poprawnie intencję algorytmu:
        // zwróć te wyrazy, z których liter w dowolnej kombinacji zbudowano wyrazy najliczniej występujące w ciągu wejściowym

        private readonly string[] _input = { "mapa", "kajak", "jak", "map", "jaka" };
        private readonly string[] _output = {"kajak", "jak", "jaka"};


        // mapa tablic bitowych (funkcja haszująca) - szybka i małe zajęcie pamieci ;-)

        // Wielkość mapy bitowej... mały zonk...o ile polski alfabet (https://pl.wikipedia.org/wiki/Alfabet_polski) zawiera 32 litery, więc słowo wydawałoby się OK,
        // ale problemem jest przeliczanie i litery zapożyczone (Q, V, X) oraz takie a nie inne upakowanie polskich liter w tablicy ASCII / Unicode...
        // Proste haszowanie odpada ;-(

        // pozostaje zbudowanie rzadkiej tablicy mapującej znaki na indeksy i wówczas uda się kompresja na 64 bitach w ulong;
        // pewnym problemem jest też sam unicode, który rozpycha alfabet od znaku 97 do 380 co bezpośrednio dałoby zbyt wiele bitów by upchnąć w czymkolwiek dostępnym w standardzie

        private readonly ulong[] _hashIndex;

        public Anagramy()
        {
            // te same flagi bitowe dla małych i wielkich liter, będzie można nie robić .ToLower() przed liczeniem hasza i zaoszczędzić K
            var pary = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToCharArray()
                .Select(ch => (int)ch)
               .ToArray();
            var paryBig = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToUpper().ToCharArray()
                .Select(ch => (int)ch)
                .ToArray();

            _hashIndex = new ulong[Math.Max(paryBig.Max(p => p), pary.Max(p => p)) + 1];
            for(int i = 0; i < pary.Length; i++)
            {
                this._hashIndex[pary[i]] = (ulong)Math.Pow(2, i);
            }
            for(int i = 0; i < paryBig.Length; i++)
            {
                this._hashIndex[paryBig[i]] = (ulong)Math.Pow(2, i);
            }
        }

        public IEnumerable<string> ZwróćNajliczniejszeAnagramy(IReadOnlyCollection<string> input)
        {
            Dictionary<ulong, uint> lista = new Dictionary<ulong, uint>();

            // pierwszy przebieg - zliczanie elementów
            foreach (string s in input)
            {
                ulong h = LiczHasz(s);
                if (lista.ContainsKey(h))
                {
                    lista[h] += 1;
                }
                else
                {
                    lista.Add(h, 1);
                }
            }

            // sortowanie listy - i wybór najczęstrzego hasza
            var wygranyHash = lista.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).FirstOrDefault();

            // drugi przebieg - zwracanie elementów o wskazanym haszu
            foreach (string s in input)
            {
                ulong h = LiczHasz(s);
                if (h == wygranyHash)
                {
                    yield return s;
                }
            }
        }

        private ulong LiczHasz(string s)
        {
            return s.ToCharArray().Select(ch => this._hashIndex[(int) ch])
                .Aggregate((ulong)0, (a, v) => a |= v);
        }

        [Test]
        public void test_bazowy()
        {
            var outp = ZwróćNajliczniejszeAnagramy(this._input).ToArray();

            CollectionAssert.AreEqual(this._output, outp);

        }
    }
}
