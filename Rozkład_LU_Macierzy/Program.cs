using System;
using System.Diagnostics;

namespace Rozkład_LU_Macierzy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Rozkład LU macierzy metodą Gaussa ===\n");

            Stopwatch stop = new Stopwatch();
            // Przykładowa macierz 3x3
            double[,] macierz = {
                { 2, -1, 3 },
                { 4, 2, -1 },
                { -2, 3, 1 }
            };

            Console.WriteLine("Macierz początkowa:");
            WyswietlMacierz(macierz);

            var (L, U, historiaU, operacje) = RozkladLU_Z_Sladem(macierz);

            ZapiszDoPliku(historiaU, operacje, "lu_graf_dane.txt");

            if (L != null && U != null)
            {
                Console.WriteLine("\nMacierz L (dolnotrójkątna):");
                WyswietlMacierz(L);

                Console.WriteLine("\nMacierz U (górnotrójkątna):");
                WyswietlMacierz(U);

                Console.WriteLine("\nSprawdzenie: L × U =");
                var wynik = PomnoźMacierze(L, U);
                WyswietlMacierz(wynik);
            }
        }

        static (double[,] L, double[,] U, List<double[,]> historiaU, List<string> historiaOperacji) RozkladLU_Z_Sladem(double[,] A)
        {
            int n = A.GetLength(0);

            double[,] U = KopiujMacierz(A);
            double[,] L = new double[n, n];

            for (int i = 0; i < n; i++)
                L[i, i] = 1.0;

            List<double[,]> historiaU = [];
            List<string> historiaOperacji = [];

            // stan początkowy
            historiaU.Add(KopiujMacierz(U));
            historiaOperacji.Add("U(0) = A");

            for (int k = 0; k < n - 1; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    // Gniazdo G1 – obliczenie mnożnika
                    double mnoznik = U[i, k] / U[k, k];
                    L[i, k] = mnoznik;

                    historiaOperacji.Add(
                        $"G1: L[{i},{k}] = U[{i},{k}] / U[{k},{k}]"
                    );

                    for (int j = k; j < n; j++)
                    {
                        // Gniazdo G2 – aktualizacja U
                        U[i, j] -= mnoznik * U[k, j];

                        historiaOperacji.Add(
                            $"G2: U[{i},{j}] = U[{i},{j}] - L[{i},{k}] * U[{k},{j}]"
                        );
                    }
                }

                // zapis U po kroku k
                historiaU.Add(KopiujMacierz(U));
            }

            return (L, U, historiaU, historiaOperacji);
        }


        static void WyswietlMacierz(double[,] macierz)
        {
            int wiersze = macierz.GetLength(0);
            int kolumny = macierz.GetLength(1);

            for (int i = 0; i < wiersze; i++)
            {
                Console.Write("| ");
                for (int j = 0; j < kolumny; j++)
                {
                    Console.Write($"{macierz[i, j],8:F3} ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine();
        }

        static double[,] PomnoźMacierze(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] wynik = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    wynik[i, j] = 0;
                    for (int k = 0; k < n; k++)
                    {
                        wynik[i, j] += A[i, k] * B[k, j];
                    }
                }
            }

            return wynik;
        }

        static double[,] KopiujMacierz(double[,] M)
        {
            int n = M.GetLength(0);
            double[,] kopia = new double[n, n];

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    kopia[i, j] = M[i, j];

            return kopia;
        }

        static void ZapiszDoPliku(List<double[,]> historiaU,List<string> operacje,string nazwaPliku)
        {
            using (StreamWriter sw = new StreamWriter(nazwaPliku))
            {
                sw.WriteLine("=== HISTORIA ALGORYTMU LU ===\n");

                for (int k = 0; k < historiaU.Count; k++)
                {
                    sw.WriteLine($"--- U po kroku {k} ---");
                    ZapiszMacierz(sw, historiaU[k]);
                    sw.WriteLine();
                }

                sw.WriteLine("=== OPERACJE (GNIAZDA GRAFU) ===\n");
                foreach (var op in operacje)
                    sw.WriteLine(op);
            }
        }

        static void ZapiszMacierz(StreamWriter sw, double[,] M)
        {
            int n = M.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    sw.Write($"{M[i, j],8:F3} ");
                sw.WriteLine();
            }
        }

    }
}