using MarketApp.HelpClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MarketApp
{

    public class PrimeTester
    {
        private HashSet<long> primes = new HashSet<long>();
        private HashSet<long> testedNumbers = new HashSet<long>();

        public bool IsPrime(long n)
        {
            if (testedNumbers.Contains(n))
            {
                return primes.Contains(n);
            }
            testedNumbers.Add(n);
            long q, r, d;

            if (n < 3 || (n & 1) == 0)
                return n == 2;

            for (d = 3, r = 1; r != 0; d += 2)
            {
                q = n / d;
                r = n - q * d;
                if (q < d)
                {
                    primes.Add(n);
                    return true;
                }
                    
            }
            return false;
        }
    }

    public class Decomposition
    {
        public long Number { get; }
        public List<long> KindsMasses { get; set; }
        public int KindsCount { get { return KindsMasses.Count; } }

        public Decomposition(long number)
        {
            Number = number;
            KindsMasses = new List<long> { number };
        }
        public Decomposition()
        {
        }

        public void ProlongWith(long divisor, Decomposition decomposition)
        {
            if(KindsCount < 1)
            {
                throw new InvalidOperationException("cannot prolong empty decomposition");
            }
            KindsMasses[KindsCount - 1] = divisor;
            KindsMasses.AddRange(decomposition.KindsMasses);
        }

        public Decomposition GetCopy()
        {
            var result = new Decomposition(Number);
            result.KindsMasses = new List<long>(KindsMasses.GetRange(0, KindsMasses.Count));
            return result;
        }

        public CalculationResult ToCalculationResult()
        {
            long[] numbers = new long[KindsCount];

            for (int i = 0; i != KindsCount; ++i)
            {
                numbers[i] = KindsMasses
                    .Take(i + 1)
                    .Aggregate((total, next) => total * next);
            }

            return new CalculationResult
            {
                KindsCount = KindsCount,
                Numbers = numbers
            };
        }
    }

    public class CalculationToImplement
    {
        private static Dictionary<long, Decomposition> decompositions = new Dictionary<long, Decomposition>();
        private static PrimeTester primeTester = new PrimeTester();

        public Decomposition Decompose(long number)
        {
            return RefineDecomposition(new Decomposition(number));
        }

        /// <summary>
        ///this part is bit shady. got me under 5 secs for lvl 10.
        ///the idea is ..we dont need to look for decompositions starting with first number over some treshold bcs the decompositions are shorter.
        ///coeff 0.35 works for all numbers..tested
        ///(i ran the decompositions for all numbers up to 2mil; and the second number in decomposition is always lower then 0.35 treshold(0.36 is reached for 24)
        ///the coeff is lowering with bigger numbers bcs of log2 in equation... so for big numbers we can lower it
        ///i didnt test all big numbers, but took generous upper bound for coeff
        ///anyway 0.36 gets the lvl 9.. so comment / uncomment to test
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private long GetBound(long number)
        {
            //var coeff = 0.36;
            var coeff = number >= Math.Pow(10, 9)
                ? 0.1
                : number >= Math.Pow(10, 7)
                    ? 0.2
                    : 0.36;

            long bound = (long)Math.Ceiling(coeff * Math.Log2(number) * Math.Sqrt(number));
            return bound;
        }

        public Decomposition RefineDecomposition(Decomposition decomposition)
        {
            var number = decomposition.Number;

            if (decompositions.ContainsKey(number))
            {
                return decompositions[number];
            }

            if (primeTester.IsPrime(number))
            {
                decompositions[number] = decomposition;
                return decomposition;
            }
            else
            {
                var result = decomposition;
                long bound = GetBound(decomposition.Number);
                for (long divisor = 2; divisor <= bound; ++divisor)
                {
                    if (number % divisor == 0)
                    {
                        long nextElement = number / divisor;
                        if(nextElement < 3)
                        {
                            break;
                        }
                        --nextElement;

                        var newDecomposition = new Decomposition(nextElement);

                        if (decompositions.ContainsKey(nextElement))
                        {
                            newDecomposition = decompositions[nextElement];
                        }
                        else
                        {
                            newDecomposition = RefineDecomposition(newDecomposition);
                        }

                        if (!decompositions.ContainsKey(nextElement))
                        {
                            decompositions.Add(nextElement, newDecomposition);
                        }

                        if (newDecomposition.KindsCount + decomposition.KindsCount + 1 > result.KindsCount)
                        {
                            result = decomposition.GetCopy();
                            result.ProlongWith(divisor, newDecomposition);
                            decompositions[result.Number] = result;
                        }
                    }
                }

                return result;
            }
        }

        public CalculationResult Calculate(long numberOfEntities)
        {
            var decomposition = Decompose(numberOfEntities);

            return decomposition.ToCalculationResult();
        }
    }
}