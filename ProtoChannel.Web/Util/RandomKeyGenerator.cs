using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoChannel.Web.Util
{
    internal static class RandomKeyGenerator
    {
        private const string KeyChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        private static readonly object _syncRoot = new object();
        private static readonly HashSet<string> _usedKeys = new HashSet<string>();
        private static readonly Random _random = new Random();

        public static string GetRandomKey(int length)
        {
            lock (_syncRoot)
            {
                while (true)
                {
                    var sb = new StringBuilder(length);

                    for (int i = 0; i < length; i++)
                    {
                        sb.Append(KeyChars[_random.Next(KeyChars.Length - 1)]);
                    }

                    string key = sb.ToString();

                    if (!_usedKeys.Contains(key))
                    {
                        _usedKeys.Add(key);

                        return key;
                    }
                }
            }
        }
    }
}
