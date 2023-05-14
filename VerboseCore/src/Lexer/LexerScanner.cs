using System;
using System.IO;
using System.Text;

using VerboseCore.Abstract;
using VerboseCore.Exceptions;
using VerboseCore.Entities;
using VerboseCore.Interfaces;

namespace VerboseCore.Lexer
{
    public class LexerScanner: ILexerScanner
    {
        private readonly StreamReader _reader;
        private APosition _position;
        private char _buffer;
        private readonly char?[] _newline = new char?[2] { null, null };
        private readonly StringBuilder _sb;

        public APosition Position { get => _position; }
        public char Buffer { get => _buffer; }
        public string BufferedError
        { get
            {
                var str = _sb.ToString();
                if (_sb[^1] == '\uffff')
                    str = str.Remove(str.Length - 1);
                str = str.Trim();
                return str;
            }
        }

        public LexerScanner(Stream stream)
        {
            _reader = new StreamReader(stream);
            _position = new Position();
            _sb = new();
            Next();
        }

        ~LexerScanner() { _reader.Dispose(); }

        public void ClearBufferedError()
        {
            var c = _sb[^1];
            _sb.Clear();
            _sb.Append(c);
        }

        public void Next()
        {
            NextChar();
            if (_buffer == '{' || _buffer == '}') Next();
        }

        public void NextCharInString()
        {
            NextChar();
        }

        public void NextCharInNumber()
        {
            NextChar();
            if (_buffer == '_' || _buffer == '{' || _buffer == '}') NextCharInNumber();
        }

        // Check if the buffered character is a new line (consuming)
        public bool TryNewline()
        {
            bool result = true;
            if (_newline[0] == null)
            {
                result = LearnNewline();
            }
            else if (_buffer == _newline[0])
            {
                Next();
                if (_newline[1] != null && _buffer == _newline[1])
                    Next();
                else if (_newline[1] != null)
                    return false;
            }
            else
            {
                result = false;
            }

            if (result)
            {
                _position.AddRow();
                _sb.Clear();
                _sb.Append(_buffer);
                return true;
            }

            return result;
        }

        public void SkipWhites()
        {
            while (Char.IsWhiteSpace(_buffer) && !IsSpecialWhiteSpace())
            {
                Next();
            }
            //_sb.Append(_buffer);
        }

        public char HandleEscape()
        {
            return _buffer switch
            {
                'a' => '\a',
                'b' => '\b',
                'f' => '\f',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'v' => '\v',
                '\\' => '\\',
                '\'' => '\'',
                '\"' => '\"',
                '#' => '#',
                _ => throw new LexerError(ErrorType.UnknownEscape, new(){ _buffer }),
            };
        }

        public bool IsSpecialWhiteSpace()
        {
            return _buffer == '\uffff' || PeekNewline();
        }

        private void NextChar()
        {
            _buffer = (char)_reader.Read();
            _sb.Append(_buffer.ToString().Replace("\\", "\\\\"));
            _position.AddColumn();
        }

        // Check if the buffered character is a new line (not consuming)
        private bool PeekNewline()
        {
            bool result = true;
            if (_newline[0] == null)
            {
                result = LearnNewline();
            }
            else if (_buffer == _newline[0])
            {
                var char_ = (char)_reader.Peek();
                if (_newline[1] != null && char_ == _newline[1])
                    _reader.Peek();
                else if (_newline[1] != null)
                    return false;
            }
            else
            {
                result = false;
            }

            if (result)
            {
                return true;
            }

            return result;
        }

        // Try to find and save a new line sequence
        private bool LearnNewline()
        {
            switch (_buffer)
            {
                case '\r':
                    {
                        _newline[0] = _buffer;
                        var char_ = (char)_reader.Peek();
                        if (char_ == '\n')
                        {
                            _newline[1] = char_;
                        }
                        return true;
                    }
                case '\n':
                    {
                        _newline[0] = _buffer;
                        var char_ = (char)_reader.Peek();
                        if (char_ == '\r')
                        {
                            _newline[1] = char_;
                        }
                        return true;
                    }
                case '\u001e':
                    {
                        _newline[0] = _buffer;
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
