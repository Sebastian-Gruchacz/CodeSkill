using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CodeSkill18
{
    // http://www.rjp.pan.pl/index.php?option=com_content&view=article&id=1047:ile-jest-sow-w-jzyku-polskim&catid=44&Itemid=145
    // Powiedzmy PL ma 150K słów, wliczajac gwarowe, zapożyczenia i odmiany, przyjmując średnio 10 znaków Unicode na wyraz, dostaniemy niespełna 3MB na same wyrazy.
    // Angielski moze mieć ich nawet jeszcze więcej - https://en.oxforddictionaries.com/explore/how-many-words-are-there-in-the-english-language/ ponad 170K
    // Można przyjąć optymistycznie, że plik wejściowy zawiera "sensowne" dane, czyli zbiór co najwyżej kilkuset wyrazów z jakiejś dzidziny, co by się jeszcze łądniej dało upchać w prostej HashMapie.
    // Co jednak jeśli na wejściu dostaniemy słownik właśnie? Kompresja będzie słaba...
    // Do tego pytanie o maksymalną zajętość pamięci w Mapie - struktury wewnętrzne, kubełki, hasze a wreszcie i same liczniki (uint wystarczy?). No ale w 100MB i tak spokojnie by się z tym dało zebrać.
    // Do tego implementacja z hasmapą byłaby dość szybka i bardzo czytelna, zwłaszcza wykorzystując systemowy typ Dictionary<string, uint> i już, bez wynajdywania koła na nowo.
    // Myk taki, ze implementacja mimo, że wydajna będzie lekko niefektywna ze względu na kilka razy liczony hash i 3 przeszukiwania mapy. 
    // Teoretycznie preszukanie mapy jest O(1), a dodawanie elemntu też (pod warunkiem, że nie tzreba jej przebudować, wówczas O(n). 
    // Moze z góry rezerwując 10K kubełków będzie OK?

    // Alternatywą wydaje się budowa drzewa (grafu bardziej chyba) rozpinającego o odpowiedniej wielkości.
    // Jego zbudowanie przeszukanie powinno być dość szybkie, pod warunkiem sprawnej funkcji indeksującej. Podobnej to tej z pierwszego zadania.
    // I pod warunkiem, że się nie rozrośnie niepotrzebnie...

    // Graf potencjalnie rozpinałby się z każdego węzła na maksymalnie 35 węzłów potomnych, przy średniej głębokości 10 poziomów (długość wyrazu),
    // 35^10? potencjalnie masakrycznie dużo...
    // przy czym, niektóre kombinacje nigdy nie wystąpią, wiec tablica podgrafów byłaby rzadka, może lepiej tu dać małe HashMapy - będzie szybciej szukać i mniej pamięci zeżrą?
    

    // I tak to wszystko teoria - bez pożądnych load-testów i tak diabli wiedzą jakie podejście byłoby najszybsze... Bo zasoby pamięci i tak są tu marginalne.
    public class LicznikWyrazów
    {
        // Prosta, czytelna i raczej dość szybka
        public IEnumerable<Tuple<string, uint>> ImplementacjaHaszMapą(IWordProvider dostawca)
        {
            Dictionary<string, uint> słowa = new Dictionary<string, uint>(10_000);
            while (dostawca.NextWord(out var słowo))
            {
                słowo = słowo.ToLower(); // tak sobie myślę, że to ma sens
                if (słowa.ContainsKey(słowo))
                {
                    słowa[słowo]++;
                }
                else
                {
                    słowa.Add(słowo, 1);
                }
            }

            foreach (var para in słowa)
            {
                yield return new Tuple<string, uint>(para.Key, para.Value);
            }
        }

        /// <summary>
        /// Kod w sumie dość podobny, niemal cała magia dzieje się również w strukturze danych
        /// </summary>
        /// <param name="dostawca"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, uint>> ImplementacjaDrzewem(IWordProvider dostawca)
        {
            var drzewo = new DrzewoMarkova();
            while (dostawca.NextWord(out var słowo))
            {
                słowo = słowo.ToLower(); // tak sobie myślę, że to ma sens

                var węzeł = drzewo.FindOrAdd(słowo);
                węzeł.Value++;
            }

            foreach (var węzeł in drzewo.All.Where(w => w.Value > 0)) // pozostaw tylko te węzły, które kończą wyrazy
            {
                var wyraz = ZbudujWyraz(węzeł);
                yield return new Tuple<string, uint>(wyraz, węzeł.Value);
            }
        }

        private string ZbudujWyraz(Node węzeł)
        {
            var literki = new List<char>(10);

            while (węzeł != null)
            {
                literki.Add(węzeł.Key);

                węzeł = węzeł.Parent;
            }

            literki.Reverse();
            
            return new string(literki.ToArray());   
        }

        [TestCase(@"C:\temp\internal-nlog.txt")]
        [TestCase(@"C:\temp\big.txt")]
        public void test_porownawczy(string filePath)
        {
            Tuple<string, uint>[] hashTest;
            Tuple<string, uint>[] treeTest;

            List<string> daneWejscioweBufor = new List<string>();

            using (var provider = new FileWordProvider(filePath))
            {
                while (provider.NextWord(out var word))
                {
                    daneWejscioweBufor.Add(word);
                }
            }

            var p1 = new ConstProvider(daneWejscioweBufor);
            var p2 = new ConstProvider(daneWejscioweBufor);

            DateTime start = DateTime.Now;

            hashTest = ImplementacjaHaszMapą(p1).OrderBy(p => p.Item1).ToArray();

            DateTime end = DateTime.Now;
            Console.WriteLine((end - start).TotalMilliseconds);
            

            start = DateTime.Now;

            treeTest = ImplementacjaDrzewem(p2).OrderBy(p => p.Item1).ToArray();

            end = DateTime.Now;
            Console.WriteLine((end - start).TotalMilliseconds); 

            // huhu, a jednak - circa dwa razy wolniejsze...zarówno dla normalnych wyrazów jak i takich o 82 znakach...
            // przy większych plikach, i większej liczbie wyrazów (ok 30k róznych wyrazów) - przewaga jest już mniejsza

            // Aktualna wersja z rzadkimi tablicami na węzłach osiąga już zbliżone wyniki do tej na haszmapie.

            
            CollectionAssert.AreEqual(hashTest, treeTest);
        }
    }

    public class ConstProvider : IWordProvider
    {
        private readonly List<string> _buffer;
        private int _index = 0;

        public ConstProvider(List<string> buffer)
        {
            this._buffer = buffer;
        }

        public bool NextWord(out string word)
        {
            if (_index < _buffer.Count)
            {
                word = _buffer[_index++];
                return true;
            }

            word = null;
            return false;
        }
    }

    internal class DrzewoMarkova
    {
        // Nie ma co symulować ułomnego jednego głównego korzenia - po prostu zasadźmy wiele drzew
        private readonly Node[] _korzenie = new Node[Node._childrenArraySize];

        // Załozenie - trafiają tu już tylko elegancko oczyszczone, gołe wyrazy
        public Node FindOrAdd(string key)
        {
            char[] literki = key.ToCharArray();
            var index = Node._hashIndex[literki[0]];
            if (_korzenie[index] == null)
            {
                this._korzenie[index] = new Node(literki[0], null);
            }

            return this._korzenie[index].ApplyTree(literki, 0);
        }

        /// <summary>
        /// Iterator
        /// </summary>
        public IEnumerable<Node> All
        {
            get
            {
                foreach (var korzeń in _korzenie.Where(k => k != null))
                {
                    yield return korzeń;

                    foreach (var child in korzeń.All)
                    {
                        yield return child;
                    }

                }
            }
        }
    }

    internal class Node
    {
        // tak czy siak - njapierw obliczenie tablicy indeksującej, ale tym razem już bez mapy bitowej

        internal static readonly uint[] _hashIndex;
        internal static int _childrenArraySize;

        static Node()
        {
            var pary = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToCharArray()
                .Select(ch => (int)ch)
                .ToArray();
            var paryBig = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToUpper().ToCharArray()
                .Select(ch => (int)ch)
                .ToArray();

            _childrenArraySize = Math.Max(paryBig.Length, pary.Length);
            var maxChildIndex = Math.Max(paryBig.Max(p => p), pary.Max(p => p)) + 1;
            _hashIndex = new uint[maxChildIndex];

            for (uint i = 0; i < pary.Length; i++)
            {
                _hashIndex[pary[i]] = i;
            }
            for (uint i = 0; i < paryBig.Length; i++)
            {
                _hashIndex[paryBig[i]] = i;
            }
        }

        public Node Parent;

        public uint Value;

        public char Key;

        public Node[] Children;
        

        public Node(char key, Node parent = null)
        {
            //Console.Write('.'); // licz węzły ;-)
            Parent = parent;
            Value = 0;
            Key = key;
            Children = new Node[_childrenArraySize];
        }

        // zwraca wszystkie węzły
        public IEnumerable<Node> All
        {
            get
            {
                foreach (var kid in this.Children.Where(ch => ch != null))
                {
                    yield return kid;

                    foreach (var child in kid.All)
                    {
                        yield return child;
                    }

                }
            }
        }

        public Node FindOrAdd(char childKey)
        {
            var index = _hashIndex[childKey];
            return this.Children[index] ?? (this.Children[index] = new Node(childKey, this));
        }

        public Node ApplyTree(char[] chars, int i)
        {
            if (i >= chars.Length - 1)
            {
                return this;
            }

            i++;

            var child = FindOrAdd(chars[i]);
            return child.ApplyTree(chars, i);
        }
    }

    public static class Extensions
    {
        private static readonly HashSet<char> _litery = new HashSet<char>("aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToCharArray());

        /// <summary>
        /// Usunie znaki nie będące literami.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>Zakłada obecność samych małych liter na wejściu - reszte traktuje jako śmieci!</remarks>
        public static string RemoveNonLetters(this string text)
        {
            return new string(text.ToCharArray().Where(ch => _litery.Contains(ch)).ToArray());
        }
    }

    /// <summary>
    /// Wyciaga słowa z pliku, wywala ciągi i znaki nie będące literami
    /// </summary>
    public class FileWordProvider : IWordProvider, IDisposable
    {
        private StreamReader _file;
        private string[] _currentLine;
        private int _currentIndex;
        private char[] _splitChars = { ',', ' ', '\t', '.', '\\', ':', ';', '!', '?', '-', '+', '=', '/' }; // na dziwn epliki i brakujące spacje pomiędzy znakami

        public FileWordProvider(string filePath)
        {
            _file = new StreamReader(new FileStream(filePath, FileMode.Open));

            ReadLine();
        }

        private bool ReadLine()
        {
            _currentIndex = 0;
            string line = _file.ReadLine();
            if (line == null)
            {
                return false;
            }

            // cleanup
            line = line.ToLower();
            _currentLine = line.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.RemoveNonLetters())
                .Where(s => !string.IsNullOrWhiteSpace(s))  // po czyszczeniu
                .ToArray();

            if (_currentLine.Length == 0)
            {
                return ReadLine(); // next line // TODO: wywalić rekurencję
            }

            return true;
        }

        public bool NextWord(out string word)
        {
            if (_currentLine == null || _currentIndex >= _currentLine.Length)
            {
                if (!ReadLine())
                {
                    word = null;
                    return false; // EOF
                }
            }

            word = _currentLine[_currentIndex++];
            return true;
        }

        public void Dispose()
        {
            _file.Close();
            _file.Dispose();
        }
    }

    public interface IWordProvider
    {
        bool NextWord(out string word);
    }
}
