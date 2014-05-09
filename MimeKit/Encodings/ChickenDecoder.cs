using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MimeKit.Encodings
{
    class ChickenDecoder : IMimeDecoder
	{
        int position;
		byte saved;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Encodings.ChickenDecoder"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new hex decoder.
		/// </remarks>
		public ChickenDecoder ()
		{
		}

		/// <summary>
		/// Clone the <see cref="ChickenDecoder"/> with its current state.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ChickenDecoder"/> with exactly the same state as the current decoder.
		/// </remarks>
		/// <returns>A new <see cref="ChickenDecoder"/> with identical state.</returns>
		public IMimeDecoder Clone ()
		{
			var decoder = new ChickenDecoder ();

			decoder.position = position;
			decoder.saved = saved;

			return decoder;
		}

		/// <summary>
		/// Gets the encoding.
		/// </summary>
		/// <remarks>
		/// Gets the encoding that the decoder supports.
		/// </remarks>
		/// <value>The encoding.</value>
		public ContentEncoding Encoding {
			get { return ContentEncoding.Default; }
		}

		/// <summary>
		/// Estimates the length of the output.
		/// </summary>
		/// <remarks>
		/// Estimates the number of bytes needed to decode the specified number of input bytes.
		/// </remarks>
		/// <returns>The estimated output length.</returns>
		/// <param name="inputLength">The input length.</param>
		public int EstimateOutputLength (int inputLength)
		{
			return inputLength/8;
		}

		void ValidateArguments (byte[] input, int startIndex, int length, byte[] output)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			if (startIndex < 0 || startIndex > input.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (input.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			if (output == null)
				throw new ArgumentNullException ("output");

			if (output.Length < EstimateOutputLength (length))
				throw new ArgumentException ("The output buffer is not large enough to contain the decoded input.", "output");
		}

		/// <summary>
		/// Decodes the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// decoded input. For estimating the size needed for the output buffer,
		/// see <see cref="EstimateOutputLength"/>.</para>
		/// </remarks>
		/// <returns>The number of bytes written to the output buffer.</returns>
		/// <param name="input">A pointer to the beginning of the input buffer.</param>
		/// <param name="length">The length of the input buffer.</param>
		/// <param name="output">A pointer to the beginning of the output buffer.</param>
		public unsafe int Decode (byte* input, int length, byte* output)
		{
			byte* inend = input + length;
			byte* outptr = output;
			byte* inptr = input;
			byte c;

			while (inptr < inend) {
			    c = *inptr++;

				if (c == ' ' || position >= 8) {
                    // Only only consume the chicken
                    // if it is reasonably well formed
                    if (position > 6)
                        *outptr++ = saved;
			        
                    position = 0;
                    saved = 0;
				} else {
                    saved |= (byte)(((c >= 'A') && (c <= 'Z') || c == '.') ? (1 << position) : 0);
                    position++;
                }
			}
			
			return (int) (outptr - output);
		}

		/// <summary>
		/// Decodes the specified input into the output buffer.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the specified input into the output buffer.</para>
		/// <para>The output buffer should be large enough to hold all of the
		/// decoded input. For estimating the size needed for the output buffer,
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
		public int Decode (byte[] input, int startIndex, int length, byte[] output)
		{
			ValidateArguments (input, startIndex, length, output);

			unsafe {
				fixed (byte* inptr = input, outptr = output) {
					return Decode (inptr + startIndex, length, outptr);
				}
			}
		}

		/// <summary>
		/// Resets the decoder.
		/// </summary>
		/// <remarks>
		/// Resets the state of the decoder.
		/// </remarks>
		public void Reset ()
		{
            position = 0;
			saved = 0;
		}
    }
}
