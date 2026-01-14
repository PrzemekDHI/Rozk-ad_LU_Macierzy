using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RozkladLU_PelnaAnaliza
{
    public class Rekord
    {
        public string Gniazdo; // G1 lub G2
        public int Nr;
        public int i1, i2, i3;
        public (int, int) Im, Ia2, Ia1; // Adresy komórek macierzy

        public override string ToString()
        {
            return $"{Gniazdo}(nr:{Nr}, i1={i1}, i2={i2}, i3={i3})";
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== ANALIZA ROZKŁADU LU - GENERATOR GRAFÓW ZALEŻNOŚCI ===\n");

            int N = PobierzRozmiarMacierzy();

            Console.WriteLine($"\nGenerowanie operacji dla macierzy {N}×{N}...");
            var (tabelaG1, tabelaG2) = GenerujWszystkieOperacje(N);

            Console.WriteLine("Tworzenie kolejności wykonania...");
            var wszystkie = PobierzOperacjeWKolejnosciWykonania(tabelaG1, tabelaG2);

            Console.WriteLine("Analiza zależności...");
            string nazwaPliku = $"analiza_LU_N{N}.txt";
            ZapiszRaport(tabelaG1, tabelaG2, wszystkie, nazwaPliku);

            Console.WriteLine($"\n✓ Gotowe! Raport zapisany w pliku: '{nazwaPliku}'");
            Console.WriteLine($"  - Operacji G1: {tabelaG1.Count}");
            Console.WriteLine($"  - Operacji G2: {tabelaG2.Count}");
            Console.WriteLine($"  - Łącznie operacji: {wszystkie.Count}");

            Console.WriteLine("\nNaciśnij Enter aby zakończyć...");
            Console.ReadLine();
        }

        static int PobierzRozmiarMacierzy()
        {
            while (true)
            {
                Console.Write("Podaj rozmiar macierzy N (2-100): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int n))
                {
                    if (n >= 2 && n <= 100)
                    {
                        return n;
                    }
                    else
                    {
                        Console.WriteLine("Błąd: Rozmiar musi być między 2 a 100.");
                    }
                }
                else
                {
                    Console.WriteLine("Błąd: Proszę podać liczbę całkowitą.");
                }
            }
        }

        static (List<Rekord> g1, List<Rekord> g2) GenerujWszystkieOperacje(int n)
        {
            var g1 = new List<Rekord>();
            var g2 = new List<Rekord>();
            int n1 = 1, n2 = 1;

            for (int k = 1; k <= n - 1; k++)
            {
                for (int i = k + 1; i <= n; i++)
                {
                    // Gniazdo G1: obliczanie mnożnika m[i,k] = a[i,k] / a[k,k]
                    g1.Add(new Rekord
                    {
                        Gniazdo = "G1",
                        Nr = n1++,
                        i1 = k,
                        i2 = i,
                        i3 = k,
                        Im = (i, k),      // czyta a[i,k]
                        Ia2 = (k, k),     // czyta a[k,k]
                        Ia1 = (i, k)      // zapisuje m[i,k] (nadpisuje a[i,k])
                    });

                    for (int j = k + 1; j <= n; j++)
                    {
                        // Gniazdo G2: aktualizacja a[i,j] = a[i,j] - m[i,k] * a[k,j]
                        g2.Add(new Rekord
                        {
                            Gniazdo = "G2",
                            Nr = n2++,
                            i1 = k,
                            i2 = i,
                            i3 = j,
                            Im = (i, k),      // czyta m[i,k]
                            Ia2 = (k, j),     // czyta a[k,j]
                            Ia1 = (i, j)      // zapisuje a[i,j]
                        });
                    }
                }
            }
            return (g1, g2);
        }

        static List<Rekord> PobierzOperacjeWKolejnosciWykonania(List<Rekord> g1, List<Rekord> g2)
        {
            var res = new List<Rekord>();

            // Grupujemy operacje G2 według (i1, i2) dla łatwego dostępu
            var g2PodlaKlucza = g2.GroupBy(r => (r.i1, r.i2))
                                  .ToDictionary(g => g.Key, g => g.ToList());

            // Przechodzimy przez operacje G1 w kolejności wykonania
            foreach (var operacjaG1 in g1)
            {
                // Dodajemy operację G1
                res.Add(operacjaG1);

                // Dodajemy wszystkie powiązane operacje G2 dla tego samego (k, i)
                var klucz = (operacjaG1.i1, operacjaG1.i2);
                if (g2PodlaKlucza.TryGetValue(klucz, out var powiazaneG2))
                {
                    // Dodajemy G2 w kolejności rosnących j (i3)
                    res.AddRange(powiazaneG2.OrderBy(r => r.i3));
                }
            }

            return res;
        }

        static void ZapiszRaport(List<Rekord> g1, List<Rekord> g2, List<Rekord> wszystkie, string plik)
        {
            using (StreamWriter sw = new StreamWriter(plik))
            {
                sw.WriteLine("=======================================================");
                sw.WriteLine("   ANALIZA ROZKŁADU LU - ZALEŻNOŚCI INFORMACYJNE");
                sw.WriteLine("=======================================================\n");

                sw.WriteLine("=== TABELA G1 (Obliczanie mnożników) ===");
                sw.WriteLine("Operacja: m[i,k] = a[i,k] / a[k,k]");
                sw.WriteLine("nr | i1 | i2 | i3 | Im(czyt) | Ia2(czyt) | Ia1(zapis)");
                sw.WriteLine("---+----+----+----+----------+-----------+-----------");
                foreach (var r in g1)
                {
                    sw.WriteLine($"{r.Nr,2} | {r.i1,2} | {r.i2,2} | {r.i3,2} | " +
                                $"<{r.Im.Item1},{r.Im.Item2}>    | " +
                                $"<{r.Ia2.Item1},{r.Ia2.Item2}>      | " +
                                $"<{r.Ia1.Item1},{r.Ia1.Item2}>");
                }

                sw.WriteLine("\n=== TABELA G2 (Aktualizacja macierzy) ===");
                sw.WriteLine("Operacja: a[i,j] = a[i,j] - m[i,k] * a[k,j]");
                sw.WriteLine("nr | i1 | i2 | i3 | Im(czyt) | Ia2(czyt) | Ia1(zapis)");
                sw.WriteLine("---+----+----+----+----------+-----------+-----------");
                foreach (var r in g2)
                {
                    sw.WriteLine($"{r.Nr,2} | {r.i1,2} | {r.i2,2} | {r.i3,2} | " +
                                $"<{r.Im.Item1},{r.Im.Item2}>    | " +
                                $"<{r.Ia2.Item1},{r.Ia2.Item2}>      | " +
                                $"<{r.Ia1.Item1},{r.Ia1.Item2}>");
                }

                sw.WriteLine("\n=== KOLEJNOŚĆ WYKONANIA OPERACJI ===");
                for (int i = 0; i < wszystkie.Count; i++)
                {
                    var op = wszystkie[i];
                    sw.WriteLine($"{i + 1,3}. {op}");
                }

                sw.WriteLine("\n=== WSZYSTKIE ŁUKI ZALEŻNOŚCI INFORMACYJNEJ (RAW) ===");
                sw.WriteLine("Zależność: Operacja Y czyta dane zapisane przez wcześniejszą operację X");
                sw.WriteLine("Format: X ---> Y | przez adres | opis\n");

                int liczbaZaleznosci = 0;

                // Szukamy wszystkich zależności RAW (Read After Write)
                for (int y = 0; y < wszystkie.Count; y++)
                {
                    for (int x = 0; x < y; x++)
                    {
                        var opX = wszystkie[x]; // Operacja wcześniejsza (producent)
                        var opY = wszystkie[y]; // Operacja późniejsza (konsument)

                        // Sprawdzamy czy Y czyta to, co X zapisał
                        // Y czyta przez Im lub Ia2, X zapisuje przez Ia1

                        if (opY.Im == opX.Ia1)
                        {
                            sw.WriteLine($"{opX.Gniazdo}(nr:{opX.Nr}) ---> {opY.Gniazdo}(nr:{opY.Nr}) " +
                                       $"| przez adres: <{opX.Ia1.Item1},{opX.Ia1.Item2}> " +
                                       $"| Y.Im czyta wynik X.Ia1");
                            liczbaZaleznosci++;
                        }

                        if (opY.Ia2 == opX.Ia1)
                        {
                            sw.WriteLine($"{opX.Gniazdo}(nr:{opX.Nr}) ---> {opY.Gniazdo}(nr:{opY.Nr}) " +
                                       $"| przez adres: <{opX.Ia1.Item1},{opX.Ia1.Item2}> " +
                                       $"| Y.Ia2 czyta wynik X.Ia1");
                            liczbaZaleznosci++;
                        }
                    }
                }

                sw.WriteLine($"\n=== PODSUMOWANIE ===");
                sw.WriteLine($"Liczba operacji G1: {g1.Count}");
                sw.WriteLine($"Liczba operacji G2: {g2.Count}");
                sw.WriteLine($"Łączna liczba operacji: {wszystkie.Count}");
                sw.WriteLine($"Liczba wykrytych zależności RAW: {liczbaZaleznosci}");

                sw.WriteLine("\n=== STATYSTYKI ZALEŻNOŚCI ===");

                // Zliczamy zależności według typu
                int g1_g1 = 0, g1_g2 = 0, g2_g2 = 0;

                for (int y = 0; y < wszystkie.Count; y++)
                {
                    for (int x = 0; x < y; x++)
                    {
                        var opX = wszystkie[x];
                        var opY = wszystkie[y];

                        if (opY.Im == opX.Ia1 || opY.Ia2 == opX.Ia1)
                        {
                            if (opX.Gniazdo == "G1" && opY.Gniazdo == "G1") g1_g1++;
                            else if (opX.Gniazdo == "G1" && opY.Gniazdo == "G2") g1_g2++;
                            else if (opX.Gniazdo == "G2" && opY.Gniazdo == "G2") g2_g2++;
                        }
                    }
                }

                sw.WriteLine($"Zależności G1 → G1: {g1_g1}");
                sw.WriteLine($"Zależności G1 → G2: {g1_g2}");
                sw.WriteLine($"Zależności G2 → G2: {g2_g2}");
            }
        }
    }
}