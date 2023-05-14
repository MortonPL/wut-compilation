using System;
using System.Linq;
using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;

namespace VerboseCore.Parser
{
    public partial class Parser
    {
        /// <summary>
        /// If `thing` is not null, returns it, otherwise raises an exception.
        /// </summary>
        /// <returns>`symbol`</returns>
        public T ExpectNotNull<T>(T? thing, SymbolType forLogger=SymbolType.Any)
        {
            if (thing is null)
                throw new ParserError(ErrorType.ExpectedSymbol, new() { forLogger }); // TODO Exceptions
            return thing;
        }

        /// <summary>
        /// If scanner content type matches the `type`, consumes the token.
        /// Raises an exception on failure.
        /// </summary>
        public void ExpectToken(TokenType type)
        {
            if (type != _scanner.Type)
                throw new ParserError(ErrorType.ExpectedToken, new() { MapSymbolType(type) }); // TODO Exception
            _scanner.Next();
        }

        /// <summary>
        /// If scanner content type matches any of the `types`, consumes the token.
        /// Raises an exception on failure.
        /// </summary>
        public void ExpectToken(List<TokenType> types)
        {
            if (!types.Contains(_scanner.Type))
            {
                var errorData = types.ConvertAll(t => (object)MapSymbolType(t));
                throw new ParserError(ErrorType.ExpectedToken, errorData); // TODO Exception
            }
            _scanner.Next();
        }

        /// <summary>
        /// Similar to `ExpectToken`, but doesn't raise an exception on failure and returns the result.
        /// </summary>
        /// <returns>True when the types match, false otherwise.</returns>
        public bool TryExpectToken(TokenType type)
        {
            var equal = type == _scanner.Type;
            if (equal)
                _scanner.Next();
            return equal;
        }

        /// <summary>
        /// Similar to `ExpectToken`, but doesn't raise an exception on failure and returns the result.
        /// </summary>
        /// <returns>True when any of the types match, false otherwise.</returns>
        public bool TryExpectToken(List<TokenType> types)
        {
            var equal = types.Contains(_scanner.Type);
            if (equal)
                _scanner.Next();
            return equal;
        }

        /// <summary>
        /// Behaves like `ZeroOrOnce`, but raises exception on failure.
        /// </summary>
        public void Once(TokenType type, Action post)
        {
            if (type != _scanner.Type)
                throw new ParserError(ErrorType.ExpectedToken, new() { MapSymbolType(type) }); // TODO Exception
            post();
        }

        /// <summary>
        /// Behaves like `ZeroOrOnce`, but raises exception on failure.
        /// </summary>
        public void Once(TokenType type, Action pre, Action post)
        {
            if (type != _scanner.Type)
                throw new ParserError(ErrorType.ExpectedToken, new() { MapSymbolType(type) }); // TODO Exception
            pre();
            _scanner.Next();
            post();
        }

        /// <summary>
        /// Behaves like `ZeroOrOnce`, but raises exception on failure.
        /// </summary>
        public void Once(List<TokenType> types, Action post)
        {
            if (!types.Contains(_scanner.Type))
            {
                var errorData = types.ConvertAll(t => (object)MapSymbolType(t));
                throw new ParserError(ErrorType.ExpectedToken, errorData); // TODO Exception
            }
            _scanner.Next();
            post();
        }

        /// <summary>
        /// Behaves like `ZeroOrOnce`, but raises exception on failure.
        /// </summary>
        public void Once(List<TokenType> types, Action pre, Action post)
        {
            if (!types.Contains(_scanner.Type))
            {
                var errorData = types.ConvertAll(t => (object)MapSymbolType(t));
                throw new ParserError(ErrorType.ExpectedToken, errorData); // TODO Exception
            }
            pre();
            _scanner.Next();
            post();
        }

        /// <summary>
        /// If scanner content type matches the `type`, consumes token, executes `post`.
        /// Nothing otherwise.
        /// </summary>
        public void ZeroOrOnce(TokenType type, Action post)
        {
            if (type == _scanner.Type)
            {
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// If scanner content type matches the `type`, executes `pre`, consumes token, executes `post`.
        /// Nothing otherwise.
        /// </summary>
        public void ZeroOrOnce(TokenType type, Action pre, Action post)
        {
            if (type == _scanner.Type)
            {
                pre();
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// If scanner content type matches any of the `types`, consumes token, executes `post`.
        /// Nothing otherwise.
        /// </summary>
        public void ZeroOrOnce(List<TokenType> types, Action post)
        {
            if (types.Contains(_scanner.Type))
            {
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// If scanner content type matches any of the `types`, executes `pre`, consumes token, executes `post`.
        /// Nothing otherwise.
        /// </summary>
        public void ZeroOrOnce(List<TokenType> types, Action pre, Action post)
        {
            if (types.Contains(_scanner.Type))
            {
                pre();
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// While scanner content type matches the `type`, consumes token, executes `post`.
        /// Does nothing on failure/exit.
        /// </summary>
        public void ZeroOrMany(TokenType type, Action post)
        {
            while (type == _scanner.Type)
            {
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// While scanner content type matches the `type`, executes `pre`, consumes token, executes `post`.
        /// Does nothing on failure/exit.
        /// </summary>
        public void ZeroOrMany(TokenType type, Action pre, Action post)
        {
            while (type == _scanner.Type)
            {
                pre();
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// While scanner content type matches any of the `types`, consumes token, executes `post`.
        /// /// Does nothing on failure/exit.
        /// </summary>
        public void ZeroOrMany(List<TokenType> types, Action post)
        {
            while (types.Contains(_scanner.Type))
            {
                _scanner.Next();
                post();
            }
        }

        /// <summary>
        /// While scanner content type matches any of the `types`, executes `pre`, consumes token, executes `post`.
        /// /// Does nothing on failure/exit.
        /// </summary>
        public void ZeroOrMany(List<TokenType> types, Action pre, Action post)
        {
            while (types.Contains(_scanner.Type))
            {
                pre();
                _scanner.Next();
                post();
            }
        }
    }
}
