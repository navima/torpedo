// <copyright file="TorpedoServiceTests.cs" company="University Of Debrecen">
// Copyright (c) University Of Debrecen. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace NationalInstruments
{
    public static class Validation
    {
        public static readonly ICollection<string> ReservedNames = new List<string>() { "AI" };
        public static readonly Regex NameValidationRegex = new("^[a-zA-Z0-9]{3,9}$");
        public static bool ValidateName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && NameValidationRegex.IsMatch(name) && !ReservedNames.Contains(name);
        }
    }
}
