// <copyright file="Precision.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;

#if PORTABLE
using System.Runtime.InteropServices;
#endif

namespace MathNet.Numerics
{

    /// <summary>
    /// Utilities for working with floating point numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful links:
    /// <list type="bullet">
    /// <item>
    /// http://docs.sun.com/source/806-3568/ncg_goldberg.html#689 - What every computer scientist should know about floating-point arithmetic
    /// </item>
    /// <item>
    /// http://en.wikipedia.org/wiki/Machine_epsilon - Gives the definition of machine epsilon
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public static partial class Precision
    {
        /// <summary>
        /// The number of binary digits used to represent the binary number for a double precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int DoubleWidth = 53;

        /// <summary>
        /// The number of binary digits used to represent the binary number for a single precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int SingleWidth = 24;

        /// <summary>
        /// The maximum relative precision of of double-precision floating numbers (64 bit)
        /// </summary>
        public static readonly double DoublePrecision = Math.Pow(2, -DoubleWidth);

        /// <summary>
        /// The maximum relative precision of of single-precision floating numbers (32 bit)
        /// </summary>
        public static readonly double SinglePrecision = Math.Pow(2, -SingleWidth);

        /// <summary>
        /// The number of significant decimal places of double-precision floating numbers (64 bit).
        /// </summary>
        public static readonly int DoubleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(DoublePrecision)));

        /// <summary>
        /// The number of significant decimal places of single-precision floating numbers (32 bit).
        /// </summary>
        public static readonly int SingleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(SinglePrecision)));

        /// <summary>
        /// Value representing 10 * 2^(-53) = 1.11022302462516E-15
        /// </summary>
        static readonly double DefaultDoubleAccuracy = DoublePrecision*10;

        /// <summary>
        /// Value representing 10 * 2^(-24) = 5.96046447753906E-07
        /// </summary>
        static readonly float DefaultSingleAccuracy = (float) (SinglePrecision*10);

        /// <summary>
        /// Returns the magnitude of the number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The magnitude of the number.</returns>
        public static int Magnitude(this double value)
        {
            // Can't do this with zero because the 10-log of zero doesn't exist.
            if (value.Equals(0.0))
            {
                return 0;
            }

            // Note that we need the absolute value of the input because Log10 doesn't
            // work for negative numbers (obviously).
            double magnitude = Math.Log10(Math.Abs(value));

#if PORTABLE
            var truncated = (int)Truncate(magnitude);
#else
            var truncated = (int) Math.Truncate(magnitude);
#endif

            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1 (unless integer)
            return magnitude < 0d && truncated != magnitude
                ? truncated - 1
                : truncated;
        }


        /// <summary>
        /// Returns the magnitude of the number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The magnitude of the number.</returns>
        public static int Magnitude(this float value)
        {
            // Can't do this with zero because the 10-log of zero doesn't exist.
            if (value.Equals(0.0f))
            {
                return 0;
            }

            // Note that we need the absolute value of the input because Log10 doesn't
            // work for negative numbers (obviously).
            var magnitude = Convert.ToSingle(Math.Log10(Math.Abs(value)));

#if PORTABLE
            var truncated = (int)Truncate(magnitude);
#else
            var truncated = (int) Math.Truncate(magnitude);
#endif

            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1 (unless integer)
            return magnitude < 0f && truncated != magnitude
                ? truncated - 1
                : truncated;
        }

        /// <summary>
        /// Returns the number divided by it's magnitude, effectively returning a number between -10 and 10.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value of the number.</returns>
        public static double ScaleUnitMagnitude(this double value)
        {
            if (value.Equals(0.0))
            {
                return value;
            }

            int magnitude = Magnitude(value);
            return value*Math.Pow(10, -magnitude);
        }

        /// <summary>
        /// Gets the equivalent <c>long</c> value for the given <c>double</c> value.
        /// </summary>
        /// <param name="value">The <c>double</c> value which should be turned into a <c>long</c> value.</param>
        /// <returns>
        /// The resulting <c>long</c> value.
        /// </returns>
        static long AsInt64(double value)
        {
#if PORTABLE
            return DoubleToInt64Bits(value);
#else
            return BitConverter.DoubleToInt64Bits(value);
#endif
        }

        /// <summary>
        /// Returns a 'directional' long value. This is a long value which acts the same as a double,
        /// e.g. a negative double value will return a negative double value starting at 0 and going
        /// more negative as the double value gets more negative.
        /// </summary>
        /// <param name="value">The input double value.</param>
        /// <returns>A long value which is roughly the equivalent of the double value.</returns>
        static long AsDirectionalInt64(double value)
        {
            // Convert in the normal way.
            long result = AsInt64(value);

            // Now find out where we're at in the range
            // If the value is larger/equal to zero then we can just return the value
            // if the value is negative we subtract long.MinValue from it.
            return (result >= 0) ? result : (long.MinValue - result);
        }

        /// <summary>
        /// Returns a 'directional' int value. This is a int value which acts the same as a float,
        /// e.g. a negative float value will return a negative int value starting at 0 and going
        /// more negative as the float value gets more negative.
        /// </summary>
        /// <param name="value">The input float value.</param>
        /// <returns>An int value which is roughly the equivalent of the double value.</returns>
        static int AsDirectionalInt32(float value)
        {
            // Convert in the normal way.
            int result = FloatToInt32Bits(value);

            // Now find out where we're at in the range
            // If the value is larger/equal to zero then we can just return the value
            // if the value is negative we subtract int.MinValue from it.
            return (result >= 0) ? result : (int.MinValue - result);
        }

        /// <summary>
        /// Increments a floating point number to the next bigger number representable by the data type.
        /// </summary>
        /// <param name="value">The value which needs to be incremented.</param>
        /// <param name="count">How many times the number should be incremented.</param>
        /// <remarks>
        /// The incrementation step length depends on the provided value.
        /// Increment(double.MaxValue) will return positive infinity.
        /// </remarks>
        /// <returns>The next larger floating point value.</returns>
        public static double Increment(this double value, int count)
        {
            if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
            {
                return value;
            }

            if (count < 0)
            {
                return Decrement(value, -count);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double 
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = AsInt64(value);
            if (intValue < 0)
            {
                intValue -= count;
            }
            else
            {
                intValue += count;
            }

            // Note that long.MinValue has the same bit pattern as -0.0.
            if (intValue == long.MinValue)
            {
                return 0;
            }

            // Note that not all long values can be translated into double values. There's a whole bunch of them 
            // which return weird values like infinity and NaN
#if PORTABLE
            return Int64BitsToDouble(intValue);
#else
            return BitConverter.Int64BitsToDouble(intValue);
#endif
        }

        /// <summary>
        /// Decrements a floating point number to the next smaller number representable by the data type.
        /// </summary>
        /// <param name="value">The value which should be decremented.</param>
        /// <param name="count">How many times the number should be decremented.</param>
        /// <remarks>
        /// The decrementation step length depends on the provided value.
        /// Decrement(double.MinValue) will return negative infinity.
        /// </remarks>
        /// <returns>The next smaller floating point value.</returns>
        public static double Decrement(this double value, int count)
        {
            if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
            {
                return value;
            }

            if (count < 0)
            {
                return Decrement(value, -count);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double 
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = AsInt64(value);

            // If the value is zero then we'd really like the value to be -0. So we'll make it -0 
            // and then everything else should work out.
            if (intValue == 0)
            {
                // Note that long.MinValue has the same bit pattern as -0.0.
                intValue = long.MinValue;
            }

            if (intValue < 0)
            {
                intValue += count;
            }
            else
            {
                intValue -= count;
            }

            // Note that not all long values can be translated into double values. There's a whole bunch of them 
            // which return weird values like infinity and NaN
#if PORTABLE
            return Int64BitsToDouble(intValue);
#else
            return BitConverter.Int64BitsToDouble(intValue);
#endif
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the zero and the number <paramref name="a"/>.</param>
        /// <returns>
        ///     Zero if |<paramref name="a"/>| is fewer than <paramref name="maxNumbersBetween"/> numbers from zero, <paramref name="a"/> otherwise.
        /// </returns>
        public static double CoerceZero(this double a, int maxNumbersBetween)
        {
            return CoerceZero(a, (long) maxNumbersBetween);
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the zero and the number <paramref name="a"/>.</param>
        /// <returns>
        ///     Zero if |<paramref name="a"/>| is fewer than <paramref name="maxNumbersBetween"/> numbers from zero, <paramref name="a"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than zero.
        /// </exception>
        public static double CoerceZero(this double a, long maxNumbersBetween)
        {
            if (maxNumbersBetween < 0)
            {
                throw new ArgumentOutOfRangeException("maxNumbersBetween");
            }

            if (double.IsInfinity(a) || double.IsNaN(a))
            {
                return a;
            }

            // We allow maxNumbersBetween between 0 and the number so
            // we need to check if there a
            if (NumbersBetween(0.0, a) <= (ulong) maxNumbersBetween)
            {
                return 0.0;
            }

            return a;
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maximumAbsoluteError">The absolute threshold for <paramref name="a"/> to consider it as zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than <paramref name="maximumAbsoluteError"/>, <paramref name="a"/> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maximumAbsoluteError"/> is smaller than zero.
        /// </exception>
        public static double CoerceZero(this double a, double maximumAbsoluteError)
        {
            if (maximumAbsoluteError < 0)
            {
                throw new ArgumentOutOfRangeException("maximumAbsoluteError");
            }

            if (double.IsInfinity(a) || double.IsNaN(a))
            {
                return a;
            }

            if (Math.Abs(a) < maximumAbsoluteError)
            {
                return 0.0;
            }

            return a;
        }

        /// <summary>
        /// Forces small numbers near zero to zero.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than 2^(-53) = 1.11e-16, <paramref name="a"/> otherwise.</returns>
        public static double CoerceZero(this double a)
        {
            return CoerceZero(a, DoublePrecision);
        }

        /// <summary>
        /// Evaluates the count of numbers between two double numbers
        /// </summary>
        /// <param name="a">The first parameter.</param>
        /// <param name="b">The second parameter.</param>
        /// <remarks>The second number is included in the number, thus two equal numbers evaluate to zero and two neighbor numbers evaluate to one. Therefore, what is returned is actually the count of numbers between plus 1.</remarks>
        /// <returns>The number of floating point values between <paramref name="a"/> and <paramref name="b"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="a"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="a"/> is <c>double.NaN</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="b"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="b"/> is <c>double.NaN</c>.
        /// </exception>
        public static ulong NumbersBetween(this double a, double b)
        {
            if (double.IsNaN(a) || double.IsInfinity(a))
            {
                throw new ArgumentOutOfRangeException("a");
            }

            if (double.IsNaN(b) || double.IsInfinity(b))
            {
                throw new ArgumentOutOfRangeException("b");
            }

            // Calculate the ulps for the maximum and minimum values
            // Note that these can overflow
            long intA = AsDirectionalInt64(a);
            long intB = AsDirectionalInt64(b);

            // Now find the number of values between the two doubles. This should not overflow
            // given that there are more long values than there are double values
            return (a >= b) ? (ulong) (intA - intB) : (ulong) (intB - intA);
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>
        /// Relative Epsilon (positive double or NaN).
        /// </returns>
        /// <remarks>Evaluates the <b>negative</b> epsilon. The more common positive epsilon is equal to two times this negative epsilon.</remarks>
        /// <seealso cref="PositiveEpsilonOf(double)"/>
        public static double EpsilonOf(this double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return double.NaN;
            }

#if PORTABLE
            long signed64 = DoubleToInt64Bits(value);
            if (signed64 == 0)
            {
                signed64++;
                return Int64BitsToDouble(signed64) - value;
            }
            if (signed64-- < 0)
            {
                return Int64BitsToDouble(signed64) - value;
            }
            return value - Int64BitsToDouble(signed64);
#else
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if (signed64 == 0)
            {
                signed64++;
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }
            if (signed64-- < 0)
            {
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }
            return value - BitConverter.Int64BitsToDouble(signed64);
#endif
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>Relative Epsilon (positive double or NaN)</returns>
        /// <remarks>Evaluates the <b>positive</b> epsilon. See also <see cref="EpsilonOf"/></remarks>
        /// <seealso cref="EpsilonOf(double)"/>
        public static double PositiveEpsilonOf(this double value)
        {
            return 2*EpsilonOf(value);
        }

        /// <summary>
        /// Converts a float valut to a bit array stored in an int.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The bit array.</returns>
        static int FloatToInt32Bits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

#if PORTABLE
        static long DoubleToInt64Bits(double value)
        {
            var union = new DoubleLongUnion {Double = value};
            return union.Int64;
        }

        static double Int64BitsToDouble(long value)
        {
            var union = new DoubleLongUnion {Int64 = value};
            return union.Double;
        }

        static double Truncate(double value)
        {
            return value >= 0.0 ? Math.Floor(value) : Math.Ceiling(value);
        }

        [StructLayout(LayoutKind.Explicit)]
        struct DoubleLongUnion
        {
            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public long Int64;
        }
#endif
    }
}
