## Instrukcja warunkowa IF DO ELSE

Jeśli wyrażenie warunku ewaluuje do prawdy (TRUE), wykonywana jest instrukcja (lub blok) po słowie kluczowym DO.
Jeśli wyrażenie warunku ewaluuje do fałszu (FALSE), wykonywana jest instrukcja (lub blok) po słowie kluczowym ELSE.
Jeśli wyrażenie warunku ewaluuje do pustości (NONE), zgłaszany jest błąd.

```
IF conditon DO onTrue; ELSE onFalse;

# Można łączyć instrukcje warunkowe #
IF ... DO
    IF ... DO
        ...
    ELSE IF ... DO
        ....
    ELSE IF ... DO
        ...
ELSE DO
    ...
```

## Instrukcje skoku

STOP - przerwij pętlę natychmiast (dopuszczalne tylko w pętli WHILE lub FOR)
SKIP - pomiń resztę bloku, ponownie sprawdź warunek (dopuszczalne tylko w pętli WHILE lub FOR)
RETURN - przerwij pętlę natychmiast, wyjdź z kontekstu
RETURN x - przerwij pętlę natychmiast, wyjdź z kontekstu, zwróć wartość (dopuszczalne tylko wewnątrz ciała funkcji)

## Instrukcja pętli WHILE, instrukcje skoku

Instrukcja WHILE będzie wykonywała instrukcję (lub blok) tak długo, jak długo warunek jest spełniony, lub do momentu przerwania pętli przez instrukcje skoku. Jeżeli warunke ewaluuje do NONE, zgłaszany jest błąd.

Pętla wypisująca liczby od 5 do 1:
```
MUTABLE NUMBER i IS 5;

WHILE i > 0 DO
BEGIN
    CALL Print WITH i NOW;
    i IS i - 1;
END
```

Pętla wypisująca liczby od 1 do 3:
```
MUTABLE NUMBER i IS 0;

WHILE i >= 0 DO
BEGIN
    IF i == 3 DO STOP;  # Jeśli prawda, wychodzimy natychmiast z pętli #
    i IS i + 1;
    CALL Print WITH i NOW;
END;
```

Pętla wypisująca 8 6 4 0
```
MUTABLE NUMBER i IS 10;

WHILE i > 0 DO
BEGIN
    i IS i - 1;
    IF i <= 3 DO SKIP;  # Jeśli prawda, pomijamy resztę bloku #
    i IS i - 1;
    CALL Print WITH i NOW;
END;
CALL Print WITH i NOW;
```

## Instrukcja pętli FOR

FOR jest to WHILE poszerzone o możliwość deklaracji zmiennej istniejącej w ramach tej pętli.

Wypisze liczby od 4 do 0.
```
FOR MUTABLE NUMBER i IS 5 WHILE i > 0 DO
BEGIN
    i IS i - 1;
    CALL Print WITH i NOW;
END;
```

## Anonimowy wzorzec MATCH

Wzorzec MATCH przyjmuje wyrażenie i na podstawie tej wartości ewaluuje warunki kolejnych gałęzi (zawierających instrukcje, w tym kolejne wzorce) do otrzymania pierwszej prawdy (dopasowania).
Po dopasowaniu, wzorzec wykonuje przypisaną instrukcję (blok). Jeśli warunek żadnej z gałęzi nie został spełniony, wykouje się gałąź DEFAULT, która zawsze musi być zdefiniowana (ale może być pusta). Wewnątrz wzorca MATCH można uzyskać dostęp do przekazanej wartości poprzez zmienną VALUE o stałej wartości.

Instrukcje gałęzi nie mogą zawierać instrukcji skoku (SKIP, STOP, RETURN).

```
MATCH WITH 1
BEGIN
    FALSE DO CALL Print WITH "false" NOW;,         # gałąź nigdy się nie wykona #
    (VALUE + 1) >= 2 DO CALL Print WITH VALUE NOW;,
    DEFAULT;,
END
```

Zwróci 2:
```
MATCH WITH 1
BEGIN
    TRUE DO MATCH WITH VALUE * 2
        BEGIN
            DEFAULT CALL Print WITH VALUE NOW;,
        END,
    DEFAULT;,
END
```
