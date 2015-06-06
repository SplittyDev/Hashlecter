using System;
using System.Text;
using MD5x = System.Security.Cryptography.MD5;

namespace hashlecter
{
	public abstract partial class HashingMethod {

		/// <summary>
		/// Gets the hashing algorithm.
		/// </summary>
		/// <value>The hashing algorithm.</value>
		public abstract HashingAlgorithm Algorithm { get; }

		/// <summary>
		/// Name of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string Name { get; }

		/// <summary>
		/// Friendly name of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string FriendlyName { get; }

		/// <summary>
		/// Format of the hashing method
		/// </summary>
		/// <value></value>
		public abstract string Format { get; }

		/// <summary>
		/// Checks the hash.
		/// </summary>
		/// <returns><c>true</c>, if the hashes collided, <c>false</c> otherwise.</returns>
		/// <param name="refhash">Original hash.</param>
		/// <param name="input">Input.</param>
		/// <param name="output">Output.</param>
		public abstract bool CheckHash (string refhash, string input, out string output);

		/// <summary>
		/// Creates a new instance of type <typeparamref name="TMethod" />
		/// </summary>
		/// <typeparam name="TMethod">The 1st type parameter.</typeparam>
		public static TMethod New<TMethod> () where TMethod : HashingMethod, new() {
			return new TMethod ();
		}
	}
}

