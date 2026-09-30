using System;
using System.Text;

namespace Eco.Core.Sessions
{
    public static class SessionTokenGenerator
    {
        private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public static string Generate(int length = 8, Random random = null)
        {
            random ??= new Random();
            var builder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                builder.Append(Alphabet[random.Next(Alphabet.Length)]);
            return builder.ToString();
        }
    }
}
