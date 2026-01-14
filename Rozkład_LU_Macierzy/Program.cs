using System;
using System.Collections.Generic;
using System.IO;

namespace RozkladLU_Graf
{
    // Struktura odpowiadająca kolumnom z Twojego slajdu
    public struct RekordGrafu
    {
        public int Nr;
        public int W1, W2, W3;
        public string Im, Ia2, Ia1;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Rozkład LU z generowaniem tabeli grafu ===\n");

            // Macierz 4x4 (żeby tabela była dłuższa, jak w przykładzie)
            double[,] macierz = {
                { 2, -1, 3, 1 },
                { 4, 2, -1, 3 },
                { -2, 3, 1, 5 },
                { 1, 1, 1, 1 }
            };

            Console.WriteLine("Macierz wejściowa:");
            WyswietlMacierz(macierz);

            // Uruchomienie algorytmu
            var (L, U, tabela) = RozkladLU_Z_Generatorem(macierz);

            // Wyświetlenie wyników
            Console.WriteLine("Macierz L:");
            WyswietlMacierz(L);
            Console.WriteLine("Macierz U:");
            WyswietlMacierz(U);

            // Zapis do pliku tekstowego (tabela jak ze slajdu)
            ZapiszTabeleDoPliku(tabela, "tabela_grafu.txt");

            Console.WriteLine("Sukces! Tabela grafu została zapisana do pliku 'tabela_grafu.txt'.");
            Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
            Console.ReadKey();
        }

        static (double[,] L, double[,] U, List<RekordGrafu> tabela) RozkladLU_Z_Generatorem(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] U = (double[,])A.Clone();
            double[,] L = new double[n, n];
            for (int i = 0; i < n; i++) L[i, i] = 1.0;

            List<RekordGrafu> tabela = new List<RekordGrafu>();
            int licznik = 0;

            // Pętle i1, i2, i3 (mapowanie: k, i, j)
            // Zakresy ustawione tak, aby odpowiadały indeksowaniu od 1 (jak na slajdzie)
            for (int k = 1; k <= n - 1; k++) // i1
            {
                for (int i = k + 1; i <= n; i++) // i2
                {
                    // Obliczenie mnożnika (Gniazdo G1)
                    double mnoznik = U[i - 1, k - 1] / U[k - 1, k - 1];
                    L[i - 1, k - 1] = mnoznik;

                    for (int j = k + 1; j <= n; j++) // i3
                    {
                        // Operacja aktualizacji (Gniazdo G2)
                        U[i - 1, j - 1] -= mnoznik * U[k - 1, j - 1];

                        // Rejestracja wiersza tabeli
                        licznik++;
                        tabela.Add(new RekordGrafu
                        {
                            Nr = licznik,
                            W1 = k,
                            W2 = i,
                            W3 = j,
                            Im = $"<{i},{k}>",   // m[i2, i1]
                            Ia2 = $"<{k},{j}>",  // a[i1, i3]
                            Ia1 = $"<{i},{j}>"   // a[i2, i3]
                        });
                    }
                }
            }
            return (L, U, tabela);
        }

        static void ZapiszTabeleDoPliku(List<RekordGrafu> tabela, string nazwaPliku)
        {
            using (StreamWriter sw = new StreamWriter(nazwaPliku))
            {
                sw.WriteLine("Konstruowanie grafów algorytmów - Tabela G2");
                sw.WriteLine("------------------------------------------------------------------");
                sw.WriteLine("{0,-4} | {1,-2} {2,-2} {3,-2} | {4,-8} | {5,-8} | {6,-8}",
                             "nr", "W1", "W2", "W3", "Im[nr]", "Ia2[nr]", "Ia1[nr]");
                sw.WriteLine("------------------------------------------------------------------");

                foreach (var r in tabela)
                {
                    sw.WriteLine("{0,-4} | {1,-2} {2,-2} {3,-2} | {4,-8} | {5,-8} | {6,-8}",
                        r.Nr, r.W1, r.W2, r.W3, r.Im, r.Ia2, r.Ia1);
                }
            }
        }

        static void WyswietlMacierz(double[,] M)
        {
            int n = M.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    Console.Write($"{M[i, j],8:F2} ");
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}