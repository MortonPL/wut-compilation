using Microsoft.VisualStudio.TestTools.UnitTesting;

using static VerboseTests.Interpreter_.InterpreterTests;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public class AcceptationTests
    {
        [TestMethod]
        public void AcceptationTestReferences()
        {
            string program = @"
            FUNCTION add WITH MUTABLE TEXT t RETURNS NOTHING
            BEGIN
                t IS t ++ "" ano lot"";
                CALL Print WITH {CALL First WITH t NOW} NOW;
                CALL Print WITH {CALL Last WITH t NOW} NOW;
                CALL Print WITH {CALL Body WITH t NOW} NOW;
                CALL Print WITH {CALL Tail WITH t NOW} NOW;
                MUTABLE TEXT a IS NONE;
                MUTABLE TEXT b IS NONE;
                CALL Split WITH t, a, b NOW;
                CALL Print WITH a NOW;
                CALL Print WITH b NOW;
                CALL BackSplit WITH t, a, b NOW;
                CALL Print WITH a NOW;
                CALL Print WITH b NOW;
            END

            MUTABLE TEXT t IS ""ban"";
            CALL add WITH t NOW;
            CALL Print WITH t NOW;
            ";

            string expected =
            "ban" + 
            "lot" + 
            "ban ano" + 
            "ano lot" + 
            "ban" + 
            "ano lot" + 
            "ban ano" + 
            "lot" + 
            "ban ano lot"
            ;

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestBuiltinFizzBuzzPattern()
        {
            string program = @"
            CALL FizzBuzz WITH 3 NOW;
            CALL FizzBuzz WITH 5 NOW;
            CALL FizzBuzz WITH 15 NOW;
            CALL FizzBuzz WITH 22 NOW;
            ";

            string expected = "FizzBuzzFizzBuzz22";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestSimpleWhile()
        {
            string program = @"
            MUTABLE NUMBER i IS 5;

            WHILE i > 0 DO
            BEGIN
                CALL Print WITH i NOW;
                i IS i - 1;
            END
            ";

            string expected = "54321";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestWhileStop()
        {
            string program = @"
            MUTABLE NUMBER i IS 0;

            WHILE i >= 0 DO
            BEGIN
                IF i == 3 DO STOP;  # Jeśli prawda, wychodzimy natychmiast z pętli #
                i IS i + 1;
                CALL Print WITH i NOW;
            END;
            ";

            string expected = @"123";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestWhileSkip()
        {
            string program = @"
            MUTABLE NUMBER i IS 10;

            WHILE i > 0 DO
            BEGIN
                i IS i - 1;
                IF i <= 3 DO SKIP;  # Jeśli prawda, pomijamy resztę bloku #
                i IS i - 1;
                CALL Print WITH i NOW;
            END;
            CALL Print WITH i NOW;
            ";

            string expected = @"8640";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestFor()
        {
            string program = @"
            FOR MUTABLE NUMBER i IS 5 WHILE i > 0 DO
            BEGIN
                i IS i - 1;
                CALL Print WITH i NOW;
            END;
            ";

            string expected = @"43210";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestAnonMatch()
        {
            string program = @"
            MATCH WITH 1
            BEGIN
                FALSE DO CALL Print WITH ""false"" NOW;,         # gałąź nigdy się nie wykona #
                (VALUE + 1) >= 2 DO CALL Print WITH VALUE NOW;,
                DEFAULT;,
            END
            ";

            string expected = @"1";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestAnonMatchNested()
        {
            string program = @"
            MATCH WITH 1
            BEGIN
                TRUE DO MATCH WITH VALUE * 2
                    BEGIN
                        DEFAULT CALL Print WITH VALUE NOW;,
                    END,
                DEFAULT;,
            END
            ";

            string expected = @"2";

            TestInterpreterAcceptation(program, expected);
        }

        [TestMethod]
        public void AcceptationTestPattern()
        {
            string program = @"
            MUTABLE NUMBER a IS 0;

            PATTERN testGąska WITH TEXT gąska
            BEGIN
                VALUE ?? DO a IS NONE;,
                VALUE == ""balbinka"" OR VALUE == ""barbara"" DO a IS 1;,
                VALUE == ""kasia"" DO a IS 2;,
                VALUE == CALL getSavedGoose NOW DO a IS 3;,
                DEFAULT a IS 0;,
            END

            CALL testGąska WITH ""balbinka"" NOW;  # ustawi zmienną a na 1 #
            CALL Print WITH a NOW;
            ";

            string expected = @"1";

            TestInterpreterAcceptation(program, expected);
        }
    }
}



