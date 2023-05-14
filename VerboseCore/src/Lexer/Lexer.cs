using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;

namespace VerboseCore.Lexer
{
    public partial class Lexer: ILexer
    {
        protected readonly ILogger _logger;
        protected readonly ILexerScanner _scanner;
        protected IToken _token;

        public string BufferedError { get => _scanner.BufferedError; }

        public Lexer(ILexerScanner scanner, ILogger logger)
        {
            _scanner = scanner;
            _logger = logger;
            _token = new Token(TokenType.SpecialNUL, _scanner.Position);
        }

        public Lexer(Stream stream, ILogger logger)
        {
            _scanner = new LexerScanner(stream);
            _logger = logger;
            _token = new Token(TokenType.SpecialNUL, _scanner.Position);
        }

        public IToken BuildToken()
        {
            try
            {
                _scanner.ClearBufferedError();
                _scanner.SkipWhites();
                var token = BuildETX()
                      ?? BuildNewline()
                      ?? BuildIdOrKeyword()
                      ?? BuildOperator()
                      ?? BuildNumber()
                      ?? BuildText()
                      ?? BuildComment();
                if (token == null)
                    throw new LexerError(ErrorType.UnknownToken);
                _token = token;
            }
            catch (LexerError e)
            {
                _token = BuildUnknownBuffered();
                _logger.EmitError(e.Error, _token.Position, _token.Value!.ToString()!, e.Values);
                throw e;
            }

            return _token;
        }

        /************************** TOKEN BUILDERS  **************************/
        IToken? BuildETX()
        {
            return _scanner.Buffer == '\uffff' ? new Token(TokenType.SpecialETX, _scanner.Position) : null;
        }

        IToken? BuildNewline()
        {
            Position pos = new(_scanner.Position);
            return _scanner.TryNewline() ? new Token(TokenType.SpecialEOL, pos) : null;
        }

        public IToken? BuildIdOrKeyword()
        {
            Position pos = new(_scanner.Position);
            if (!Char.IsLetter(_scanner.Buffer) && _scanner.Buffer != '_') return null;

            string str = BuildLimitedString(c => Char.IsLetterOrDigit(c) || c == '_', Shared.MAX_IDENTIFIER_SIZE);

            Token makeToken(TokenType tt, object? val) { return new Token(tt, pos, val); }

            return KeywordDict.TryGetValue(str, out TokenType type) ?
                type switch
                {
                    TokenType.KeywordTrue => makeToken(TokenType.ValueFact, true),
                    TokenType.KeywordFalse => makeToken(TokenType.ValueFact, false),
                    TokenType.KeywordPipe => makeToken(TokenType.ValueIdentifier, "PIPE"),
                    TokenType.KeywordValue => makeToken(TokenType.ValueIdentifier, "VALUE"),
                    _ => makeToken(type, null),
                }
                :
                makeToken(TokenType.ValueIdentifier, str);
        }

        IToken? BuildOperator()
        {
            Position start = new(_scanner.Position);
            var singleOp = SingleOpDict.TryGetValue(_scanner.Buffer, out var type);
            var doubleOp = DoubleOpDict.TryGetValue(_scanner.Buffer, out var dict21);
            var tripleOp = TripleOpDict.TryGetValue(_scanner.Buffer, out var dict31);

            IToken? singleOpToken = singleOp ? new Token(type, start) : null;

            // Not an op
            if (!singleOp && !doubleOp && !tripleOp)
                return null;
            // Single
            if (!doubleOp && !tripleOp)
            {
                _scanner.Next();
                return new Token(type, start);
            }
            // Double
            if (!singleOp && !tripleOp)
            {
                _scanner.Next();
                var token = BuildDoubleOp(dict21!, start);
                _scanner.Next();
                return token;
            }
            // Triples
            if (!singleOp && !doubleOp)
            {
                _scanner.Next();
                return BuildTripleOp(dict31!, start, () => null, () => null);
            }
            // Double or single
            if (!tripleOp)
            {
                _scanner.Next();
                var token = BuildDoubleOp(dict21!, start, singleOpToken);
                _scanner.Next();
                return token;
            }
            // Triple or single
            if (!doubleOp)
            {
                _scanner.Next();
                return BuildTripleOp(dict31!, start, () => singleOpToken, () => null);
            }
            // Triple or double
            if (!singleOp)
            {
                _scanner.Next();
                return BuildTripleOp(dict31!, start, () => BuildDoubleOp(dict21!, start), () => null);
            }
            // else all three match
            _scanner.Next();
            return BuildTripleOp(dict31!, start, () => BuildDoubleOp(dict21!, start, singleOpToken), () => singleOpToken);
        }

        public IToken? BuildNumber()
        {
            Position pos = new(_scanner.Position);
            double value = 0;

            if (!char.IsDigit(_scanner.Buffer)) return null;
            if (_scanner.Buffer == '0')
            {
                _scanner.NextCharInNumber();
                if (!BuildNumberOtherBase(ref value))
                {
                    if (!BuildNumberDecimalPart(ref value))
                    {
                        if (char.IsDigit(_scanner.Buffer))
                        {
                            throw new LexerError(ErrorType.NumberNotADecimal);
                        }
                        else if (char.IsLetter(_scanner.Buffer))
                        {
                            throw new LexerError(ErrorType.NumberUnknownBase);
                        }
                    }
                }
            }
            else
            {
                BuildNumberWholePart(ref value);
                if (_scanner.Buffer == '.' && !BuildNumberDecimalPart(ref value))
                    throw new LexerError(ErrorType.NumberNotADecimal);
            }

            return new Token(TokenType.ValueNumber, pos, value);
        }

        IToken? BuildText() => BuildLimitedStringToken(c => c != '"', TokenType.ValueText, Shared.MAX_TEXT_SIZE);

        IToken? BuildComment() => BuildLimitedStringToken(c => c != '#', TokenType.ValueComment, Shared.MAX_COMMENT_SIZE);

        IToken BuildUnknown()
        {
            while(!Char.IsWhiteSpace(_scanner.Buffer) && !_scanner.IsSpecialWhiteSpace())
                _scanner.Next();
            return new Token(TokenType.SpecialNUL, new Position(_scanner.Position), BufferedError);
        }

        IToken BuildUnknownBuffered()
        {
            return new Token(TokenType.SpecialNUL, new Position(_scanner.Position), BufferedError);
        }

        /************************** OPERATOR SECTION *************************/

        IToken? BuildDoubleOp(Helpers.CharDictionary dict2, Position pos, IToken? onFail=null)
        {
            // Test double 2
            if (dict2!.TryGetValue(_scanner.Buffer, out var type))
                return new Token(type, pos);
            return onFail;
        }

        IToken? BuildTripleOp(Helpers.CharDictionary<Helpers.CharDictionary> dict31, Position pos,
            Func<IToken?> onFail, Func<IToken?> onSecondFail)
        {
            // Test triple 2
            bool tripleOp = dict31!.TryGetValue(_scanner.Buffer, out var dict32);
            if (!tripleOp)
            {
                IToken? token;
                if ((token = onFail()) == null)
                    _scanner.Next();
                return token;
            }

            var res = onFail();

            _scanner.Next();
            // Test triple 3
            tripleOp = dict32!.TryGetValue(_scanner.Buffer, out var type);
            if (!tripleOp)
                return res ?? onSecondFail();
            _scanner.Next();
            return new Token(type, pos);
        }

        /************************** NUMBERS SECTION **************************/

        bool BuildNumberWholePart(ref double value)
        {
            double whole = _scanner.Buffer - '0';
            _scanner.NextCharInNumber();
            while (char.IsDigit(_scanner.Buffer))
            {
                try
                {
                    whole = checked((whole * 10) + _scanner.Buffer - '0');
                }
                catch (OverflowException)
                {
                    throw new LexerError(ErrorType.NumberOverflow);
                }
                CheckOverflow(whole);
                _scanner.NextCharInNumber();
            }
            value = whole;
            return true;
        }

        bool BuildNumberOtherBase(ref double value) => _scanner.Buffer switch
        {
            'x' => BuildNumberBaseHelper(ref value, 16),
            'o' => BuildNumberBaseHelper(ref value, 8),
            'b' => BuildNumberBaseHelper(ref value, 2),
            _ => false,
        };

        bool BuildNumberDecimalPart(ref double value)
        {
            if (_scanner.Buffer != '.') return false;

            int part = 0;
            int decimals = 0;

            _scanner.NextCharInNumber();
            if (!char.IsDigit(_scanner.Buffer)) throw new LexerError(ErrorType.NumberNotADecimal);

            while (char.IsDigit(_scanner.Buffer))
            {
                try
                {
                    part = checked((part * 10) + _scanner.Buffer - '0');
                }
                catch (OverflowException)
                {
                    throw new LexerError(ErrorType.NumberOverflow);
                }
                CheckOverflow(part);
                decimals++;
                _scanner.NextCharInNumber();
            }
            value += part / Math.Pow(10, decimals);
            return true;
        }

        bool BuildNumberBaseHelper(ref double value, int numbase = 10)
        {
            int num = 0;
            _scanner.NextCharInNumber();
            Func<char, int> offset = c => '0'; ;
            Func<char, bool> pred = char.IsDigit;
            switch (numbase)
            {
                case 16:
                    pred = c => "0123456789abcdefABCDEF".Contains(c);
                    offset = c =>
                    {
                        if (('0' <= c) && (c <= '9')) return '0';
                        else if (('a' <= c) && (c <= 'f')) return 'a' - 10;
                        else return 'A' - 10;
                    };
                    break;
                case 8:
                    pred = c => ('0' <= c) && (c <= '7');
                    break;
                case 2:
                    pred = c => (c == '0') || (c == '1');
                    break;
                case 10:
                default:
                    break;
            }

            if (!pred(_scanner.Buffer)) throw new LexerError(ErrorType.NumberNotInBase);
            while (pred(_scanner.Buffer))
            {
                try
                {
                    num = checked((num * numbase) + _scanner.Buffer - offset(_scanner.Buffer));
                }
                catch (OverflowException)
                {
                    throw new LexerError(ErrorType.NumberOverflow);
                }
                CheckOverflow(num);
                _scanner.NextCharInNumber();
            }
            value = num;
            return true;
        }

        static void CheckOverflow(double val)
        {
            if (val > Shared.MAX_NUMBER_VALUE) throw new LexerError(ErrorType.NumberOverflow);
        }

        /************************** STRINGS SECTION **************************/

        string BuildLimitedString(Func<char, bool> rule, int sizeLimit)
        {
            var sb = new StringBuilder();
            uint size = 0;
            while (rule(_scanner.Buffer) && size <= sizeLimit)
            {
                size += 1;
                sb.Append(_scanner.Buffer);
                _scanner.Next();
            }
            if (size > sizeLimit) throw new LexerError(ErrorType.IdentifierTooBig, new() { sizeLimit });

            return sb.ToString();
        }

        IToken? BuildLimitedStringToken(Func<char, bool> rule, TokenType type, int sizeLimit)
        {
            Position pos = new(_scanner.Position);
            if (rule(_scanner.Buffer)) return null;

            var sb = new StringBuilder();
            _scanner.NextCharInString();
            uint size = 0;
            while (rule(_scanner.Buffer) && size <= sizeLimit)
            {
                if (_scanner.Buffer == '\uffff')
                    throw new LexerError(type == TokenType.ValueText ? ErrorType.TextSuddenETX : ErrorType.CommentSuddenETX);

                var c = _scanner.Buffer;

                if (_scanner.Buffer == '\\') 
                {
                    _scanner.NextCharInString();
                    c = _scanner.HandleEscape();
                }
                size += 1;
                sb.Append(c);
                _scanner.NextCharInString();
            }

            if (size > sizeLimit) throw new LexerError(
                type == TokenType.ValueText ? ErrorType.TextTooBig : ErrorType.CommentTooBig,
                new() { sizeLimit });
            _scanner.NextCharInString();

            return new Token(type, pos, sb.ToString());
        }
    }
}
