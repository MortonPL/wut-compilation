# Leksyka

Klamry `{` i `}` są zawsze ignorowane.

```ebnf
token = IDENTIFIER
      | KEYWORD
      | LITERAL
      | PUNCTUATOR
      | COMMENT;
```

## Identyfikatory

Identyfikator składa się z jednej litery lub znaku podkreślenia i dowolnej liczby liter, cyfr i znaków podkreślenia.

`IDENTIFIER = "[\p{L}\p{M}_][\p{L}\p{M}\p{N}_]+"`; (wyrażenie regularne)

## Słowa kluczowe

```ebnf
KEYWORD = AND
        | BEGIN
        | CALL
        | DO
        | ELSE
        | END
        | FACT
        | FALSE
        | FOR
        | FUNCTION
        | IS
        | IF
        | MATCH
        | MUTABLE
        | NONE
        | NOW
        | NOT
        | NOTHING
        | NUMBER
        | OR
        | OTHERWISE
        | OVERRIDE
        | PATTERN
        | PIPE
        | RETURN
        | RETURNS
        | SKIP
        | STOP
        | TEXT
        | THEN
        | TRUE
        | VALUE
        | WHILE
        | WITH;

AND       = "AND";
BEGIN     = "BEGIN";
CALL      = "CALL";
DO        = "DO";
ELSE      = "ELSE";
END       = "END";
FACT      = "FACT";
FALSE     = "FALSE";
FOR       = "FOR";
FUNCTION  = "FUNCTION";
IS        = "IS";
IF        = "IF";
MATCH     = "MATCH";
MUTABLE   = "MUTABLE";
NONE      = "NONE";
NOT       = "NOT";
NOTHING   = "NOTHING";
NOW       = "NOW";
NUMBER    = "NUMBER";
OR        = "OR";
OTHERWISE = "OTHERWISE";
OVERRIDE  = "OVERRIDE";
PATTERN   = "PATTERN";
PIPE      = "PIPE";
RETURN    = "RETURN";
RETURNS   = "RETURNS";
SKIP      = "SKIP";
STOP      = "STOP";
TEXT      = "TEXT";
THEN      = "THEN";
TRUE      = "TRUE";
VALUE     = "VALUE";
WHILE     = "WHILE";
WITH      = "WITH";
```

## Literały

```ebnf
LITERAL = number_literal
        | text_literal
        | fact_literal
        | none_literal;
```

```ebnf
number_literal = binary_number
               | octal_number
               | decimal_number
               | hex_number;
```

`binary_number = "0b[0-1]+"` (wyrażenie regularne)
	
`octal_number = = "0o[0-7]+"` (wyrażenie regularne)
  
`decimal_number = "(([1-9][0-9]*)|0)(.[0-9]+)?"` (wyrażenie regularne)

`hex_number = "0x[0-9a-fA-F]+"` (wyrażenie regularne)

Stałe tekstowe muszą zaczynać i kończyć się '"'. Ucieczki znaków standardowe: '\"' i '\\'.

`text_literal =  "((\\")|(\\\\)|[^"])*";` (wyrażenie regularne)

`fact_literal = "(TRUE)|(FALSE)";` (wyrażenie regularne)

`none_literal = "NONE";` (wyrażenie regularne)

## Operatory i punktuatory

```ebnf
PUNCTUATOR = DOT
           | COMMA
           | PLUS
           | MINUS
           | MULTIPLY
           | DIVIDE
           | MODULO
           | CONCAT
           | EQUAL
           | NOT_EQUAL
           | TEXT_EQUAL
           | TEXT_NOT_EQUAL
           | GREATER
           | GREATER_EQUAL
           | LESSER
           | LESSER_EQUAL
           | PARENTHESIS_OPEN
           | PARENTHESIS_CLOSE
           | TERNARY_YES
           | TERNARY_NO
           | NONE_TEST;

DOT               = ";";
COMMA             = ",";
PLUS              = "+";
MINUS             = "-";
MULTIPLY          = "*";
DIVIDE            = "/";
MODULO            = "%";
CONCAT            = "++";
EQUAL             = "==";
NOT_EQUAL         = "!=";
TEXT_EQUAL        = "===";
TEXT_NOT_EQUAL    = "!==";
GREATER           = ">";
GREATER_EQUAL     = ">=";
LESSER            = "<";
LESSER_EQUAL      = "<=";
PARENTHESIS_OPEN  = "(";
PARENTHESIS_CLOSE = ")";
TERNARY_YES       = "?";
TERNARY_NO        = ":";
NONE_TEST         = "??";
```

## Komentarze

Komentarze muszą zaczynać i kończyć się znakiem '#'.

`COMMENT = "#[^#]*#";` (wyrażenie regularne)

# Składnia

```ebnf
program = {declaration | statement};
```

## Instrukcje

```ebnf
statement = coumpound_statement
          | expression_statement
          | if_statement
          | iteration_statement
          | jump_statement
          | anon_match
          | DOT;

compound_statement = BEGIN, {declaration | statement}, END;

expression_statement = expression, DOT;

if_statement = IF, expression, DO, statement, [ELSE, statement];

iteration_statement = for_statement
                    | while_statement;

jump_statement = SKIP
               | STOP
               | RETURN
               | (RETURN, expression),
               DOT;

for_statement = FOR, variable_declaration, while_statement;

while_statement = WHILE, expression, DO, statement;

anon_match = MATCH, WITH, expression, match_block;
```

## Deklaracje

```ebnf
declaration = (variable_declaration, DOT)
           | function_declaration
           | pattern_declaration;

variable_declaration = declarator, [IS, expression];

declarator = [MUTABLE], type, IDENTIFIER;

type = NUMBER | TEXT | FACT;

return_type = type | NOTHING;

function_declaration = FUNCTION, [OVERRIDE], IDENTIFIER, [WITH, parameters], RETURNS, return_type, statement;

parameters = declarator, {COMMA, declarator}

pattern_declaration = PATTERN, [OVERRIDE], IDENTIFIER, WITH, declarator, (match_block | DOT);

match_block = BEGIN, {match_branch}, DEFAULT, statement, COMMA, END;

match_branch = expression, DO, statement, COMMA;
```

## Wyrażenia

```ebnf
expression = operator_12;

operator_12 = {operator_11, IS}, operator_11;

operator_11 = operator_10, {[THEN, operator_10], [OTHERWISE, operator_10]};

operator_10 = operator_9, {TERNARY_YES, operator_9, [TERNARY_NO, operator_9]};

operator_9 = operator_8, {OR, operator_8};

operator_8 = operator_7, {AND, operator_7};

operator_7 = operator_6, {(EQUAL | NOT_EQUAL | TEXT_EQUAL | TEXT_NOT_EQUAL), operator_6};

operator_6 = operator_5, {(GREATER | GREATER_EQUAL | LESSER | LESSER_EQUAL), operator_5};

operator_5 = operator_4, {(PLUS | MINUS | CONCAT), operator_4};

operator_4 = operator_3, {(MULTIPLY | DIVIDE | MODULO), operator_3};

operator_3 = [MINUS | NOT], operator_2;

operator_2 = (LITERAL | IDENTIFIER | PIPE | VALUE | operator_1 | (PARENTHESIS_OPEN, expression, PARENTHESIS_CLOSE)), [TEST_NONE];

operator_1 = CALL, expression, [WITH, arguments], NOW;

arguments  = expression, {COMMA, expression};
```
