using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSkill18
{
    // http://www.rjp.pan.pl/index.php?option=com_content&view=article&id=1047:ile-jest-sow-w-jzyku-polskim&catid=44&Itemid=145
    // Powiedzmy PL ma 150K słów, wliczajac gwarowe, zapożyczenia i odmiany, przyjmując średnio 10 znaków Unicode na wyraz, dostaniemy niespełna 3MB na same wyrazy.
    // Angielski moze mieć ich nawet jeszcze więcej - https://en.oxforddictionaries.com/explore/how-many-words-are-there-in-the-english-language/ ponad 170K
    // Można przyjąć optymistycznie, że plik wejściowy zawiera "sensowne" dane, czyli zbiór co najwyżej kilkuset wyrazów z jakiejś dzidziny, co by się jeszcze łądniej dało upchać w prostej HashMapie.
    // Co jednak jeśli na wejściu dostaniemy słownik właśnie? Kompresja będzie słaba...
    // Do tego pytanie o maksymalną zajętość pamięci w Mapie - struktury wewnętrzne, kubełki, hasze a wreszcie i same liczniki (uint wystarczy?). No ale w 100MB i tak spokojnie by się z tym dało zebrać.
    // Do tego implementacja z hasmapą byłaby dość szybka i bardzo czytelna, zwłaszcza wykorzystując systemowy typ Dictionary<string, uint> i już, bez wynajdywania koła na nowo.

    // Alternatywą wydaje się budowa drzewa (grafu bardziej chyba) rozpinającego o odpowiedniej wielkości. Jego przeszukanie powinno być dość szybkie, pod warunkiem sprawnej funkcji indeksującej.
    // Podobnej to tej z pierwszego zadania.

    // Graf potencjalnie rozpinałby się z każdego węzła na maksymalnie 35 węzłów potomnych, przy średniej głębokości 10 poziomów (długość wyrazu),
    // 35^10? potencjalnie masakrycznie dużo...
    // przy czym, niektóre kombinacje nigdy nie wystąpią, wiec tablica podgrafów byłaby rzadka, może lepiej tu dać małe HashMapy - będzie szybciej szukać i mniej pamięci zeżrą?
    

    // I tak to wszystko teoria - bez pożądnych load-testów i tak diabli wiedzą jakie podejście byłoby najszybsze... Bo zasoby pamięci i tak są tu marginalne.
    public class LicznikWyrazów
    {
        // tak czy siak - njapierw obliczenie tablicy indeksującej, ale tym razem już bez mapy bitowej

        private readonly uint[] _hashIndex;

        public LicznikWyrazów()
        {
            var pary = "aąbcćdeęfghijklłmnńoópqrsśtuvwxyzżź".ToCharArray()
                .Select(ch => (int)ch)
                .ToArray();

            _hashIndex = new uint[pary.Max(p => p) + 1];
            for (uint i = 0; i < pary.Length; i++)
            {
                this._hashIndex[pary[i]] = i;
            }
        }



    }
}
