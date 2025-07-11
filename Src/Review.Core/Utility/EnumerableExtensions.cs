﻿#region using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Review.Core.Utility;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Checks whether a sequence is empty or not.
    ///     Reverse of IEnumerable{T}.Any(), optimized for ICollection{T}.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        switch (enumerable)
        {
            case null:
                throw new ArgumentNullException(nameof(enumerable));
            case ICollection<T> collection:
                return collection.Count <= 0;
            case IReadOnlyCollection<T> readOnlyCollection:
                return readOnlyCollection.Count <= 0;
            default:
                return !enumerable.Any();
        }
    }

    /// <summary>
    ///     Executes given action for each element of enumerable.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> doAction)
    {
        foreach (var element in enumerable)
        {
            doAction(element);
        }
    }

    /// <summary>
    ///     Returns random element from the given enumerable.
    /// </summary>
    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

        var list = enumerable.ToList();
        if (list.IsEmpty()) throw new ArgumentException("The sequence contains no elements.");

        if (list.Count == 1) return list[0];

        var rand = new Random();
        return list[rand.Next(list.Count)];
    }
}