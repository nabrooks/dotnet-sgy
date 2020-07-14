using System;

namespace Utility.Extensions
{
    /// <summary>
    /// Utility methods for working with floats and doubles
    /// </summary>
    /// <remarks>
    /// Many of the methods in here take advantage of IEEE floating point storage.  Here is a good article about that:
    /// <para>
    /// From http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm:
    /// </para>
    /// <para>
    /// There is an alternate technique for checking whether two floating point numbers are close to each other. 
    /// Recall that the problem with absolute error checks is that they don't take into consideration whether there 
    /// are any values in the range being checked. That is, with an allowable absolute error of 0.00001 and an expectedResult of 10,000 
    /// we are saying that we will accept any number in the range 9,999.99999 to 10,000.00001, without 
    /// realizing that when using 4-byte floats there is only one representable float in that range – 10,000. 
    /// Wouldn't it be handy if we could easily specify our error range in terms of how many floats we want in 
    /// that range? That is, wouldn't it be convenient if we could say "I think the answer is 10,000 but
    /// since floating point math is imperfect I'll accept the 5 floats above and the 5 floats below that value.
    /// It turns out there is an easy way to do this.
    /// The IEEE float and double formats were designed so that the numbers are "lexicographically ordered", 
    /// which – in the words of IEEE architect William Kahan means "if two floating-point numbers in the same
    /// format are ordered ( say x &lt; y ), then they are ordered the same way when their bits are 
    /// reinterpreted as Sign-Magnitude integers."
    /// </para>
    /// </remarks>
    public static class FloatingPointExtensions
    {
        private const long _LONG_MINUS_SIGN = 1L << 63;
        private const int _INT_MINUS_SIGN = 1 << 31;

        #region Interpolation


        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static double Lerp(this double from, double to, double amount)
        {
            return (1 - amount) * from + amount * to;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static float Lerp(this float from, float to, float amount)
        {
            return (1 - amount) * from + amount * to;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static byte Lerp(this byte from, byte to, float amount)
        {
            return (byte)Lerp((float)from, (float)to, amount);
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmoothStep(this float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * (3 - (2 * amount));
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static double SmoothStep(this double amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * (3 - (2 * amount));
        }

        /// <summary>
        /// Performs a smooth(er) interpolation between 0 and 1 with 1st and 2nd order derivatives of zero at endpoints.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmootherStep(this float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * amount * (amount * ((amount * 6) - 15) + 10);
        }

        /// <summary>
        /// Performs a smooth(er) interpolation between 0 and 1 with 1st and 2nd order derivatives of zero at endpoints.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static double SmootherStep(this double amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * amount * (amount * ((amount * 6) - 15) + 10);
        }

        #endregion


        /// <summary>
        /// Returns the next representable floating point value that is larger than <paramref name="value"/>.
        /// Use this when you need a number that is only "slightly" bigger than your current number.
        /// Returns <see cref="float.PositiveInfinity"/> if <paramref name="value"/> == <see cref="float.MaxValue"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the next larger value, or infinity if given MaxValue</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures", Justification = "Static analyzer has no chance to validate this")]
        public static float NextLarger(this float value)
        {
            if (value == float.MaxValue)
            {
                return float.PositiveInfinity;
            }

            return NextValue(value, 1);
        }

        /// <summary>
        /// Returns the next representable floating point value that is smaller than <paramref name="value"/>.
        /// Use this when you need a number that is only "slightly" smaller than your current number.
        /// Returns <see cref="float.NegativeInfinity"/> if <paramref name="value"/> == <see cref="float.MinValue"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the next larger value, or infinity if given MaxValue</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures", Justification = "Static analyzer has no chance to validate this")]
        public static float NextSmaller(this float value)
        {
            if (value == float.MinValue)
            {
                return float.NegativeInfinity;
            }

            return NextValue(value, -1);
        }

        /// <summary>
        /// Returns the next representable floating point value that is larger than <paramref name="value"/>.
        /// Use this when you need a number that is only "slightly" bigger than your current number.
        /// Returns <see cref="double.PositiveInfinity"/> if <paramref name="value"/> == <see cref="double.MaxValue"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the next larger value, or infinity if given MaxValue</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures", Justification = "Static analyzer has no chance to validate this")]
        public static double NextLarger(this double value)
        {
            if (value == double.MaxValue)
            {
                return double.PositiveInfinity;
            }

            return NextValue(value, 1);
        }

        /// <summary>
        /// Returns the next representable floating point value that is smaller than <paramref name="value"/>.
        /// Use this when you need a number that is only "slightly" smaller than your current number.
        /// Returns <see cref="double.NegativeInfinity"/> if <paramref name="value"/> == <see cref="double.MinValue"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the next larger value, or infinity if given MaxValue</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures", Justification = "Static analyzer has no chance to validate this")]
        public static double NextSmaller(this double value)
        {
            if (value == double.MinValue)
            {
                return double.NegativeInfinity;
            }

            return NextValue(value, -1);
        }

        private static float NextValue(float value, int ulps)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) { return value; }

            // Reinterpret the bytes of the float as an integer
            var ivalue = Reinterpret(value);

            // Add ulps to it and convert back to float.  This will yield the next representable floating point value
            ivalue += ulps;

            return Reinterpret(ivalue);
        }

        private static double NextValue(double value, long ulps)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) { return value; }

            // Reinterpret the bytes of the double as an integer
            var ivalue = Reinterpret(value);

            // Add ulps to it and convert back to double.  This will yield the next representable floating point value
            ivalue += ulps;

            return Reinterpret(ivalue);
        }

        /// <summary>
        /// Reinterprets the bytes that make up the integer as an IEEE float.
        /// </summary>
        /// <returns>Returns the IEEE float represented by the twos-complement integer byte pattern</returns>
        private static unsafe float Reinterpret(int value)
        {
            // adjust negative integers back to sign-magnitude integers
            if (value < 0)
            {
                value = _INT_MINUS_SIGN - value;
            }

            return *(float*)(&value);
        }

        /// <summary>
        /// Reinterprets the bytes that make up the IEEE float as an integer.
        /// </summary>
        /// <returns>Returns the twos-complement integer represented by the IEEE Float byte pattern</returns>
        private static unsafe int Reinterpret(float value)
        {
            int i = *(int*)(&value);

            // adjust negative numbers to be lexographically ordered as twos-complement ints
            if (i < 0)
            {
                i = _INT_MINUS_SIGN - i;
            }

            return i;
        }

        /// <summary>
        /// Reinterprets the bytes that make up the integer as an IEEE double.
        /// </summary>
        /// <returns>Returns the IEEE double represented by the twos-complement integer byte pattern</returns>
        private static double Reinterpret(long value)
        {
            // adjust negative integers back to sign-magnitude integers
            if (value < 0)
            {
                value = _LONG_MINUS_SIGN - value;
            }

            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Reinterprets the bytes that make up the IEEE double as an integer.
        /// </summary>
        /// <returns>Returns the twos-complement integer represented by the IEEE double byte pattern</returns>
        private static long Reinterpret(double value)
        {
            var i = BitConverter.DoubleToInt64Bits(value);

            // adjust negative numbers to be lexographically ordered as twos-complement ints
            if (i < 0)
            {
                i = _LONG_MINUS_SIGN - i;
            }

            return i;
        }
    }
}
