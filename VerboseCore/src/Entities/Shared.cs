using System;
using System.IO;

namespace VerboseCore.Entities
{
    public static class Shared
    {
        public static int MAX_TEXT_SIZE = 100;
        public static int MAX_COMMENT_SIZE = 100;
        public static int MAX_IDENTIFIER_SIZE = 100;
        public static double MAX_NUMBER_VALUE = double.MaxValue;

        public static double EPSILON = 1e-9;

        public static Stream? CLI_SOURCE = null;
        public static TextWriter CLI_STDOUT = Console.Out;
        public static TextWriter CLI_STDERR = Console.Error;
        public static bool FATAL = false;
        public static string LOCALE = "en-us";
    }
}
