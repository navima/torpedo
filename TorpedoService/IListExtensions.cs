// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

#pragma warning disable SA1000 // Keywords should be spaced correctly
#nullable enable

using System;
using System.Collections.Generic;

namespace NationalInstruments
{
    public static class IListExtensions
    {
        private static readonly Random _random = new();
        /// <summary>
        /// <para>
        /// Shuffles the List in-place using the Fisher-Yates algorithm.
        /// </para>
        /// <para>
        /// <see href="https://en.wikipedia.org/wiki/Fisher–Yates_shuffle"/>
        /// </para>
        /// <para>
        /// <seealso href="https://stackoverflow.com/a/1262619/9281022"/>
        /// </para>
        /// </summary>
        /// <param name="list">this</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
