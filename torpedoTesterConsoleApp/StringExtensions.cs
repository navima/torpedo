// <copyright file="StringExtensions.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

namespace NationalInstruments
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Class for holding extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a capitalized version of source string using InvariantCulture to capitalize.
        /// </summary>
        /// <param name="thisString">string to operate on.</param>
        /// <returns>capitalized version of source string.</returns>
        public static string Capitalize(this string thisString) => CapitalizeInvariant(thisString);

        /// <summary>
        /// Returns a capitalized version of source string using InvariantCulture to capitalize.
        /// </summary>
        /// <param name="thisString">string to operate on.</param>
        /// <returns>capitalized version of source string.</returns>
        public static string CapitalizeInvariant(this string thisString) => Capitalize(thisString, CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns a capitalized version of source string.
        /// </summary>
        /// <param name="thisString">string to operate on.</param>
        /// <param name="culture">culture used in capitalization.</param>
        /// <returns>capitalized version of source string.</returns>
        public static string Capitalize(this string thisString, CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            if (string.IsNullOrEmpty(thisString))
            {
                return thisString;
            }

            return string.Concat(thisString[0].ToString(culture.NumberFormat).ToUpper(culture), thisString.AsSpan(1));
        }
    }
}
