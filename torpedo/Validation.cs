using System.Collections.Generic;
using System.Text.RegularExpressions;

#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace NationalInstruments
{
    public static class Validation
    {
        public static readonly List<string> ReservedNames = new() { "AI" };
        public static readonly Regex NameValidationRegex = new("^[a-zA-Z0-9]{3,9}$");
        public static bool ValidateName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && NameValidationRegex.IsMatch(name) && !ReservedNames.Contains(name);
        }
    }
}
