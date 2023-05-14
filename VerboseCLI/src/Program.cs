using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using VerboseCLI.Entities;
using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCore.Exceptions;

namespace VerboseCLI
{
    class Program
    {
        static readonly Dictionary<string, Locale> LocaleSet = new(){{ Locale.LocaleEN.Name.ToLower(), Locale.LocaleEN}};

        static void ReadLocale(string path)
        {
            if (XmlParserLocale.Parse(File.OpenRead(path), out var locale))
            {
                LocaleSet.Add(locale.Name.ToLower(), locale);
                Shared.LOCALE = locale.Name.ToLower();
            };
        }

        static void ParseArgs(string[] args)
        {
            var arg = args.GetEnumerator();
            var finished = false;
            if ((finished = !arg.MoveNext()))
                return;


            while (!finished)
            {
                switch ((string)arg.Current)
                {
                    case "--input":
                    case "-i":
                        {
                            if (!arg.MoveNext())
                                Console.WriteLine("No source string provided! Exiting...");

                            var stream = new MemoryStream();
                            var writer = new StreamWriter(stream);
                            writer.Write((string)arg.Current);
                            writer.Flush();
                            stream.Position = 0;
                            Shared.CLI_SOURCE = stream;
                            if (!arg.MoveNext())
                                return;
                        }
                        break;
                    case "--file":
                    case "-f":
                        {
                            if (!arg.MoveNext())
                                Console.WriteLine("No path provided! Exiting...");
                            try
                            {
                                Shared.CLI_SOURCE = File.OpenRead((string)arg.Current);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Incorrect path or file provided! Exiting...");
                                Shared.FATAL = true;
                                return;
                            }
                            finished = !arg.MoveNext();
                        }
                        break;
                    case "--locale":
                    case "-l":
                        {
                            if (!arg.MoveNext())
                                Console.WriteLine("No path provided! Exiting...");
                            try
                            {
                                ReadLocale((string)arg.Current);
                            }
                            catch
                            {
                                Console.WriteLine("Incorrect path provided! Exiting...");
                                Shared.FATAL = true;
                                return;
                            }
                            finished = !arg.MoveNext();
                        }
                        break;
                    case "--out":
                    case "-o":
                        {
                            if (!arg.MoveNext())
                                Console.WriteLine("No path provided! Exiting...");
                            try
                            {
                                Shared.CLI_STDOUT = new StreamWriter((string)arg.Current);
                            }
                            catch
                            {
                                Console.WriteLine("Incorrect path or file provided! Exiting...");
                                Shared.FATAL = true;
                                return;
                            }
                            finished = !arg.MoveNext();
                        }
                        break;
                    case "--err":
                    case "-e":
                        {
                            if (!arg.MoveNext())
                                Console.WriteLine("No path provided! Exiting...");
                            try
                            {
                                Shared.CLI_STDERR = new StreamWriter((string)arg.Current);
                            }
                            catch
                            {
                                Console.WriteLine("Incorrect path or file provided! Exiting...");
                                Shared.FATAL = true;
                                return;
                            }
                            finished = !arg.MoveNext();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            ParseArgs(args);
            if (Shared.FATAL)
                return;

            if(!LocaleSet.TryGetValue(Shared.LOCALE, out var locale))
            {
                locale = Locale.LocaleEN;
            }

            var logger = new Logger(locale);
            logger.StdOut = Shared.CLI_STDOUT;
            logger.StdErr = Shared.CLI_STDERR;
            logger.EmitLog(LogType.Initialized, new Position(), "", new() { DateTime.Now });

            if (Shared.CLI_SOURCE == null)
            {
                logger.EmitError(ErrorType.NoSource, new Position(), "", new() { });
                return;
            }

            try
            {
                var interpreter = new Interpreter.Interpreter(Shared.CLI_SOURCE, logger);
                interpreter.BuildProgram();
                interpreter.RunProgram();
            }
            catch (LexerError)
            {
            }
            catch (ParserError)
            {
            }
        }
    }
}
