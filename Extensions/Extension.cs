using System;
using System.Collections.Generic;
using System.Linq;

namespace core.data.helper.extensions
{
#pragma warning disable CS8603 
    public static class Extension
    {
        private static IEnumerable<TList> Split<TList, T>(this TList value, int countOfEachPart)
            where TList : IEnumerable<T>
        {
            var Cnt = value.Count() / countOfEachPart;
            var Result = new List<IEnumerable<T>>();
            for (var I = 0; I <= Cnt; I++)
            {
                IEnumerable<T> NewPart = value.Skip(I * countOfEachPart).Take(countOfEachPart).ToArray();
                if (NewPart.Any())
                    Result.Add(NewPart);
                else
                    break;
            }

            return Result.Cast<TList>();
        }
#pragma warning disable CS8603, notnull 
        public static IEnumerable<IDictionary<TKey, TValue>> Split<TKey, TValue>(this IDictionary<TKey, TValue> value, int countOfEachPart)
        {
            var Result = value.ToArray()
                .Split(countOfEachPart)
                .Select(p => p.ToDictionary(k => k.Key, v => v.Value));
            return Result;
        }

        public static IEnumerable<IList<T>> Split<T>(this IList<T> value, int countOfEachPart)
        {
            return value.Split<IList<T>, T>(countOfEachPart);
        }

        public static IEnumerable<T[]> Split<T>(this T[] value, int countOfEachPart)
        {
            return value.Split<T[], T>(countOfEachPart);
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> value, int countOfEachPart)
        {
            return value.Split<IEnumerable<T>, T>(countOfEachPart);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var Element in source)
                action(Element);
        }
    }
}