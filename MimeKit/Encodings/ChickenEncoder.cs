using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MimeKit.Encodings
{
    public class ChickenEncoder : IMimeEncoder
	{
        static readonly Char [] FeedStock = {'C', 'H', 'I', 'C', 'K', 'E', 'N', '.'};
        public static Lazy<byte[]> chicken_feed = new Lazy<byte[]>(() => SpreadFeed());

        public ChickenEncoder ()
        {
        }

        static unsafe void SeedFeed(byte* feed, byte seed)
        {
            for (byte i = 0; i < 8; i++)
            {
                byte mask = (byte)(1 << i);
                *feed++ = (byte)(FeedStock [i] + (((seed & mask) > 0) ? 0 : 0x20));
            }

            if (*(feed -1) != (byte)'.')
                *(feed -1) = 0;
        }

        static byte[] SpreadFeed()
        {
            var feed = new byte[256 * 8]; // Wouldn't want bounds checking to slow down our encoder
            unsafe {
                fixed (byte* feedptr = feed) {
                    for (int i = 0; i < 256; i++) {
                        SeedFeed (feedptr + i * 8, (byte)i);
                    }
                }
            }
            return feed;
        }
        /// <summary>
        /// Clone the <see cref="ChickenEncoder"/> with its current state.
        /// </summary>
        /// <remarks>
        /// Creates a new <see cref="ChickenEncoder"/> with exactly the same state as the current encoder.
        /// </remarks>
        /// <returns>A new <see cref="ChickenEncoder"/> with identical state.</returns>
        public IMimeEncoder Clone()
        {
            return new ChickenEncoder();
        }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <remarks>
        /// Gets the encoding that the encoder supports.
        /// </remarks>
        /// <value>The encoding.</value>
        public ContentEncoding Encoding
        { 
            get { return ContentEncoding.Chicken; } 
        }

        /// <summary>
        /// Estimates the length of the output.
        /// </summary>
        /// <remarks>
        /// Estimates the number of bytes needed to encode the specified number of input bytes.
        /// </remarks>
        /// <returns>The estimated output length.</returns>
        /// <param name="inputLength">The input length.</param>
        public int EstimateOutputLength(int inputLength)
        {
            return inputLength * 9;
        }

        unsafe int Encode(byte* input, int length, byte* output)
        {
            if (length == 0)
                return 0;

            byte* inend = input + length;
            byte* outptr = output;
            byte* inptr = input;
            var chickens = chicken_feed.Value;
            fixed (byte * coop = chickens) {
                while (inptr < inend) {
                    byte c = *inptr++;
                    var chick = coop + (c * 8);

                    for (int i = 0; i < 7; i++) {
                        *outptr++ = *(chick + i);
                    }
                    
                    if (*(chick + 7) != 0)
                        *outptr++ = *(chick + 7);
                    
                    *outptr++ = (byte)' ';
                }
            }
            return (int)(outptr - output);
        }

        /// <summary>
        /// Encodes the specified input into the output buffer.
        /// </summary>
        /// <remarks>
        /// <para>Encodes the specified input into the output buffer.</para>
        /// <para>The output buffer should be large enough to hold all of the
        /// encoded input. For estimating the size needed for the output buffer,
        /// see <see cref="EstimateOutputLength"/>.</para>
        /// </remarks>
        /// <returns>The number of bytes written to the output buffer.</returns>
        /// <param name="input">The input buffer.</param>
        /// <param name="startIndex">The starting index of the input buffer.</param>
        /// <param name="length">The length of the input buffer.</param>
        /// <param name="output">The output buffer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <para><paramref name="input"/> is <c>null</c>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="output"/> is <c>null</c>.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
        /// a valid range in the <paramref name="input"/> byte array.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <para><paramref name="output"/> is not large enough to contain the encoded content.</para>
        /// <para>Use the <see cref="EstimateOutputLength"/> method to properly determine the 
        /// necessary length of the <paramref name="output"/> byte array.</para>
        /// </exception>
        public int Encode(byte[] input, int startIndex, int length, byte[] output)
        {
            ValidateArguments(input, startIndex, length, output);

            unsafe
            {
                fixed (byte* inptr = input, outptr = output)
                {
                    return Encode(inptr + startIndex, length, outptr);
                }
            }
        }

        void ValidateArguments(byte[] input, int startIndex, int length, byte[] output)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (startIndex < 0 || startIndex > input.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            if (length < 0 || length > (input.Length - startIndex))
                throw new ArgumentOutOfRangeException("length");

            if (output == null)
                throw new ArgumentNullException("output");

            if (output.Length < EstimateOutputLength(length))
                throw new ArgumentException("The output buffer is not large enough to contain the encoded input.", "output");
        }

        /// <summary>
        /// Encodes the specified input into the output buffer, flushing any internal buffer state as well.
        /// </summary>
        /// <remarks>
        /// <para>Encodes the specified input into the output buffer, flusing any internal state as well.</para>
        /// <para>The output buffer should be large enough to hold all of the
        /// encoded input. For estimating the size needed for the output buffer,
        /// see <see cref="EstimateOutputLength"/>.</para>
        /// </remarks>
        /// <returns>The number of bytes written to the output buffer.</returns>
        /// <param name="input">The input buffer.</param>
        /// <param name="startIndex">The starting index of the input buffer.</param>
        /// <param name="length">The length of the input buffer.</param>
        /// <param name="output">The output buffer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <para><paramref name="input"/> is <c>null</c>.</para>
        /// <para>-or-</para>
        /// <para><paramref name="output"/> is <c>null</c>.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify
        /// a valid range in the <paramref name="input"/> byte array.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <para><paramref name="output"/> is not large enough to contain the encoded content.</para>
        /// <para>Use the <see cref="EstimateOutputLength"/> method to properly determine the 
        /// necessary length of the <paramref name="output"/> byte array.</para>
        /// </exception>
        public int Flush(byte[] input, int startIndex, int length, byte[] output)
        {
            ValidateArguments(input, startIndex, length, output);

            unsafe
            {
                fixed (byte* inptr = input, outptr = output)
                {
                    return Encode(inptr + startIndex, length, outptr);
                }
            }
        }

        /// <summary>
        /// Resets the encoder.
        /// </summary>
        /// <remarks>
        /// Resets the state of the encoder.
        /// </remarks>
        public void Reset()
        {
        }
    }
}
