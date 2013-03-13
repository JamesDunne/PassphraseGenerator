using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WellDunne
{
    /// <summary>
    /// The container class for common passphrase policies.
    /// </summary>
    public static class PassphrasePolicy
    {
        /// <summary>
        /// A reasonable default passphrase complexity policy.
        /// </summary>
        public static PassphraseGenerator.SanitizedGeneratorOptions Default =
            PassphraseGenerator.ValidateOptions(
                new PassphraseGenerator.GeneratorOptions
                {
                    MinLength = 10,
                    MaxLength = 22,
                    MinWordCount = 4,
                    RequireUppercase = true,
                    UppercaseOnlyFirstLetters = true,
                    RequireNumber = true,
                    AppendNumberToEndOnly = true,
                    RemoveTheseChars = BadChars,
                    RequiredSpecialChars = null
                }
            );

        /// <summary>
        /// A standard set of chars to remove from dictionary words.
        /// </summary>
        public static readonly char[] BadChars = new char[] { '~', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '+', '=', '&', '<', ';', '\'', '\"', '?', '.', ',', ':', ';', '[', ']', '{', '}', '|', '\\' };
    }
}
