using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Data.Helper.Extensions
{
    #pragma warning disable CS8603
    public static class Extension
    {
        private static IEnumerable<TList> Split<TList, T>(this TList value, int countOfEachPart)
            where TList : IEnumerable<T>
        {
            var cnt    = value.Count() / countOfEachPart;
            var result = new List<IEnumerable<T>>();

            for (var I = 0; I <= cnt; I++)
            {
                IEnumerable<T> newPart = value.Skip(I * countOfEachPart)
                                              .Take(countOfEachPart)
                                              .ToArray();

                if (newPart.Any())
                    result.Add(newPart);
                else
                    break;
            }

            return result.Cast<TList>();
        }
        #pragma warning disable CS8603, notnull
        public static IEnumerable<IDictionary<TKey, TValue>> Split<TKey, TValue>(this IDictionary<TKey, TValue> value,
                                                                                 int countOfEachPart)
        {
            var result = value.ToArray()
                              .Split(countOfEachPart)
                              .Select(p => p.ToDictionary(k => k.Key, v => v.Value));

            return result;
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
            foreach (var element in source) action(element);
        }
    }
}