using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Security.Cryptography
{
    public static class RandomNumberGeneratorExtensions
    {
        [ThreadStatic]
        static byte[] bytes1 = null;
        [ThreadStatic]
        static byte[] bytes2 = null;
        [ThreadStatic]
        static byte[] bytes3 = null;
        [ThreadStatic]
        static byte[] bytes4 = null;

        /// <summary>
        /// Gets a random value in the range [0 .. (2^bits) - 1] from the random number generator.
        /// </summary>
        /// <param name="rng">Random number generator instance to pull random bits from</param>
        /// <param name="bits">Number of bits [1..31] to use</param>
        /// <remarks>
        /// Discards extra bits of the last byte pulled from the RNG.
        /// </remarks>
        /// <returns></returns>
        public static int GetBits(this RandomNumberGenerator rng, int bits)
        {
            Debug.Assert(rng != null);

            if (rng == null) throw new ArgumentNullException("rng");
            if (bits < 0 || bits > 31) throw new ArgumentOutOfRangeException();
            if (bits == 0) return 0;

            // Calculate number of bytes required from the RNG:
            int numbytes = ((bits + 7) & ~7) >> 3;

            uint final;
            uint mask = (1U << bits) - 1;

            // We read the bits from the RNG in big-endian order, which makes sense from a streaming perspective.
            byte[] bytes;
            switch (numbytes)
            {
                case 1:
                    if (bytes1 == null)
                        bytes1 = new byte[1];
                    bytes = bytes1;
                    rng.GetBytes(bytes);
                    final = bytes[0] & mask;
                    break;
                case 2:
                    if (bytes2 == null)
                        bytes2 = new byte[2];
                    bytes = bytes2;
                    rng.GetBytes(bytes);
                    final = (bytes[0] | ((uint)bytes[1] << 8)) & mask;
                    break;
                case 3:
                    if (bytes3 == null)
                        bytes3 = new byte[3];
                    bytes = bytes3;
                    rng.GetBytes(bytes);
                    final = (bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16)) & mask;
                    break;
                case 4:
                    if (bytes4 == null)
                        bytes4 = new byte[4];
                    bytes = bytes4;
                    rng.GetBytes(bytes);
                    final = (bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16) | ((uint)bytes[3] << 24)) & mask;
                    break;
                default:
                    throw new Exception("GetBits calculated too many bytes required!");
            }

            Debug.Assert(final < (1U << bits));

            return (int)final;
        }

        /// <summary>
        /// Uses rejection sampling to get a random value in the range [0 .. max - 1].
        /// </summary>
        /// <param name="rng">Random number generator instance to pull random bits from</param>
        /// <param name="bits">Number of bits to approximate max</param>
        /// <param name="max">Exclusive upper bound</param>
        /// <returns></returns>
        public static int GetBitsInRange(this RandomNumberGenerator rng, int bits, int max)
        {
            Debug.Assert(rng != null);

            if (rng == null) throw new ArgumentNullException("rng");
            if (max <= 1) return 0;

            // Prevent the loop from (theoretically) running forever:
            int iterCount = 0;

            // Sample random values until we come upon one within range or until we iterate too much:
            int v;
            do
            {
                v = GetBits(rng, bits);
                ++iterCount;
            } while ((v >= max) && (iterCount <= 20));

            // If we did 20 iterations, take the last sample and scale it down to range:
            if (iterCount > 20)
                v = v * (max - 1) / (1 << bits);

            Debug.Assert(v < max);

            return v;
        }
    }
}
