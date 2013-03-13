using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WellDunne
{
    /// <summary>
    /// Passphrase generator using a diceware method.
    /// </summary>
    public static class PassphraseGenerator
    {
        /// <summary>
        /// The dictionary of words to use for generating passphrases from.
        /// </summary>
        static readonly string[] wordList;

        /// <summary>
        /// Total number of dictionary words.
        /// </summary>
        static readonly int dictionaryCount;
        /// <summary>
        /// Minimum number of bits needed to represent the size of the dictionary.
        /// </summary>
        static readonly int dictionaryBitOrder;

        /// <summary>
        /// Maximum number of iterations to attempt to generate a passphrase in.
        /// </summary>
        const int InconceivableIterationLimit = 10000;

        /// <summary>
        /// Static constructor to initialize the word list from the embedded resource.
        /// </summary>
        static PassphraseGenerator()
        {
            // Read the embedded resource as a flat text file, one word per line:
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            // Embedded resource names are stored in (generally) the default namespace:
            var embeddedResourceName = String.Format("{0}.diceware.txt", typeof(PassphraseGenerator).Namespace);

            var txt = asm.GetManifestResourceStream(embeddedResourceName);
            if (txt == null)
                throw new Exception("Embedded resource named '{0}' could not be found!".F(embeddedResourceName));

            using (txt)
            using (var sr = new System.IO.StreamReader(txt))
                wordList = sr.ReadToEnd()
                    // Split by newline (file must use LF line endings only, no CR):
                    .Split('\n')
                    .ToArray();

            // Set up our "constants":
            dictionaryCount = wordList.Length;
            dictionaryBitOrder = CountBits(dictionaryCount);
        }

        /// <summary>
        /// Count the minimum number of bits required to represent the value:
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static int CountBits(int value)
        {
            Debug.Assert(value >= 0);
            int b = 0;
            do
            {
                ++b;
                value >>= 1;
            } while (value > 0);
            return b;
        }

        /// <summary>
        /// Passphrase generation options.
        /// </summary>
        public class GeneratorOptions
        {
            /// <summary>
            /// Absolute minimum length of a passphrase.
            /// </summary>
            public int? MinLength;
            /// <summary>
            /// Absolute maximum length of a passphrase.
            /// </summary>
            public int? MaxLength;

            /// <summary>
            /// The minimum number of words to include in the passphrase.
            /// </summary>
            public int? MinWordCount;

            /// <summary>
            /// If true, at least one uppercase char is required.
            /// </summary>
            public bool? RequireUppercase;
            /// <summary>
            /// If true, uppercase the first letter of each word. If false, select a single char at random to uppercase.
            /// </summary>
            public bool? UppercaseOnlyFirstLetters;

            /// <summary>
            /// If true, at least one numeric char is required.
            /// </summary>
            public bool? RequireNumber;
            /// <summary>
            /// If true, append a single digit to the end of the passphrase. If false, insert a single digit between any two words.
            /// </summary>
            public bool? AppendNumberToEndOnly;

            /// <summary>
            /// At least one of these characters is required in a passphrase.
            /// </summary>
            public char[] RequiredSpecialChars;
            /// <summary>
            /// None of these characters are allowable in a passphrase.
            /// </summary>
            public char[] RemoveTheseChars;
        }

        /// <summary>
        /// An immutable set of validated/sanitized passphrase policy options.
        /// </summary>
        public class SanitizedGeneratorOptions
        {
            internal readonly int MinLength;
            internal readonly int MaxLength;

            internal readonly int MinWordCount;

            internal readonly bool RequireUppercase;
            internal readonly bool UppercaseOnlyFirstLetters;

            internal readonly bool RequireNumber;
            internal readonly bool AppendNumberToEndOnly;

            internal readonly char[] AddTheseChars;
            internal readonly char[] RemoveTheseChars;

            internal SanitizedGeneratorOptions()
            {
            }

            internal SanitizedGeneratorOptions(
                int minLength,
                int maxLength,
                int minWordCount,
                bool requireUppercase,
                bool uppercaseOnlyFirstLetters,
                bool requireNumber,
                bool appendNumberToEndOnly,
                char[] addTheseChars,
                char[] removeTheseChars
            )
            {
                MinLength = minLength;
                MaxLength = maxLength;
                MinWordCount = minWordCount;
                RequireUppercase = requireUppercase;
                UppercaseOnlyFirstLetters = uppercaseOnlyFirstLetters;
                RequireNumber = requireNumber;
                AppendNumberToEndOnly = appendNumberToEndOnly;
                AddTheseChars = addTheseChars;
                RemoveTheseChars = removeTheseChars;
            }
        }

        /// <summary>
        /// Validate the policy first and return a sanitized/valid set of options for use with Generate.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SanitizedGeneratorOptions ValidateOptions(GeneratorOptions options)
        {
            // Enforce some acceptable defaults:
            int minLength = options.MinLength ?? 8;
            int maxLength = options.MaxLength ?? 24;
            int minWordCount = options.MinWordCount ?? 3;
            bool requireUppercase = options.RequireUppercase ?? true;
            bool uppercaseOnlyFirstLetters = options.UppercaseOnlyFirstLetters ?? false;
            bool requireNumber = options.RequireNumber ?? true;
            bool appendNumberToEndOnly = options.AppendNumberToEndOnly ?? false;

            // Limit the ranges:
            if (minLength < 1) minLength = 1;
            if (maxLength < 1) maxLength = 1;
            if (minWordCount < 1) minWordCount = 1;

            // Check sanity of the policy:
            if (minLength > maxLength)
                throw new ArgumentOutOfRangeException("MinLength cannot be greater than MaxLength");

            // Each word is at least 2 chars (avg):
            int trueMinLength = (minWordCount * 2) + (requireNumber ? 1 : 0);
            if (maxLength < trueMinLength)
                throw new ArgumentOutOfRangeException("MaxLength is not large enough to accommodate the minimum number of words required");

            char[] addThese = options.RequiredSpecialChars;
            // TODO(jsd): I'd feel better with a character whitelist as opposed to this blacklist.
            char[] removeThese = options.RemoveTheseChars;

            if (removeThese != null)
            {
                // Can't exclude alpha chars:
                if (removeThese.Any(c => Char.IsLetter(c)))
                    throw new ArgumentOutOfRangeException("RemoveTheseChars cannot contain alpha chars");

                // If we require a numeric char yet we exclude numeric chars, that does not make sense:
                if (requireNumber && removeThese.Any(c => Char.IsDigit(c)))
                    throw new ArgumentOutOfRangeException("RequireNumber is true yet RemoveTheseChars contains numeric chars");

                // Make sure the char sets don't conflict:
                if (addThese != null)
                {
                    if (addThese.Intersect(removeThese).Any())
                        throw new ArgumentOutOfRangeException("Set of chars to add and set of chars to remove cannot overlap");
                    if (removeThese.Intersect(addThese).Any())
                        throw new ArgumentOutOfRangeException("Set of chars to add and set of chars to remove cannot overlap");
                }
            }

            return new SanitizedGeneratorOptions(
                minLength,
                maxLength,
                minWordCount,
                requireUppercase,
                uppercaseOnlyFirstLetters,
                requireNumber,
                appendNumberToEndOnly,
                addThese,
                removeThese
            );
        }

        static string PickWord(RandomNumberGenerator rng, char[] removeThese, bool requireUppercase, bool uppercaseOnlyFirstLetters)
        {
            Debug.Assert(rng != null);

            string s;
            do
            {
                // Pick a word at random within the dictionary:
                int v = rng.GetBitsInRange(dictionaryBitOrder, dictionaryCount);

                s = wordList[v];
                if (removeThese != null)
                {
                    s = s.Filter(removeThese);
                    // We rejected all the chars; try again:
                    if (s.Length == 0) continue;
                }

                // Uppercase the first letter of each word:
                if (requireUppercase && uppercaseOnlyFirstLetters)
                    s = Char.ToUpper(s[0]) + s.Substring(1);

                break;
            } while (true);
            return s;
        }

        /// <summary>
        /// Generates a passphrase consisting of a number of words strung together pulled from a dictionary at random.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Generate(SanitizedGeneratorOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            int minLength = options.MinLength;
            int maxLength = options.MaxLength;
            int minWordCount = options.MinWordCount;
            bool requireUppercase = options.RequireUppercase;
            bool uppercaseOnlyFirstLetters = options.UppercaseOnlyFirstLetters;
            bool requireNumber = options.RequireNumber;
            bool appendNumberToEndOnly = options.AppendNumberToEndOnly;
            char[] addThese = options.AddTheseChars;
            char[] removeThese = options.RemoveTheseChars;

            // Make sure that we read the number of words we expected:
            System.Diagnostics.Debug.Assert(dictionaryCount == wordList.Length);

            // Use a cryptographically strong random number generator:
            var rng = RNGCryptoServiceProvider.Create();
            Debug.Assert(rng != null);
            if (rng == null) throw new Exception("RNGCryptoServiceProvider.Create() failed and returned null!");

            StringBuilder sb;
            List<int> wordIndex;

            // Using a StringBuilder to construct the passphrase:
            sb = new StringBuilder(maxLength + 8);

            int iterCount = 0;
            do
            {
                sb.Remove(0, sb.Length);

                int randomExtraWords = rng.GetBitsInRange(6, Math.Max(2, (maxLength - minLength) / 3 + 1));
                int wordCount = minWordCount + (int)randomExtraWords;

                // Keep appending words until we reach our desired random word count:
                wordIndex = new List<int>(maxLength);
                for (int w = 0; w < wordCount; ++w)
                {
                    // Pick a word at random within the dictionary:
                    string s = PickWord(rng, removeThese, requireUppercase, uppercaseOnlyFirstLetters);

                    // Good to go to append word:
                    wordIndex.Add(sb.Length);
                    sb.Append(s);

                    if (sb.Length >= maxLength)
                        break;
                }

                // Not enough length? Keep adding words until we reach the minimum length:
                while (sb.Length < minLength)
                {
                    // Pick a word at random within the dictionary:
                    string s = PickWord(rng, removeThese, requireUppercase, uppercaseOnlyFirstLetters);

                    // Good to go to append word:
                    wordIndex.Add(sb.Length);
                    sb.Append(s);

                    if (sb.Length >= maxLength)
                        break;
                }

                if (requireNumber)
                {
                    if (appendNumberToEndOnly)
                    {
                        // Append a single digit to the end of the passphrase:
                        int n = rng.GetBitsInRange(4, 10);
                        char c = (char)('0' + n);
                        sb.Append(c);
                    }
                    else
                    {
                        int insertedAt = -1;
                        // Insert a number at random if there isn't one already:
                        if (!Enumerable.Range(0, sb.Length).Any(n => Char.IsNumber(sb[n])))
                        {
                            int v = rng.GetBitsInRange(6, wordIndex.Count + 1);
                            int n = rng.GetBitsInRange(4, 10);

                            char c = (char)('0' + n);
                            if (v == wordIndex.Count)
                                sb.Append(c);
                            else
                            {
                                sb.Insert(wordIndex[v], c);
                                insertedAt = v;
                            }
                        }

                        // Update wordIndexes:
                        if (insertedAt != -1)
                            for (int i = insertedAt; i < wordIndex.Count; ++i)
                                wordIndex[i]++;
                    }
                }

                if (addThese != null)
                {
                    int numbits = CountBits(addThese.Length);

                    // Insert a special char at random if there isn't one already:
                    if (!Enumerable.Range(0, sb.Length).Any(n => addThese.Contains(sb[n])))
                    {
                        int v = rng.GetBitsInRange(6, wordIndex.Count + 1);
                        int n = rng.GetBitsInRange(numbits, addThese.Length);

                        char c = addThese[n];
                        if (v == wordIndex.Count)
                            sb.Append(c);
                        else
                            sb.Insert(wordIndex[v], c);
                    }
                }

                // Capitalize a char at random if there isn't one already:
                if (requireUppercase && !uppercaseOnlyFirstLetters && !Enumerable.Range(0, sb.Length).Any(n => Char.IsUpper(sb[n])))
                {
                    while (true)
                    {
                        int v = rng.GetBitsInRange(6, sb.Length);
                        if (!Char.IsLower(sb[v])) continue;

                        // Uppercase the character:
                        sb[v] = Char.ToUpper(sb[v]);
                        break;
                    }
                }

                // Fail the passphrase and try again if any of these checks do not pass:
                ++iterCount;
                if (sb.Length < minLength) continue;
                if (sb.Length > maxLength) continue;
                if (wordIndex.Count < minWordCount) continue;

                if (requireNumber && !Enumerable.Range(0, sb.Length).Any(i => Char.IsNumber(sb[i]))) continue;
                if (requireUppercase && !Enumerable.Range(0, sb.Length).Any(i => Char.IsUpper(sb[i]))) continue;
                if ((removeThese != null) && !Enumerable.Range(0, sb.Length).All(i => !removeThese.Contains(sb[i]))) continue;
                if ((addThese != null) && !Enumerable.Range(0, sb.Length).Any(i => addThese.Contains(sb[i]))) continue;

                break;
            } while (iterCount < InconceivableIterationLimit);

            if (iterCount >= InconceivableIterationLimit)
                throw new Exception("Maximum of {0} iterations attempted while trying to satisfy passphrase generation policy; try reducing MinWordCount or increasing MaxLength.".F(InconceivableIterationLimit));

            Debug.Assert(sb.Length >= minLength);
            Debug.Assert(sb.Length <= maxLength);
            Debug.Assert(wordIndex.Count >= minWordCount);
            Debug.Assert(!requireNumber || Enumerable.Range(0, sb.Length).Any(i => Char.IsNumber(sb[i])));
            Debug.Assert(!requireUppercase || Enumerable.Range(0, sb.Length).Any(i => Char.IsUpper(sb[i])));
            Debug.Assert((removeThese == null) || Enumerable.Range(0, sb.Length).All(i => !removeThese.Contains(sb[i])));
            Debug.Assert((addThese == null) || Enumerable.Range(0, sb.Length).Any(i => addThese.Contains(sb[i])));

            return sb.ToString();
        }
    }
}
