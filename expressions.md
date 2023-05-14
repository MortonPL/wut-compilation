## Typy danych

* FACT - wartosć TRUE / FALSE
* NUMBER - wartość liczbowa całkowita lub zmiennoprzecinkowa
* TEXT - ciąg znaków dowolnej długości

Specjalny "para-typ" danych:
* NOTHING - brak typu. Używany w deklaracjach funkcji, gdy nie chcemy zwrócić z nich wartości.

NONE - **wartość pusta**. NONE nie jest typem, ale specjalną wartością, którą może posiadać dowolny typ danych. NONE jest propagowane, tzn. każda operacja, włącznie z rzutowaniem typu, z NONE jako argumentem da wynik NONE (wyjątki: operator sprawdzania pustości, wywołanie funkcji).

Rzutowanie typów:
* FACT => NUMBER: TRUE => 1, FALSE => 0
* NUMBER => FACT: nie  0 => TRUE, 0 => FALSE
* FACT => TEXT:   TRUE => "TRUE", FALSE => "FALSE"
* TEXT => FACT:   nie "" => TRUE, "" => FALSE
* NUMBER => TEXT: dowolna liczba => dowolna liczba jako tekst
* TEXT => NUMBER: dowolna liczba jako tekst => liczba, ciąg nie będący liczbą => NONE

## Ciągi

```
# komentarz #

# komentarz
wielo
linijkowy #

"ciąg znaków"

"ciąg
znaków wielo
linijkowy"

"ciąg z \" albo \\ w środku"
```

## Wyrażenia logiczne (x OR y, x AND y, NOT x)

Wyrażenia arytmetyczne wymuszają konwersję na typ FACT. Nie jest stosowane tzw. "short-circuiting".

Typ zwrotny to FACT.

```
x OR y            # Alternatywa #
x AND y           # Koniunkcja #
NOT x             # Negacja #

TRUE OR FALSE OR TRUE       # Zwróci TRUE  #
TRUE AND FALSE AND TRUE     # Zwróci FALSE #
TRUE AND FALSE OR TRUE      # Zwróci TRUE, bo kolejność wykonywania działań! #
TRUE AND (FALSE OR TRUE)    # Zwróci FALSE, bo nawias podwyższa priorytet! #

NONE OR TRUE      # Jakakolwiek operacja logiczna z wartością pustą (NONE) #
                  # zawsze zwróci NONE #
```

## Wyrażenia arytmetyczne (-x, x + y, x -y , x * y, x / y, x % y)

Wyrażenia arytmetyczne wymuszają konwersję argumentów na typ NUMBER.

Typ zwrotny to NUMBER.

Próba dzielenia przez zero generuje błąd.

```
-12                    # Zwróci -12 #
10 + 5                 # Zwróci 15 #
10 + 5 - 1.1           # Zwróci 13,9 #
-7 + 7                 # Zwróci 0 #

2 + 3 * 2              # Zwróci 8 #
(2 + 3) * 2            # Zwróci 10 #
8 / 4                  # Zwróci 2 #
8 / 5                  # Zwróci 1,6 #

15 % 4                # Zwróci 3 #
-15 % 4               # Zwróci 1 #
15 % -4               # Zwróci -1 #

10 + NONE             # Jakakolwiek operacja arytmetyczna z wartością pustą (NONE) #
                      # zawsze zwróci NONE #
```

## Operacje na ciągach (x ++ y, x === y, x !== y)
Operacje na ciągach wymuszają konwersję na typ TEXT.

Typ zwrotny operatora konkatenacji to TEXT.
Typ zwrotny operatorów porównania zawartości ciągów to FACT.

```
"PIES" ++ "KOT"       # Zwróci "PIESKOT" #
"ALA" ++ NONE         # Zwróci NONE! #
"ABBA" === "BAAB"     # Zwróci TRUE #
"144" !== 144         # Zwróci FALSE #

```

## Operator testu pustości (x ??)

Operator testu pustości zwraca TRUE, jeżeli argument jest pusty (ma wartość NONE), w przeciwnym wypadku FALSE.

```
NONE ??                # Zwróci TRUE #
"ALA" ??               # Zwróci FALSE #
```

## Wyrażenia porównanwcze (x == y, x != y, x > y, x >= y, x < y, x <= y)

Wyrażenia porównawcze mają różne zachowanie, zależne od typu pierwszego argumentu.

Dla typu NUMBER, oba argumenty są rzutowane na ten typ i porównywane są ich wartości liczbowe.

Dla typu TEXT, oba argumenty są rzutowane na ten typ i porównywane są ich długości.

Dla typu FACT, oba argumenty są rzutowane na ten typ i porównywane są ich reprezentacje liczbowe (TRUE = 1. FALSE = 0).

```
10 == 10             # Zwróci TRUE #
10 != 5              # Zwróci TRUE #
"abcd" >= "abc"      # Zwróci TRUE #
TRUE <= FALSE        # Zwróci FALSE #

NONE == 10           # Jakakolwiek operacja porównawcza z wartością pustą (NONE) #
                     # zawsze zwróci NONE #
```

## Przypisanie wartości (x IS y)

Operacja przypisania wartości przyjmuje taki typ danych, jaki ma pierwszy (lewy) argument.

Operacja przypisania powiedzie się tylko, gdy pierwszy argument jest mutowalną zmienną. W przeciwnym wypadku (jeśli nie jest zmienną, lub nie jest mutowalną zmienną) ogłaszany jest odpowiedni błąd.

Operacja przypisania zwraca jako wynik wartość drugiego (prawego) argumentu.

Przypisanie jakiejkolwiek wartości do zmiennych zarezerwowanych PIPE lub VALUE jest nielegalną operacją i wywoła błąd.

```
a IS 10
a IS b IS 5
```

## Operator ternarny (x ? y : z)

```
# Operator ternarny: jeśli warunek jest spełniony, to zwróć wyrażenie po lewej, w przeciwnym wypadku po prawej #
kwota > 500 ? kwota - 500: 0;

# Dopuszczalne jest pominięcie trzeciej wartości, w przypadku ewaluacji zwracane jest NONE #

NUMBER zysk IS kwota > 500 ? kwota - 500;  # jeśli kwota mniejsza od 500 to zysk jest NONE #

# Operator ternarny jest wyrażeniem i może być stosowany do innych rzeczy niż przypisanie #
isPies ? CALL szczekaj NOW;

# Operator ternarny w tandemie z negacją operatora testu pustości może zostać użyty jako gwarancja, że zwrócona zostanie wartość nie pusta. #
NOT a?? ? CALL sqrt WITH a NOW;
```

## Operator rurociągu (x THEN y OTHERWISE z)

```
# Operator rurociągu: ewaluuję sekwencję wyrażeń, przekazując wynik do zarezerwowanej zmiennej PIPE.
Jeśli jakikolwiek segment "rury" zwróci NONE, reszta segmentów jest pomijana i zwracane jest NONE.
Jeśli występuje klauzula OTHERWISE, zostanie ona zwrócona w momencie zwrócenia wartości NONE
przez jakikolwiek segment "rury".
Jeśli występuje OTHERWISE, ale nie PIPE, w przypadku, gdy pierwszy argument zwróci nie NONE, zwrócone zostanie i tak NONE.
Dopuszczalna jest dowolna kombinacja PIPE i OTHERWISE. #

# załóżmy, że istnieje funkcja revstr(TEXT, NUMBER), która odwraca pierwsze N znaków ciągu #
# załóżmy, że istnieje funkcja upper(TEXT), która zmienia wszystkie litery ciągu na wielkie #
"Reksio" THEN CALL revstr WITH PIPE, 3 NOW THEN CALL upper WITH PIPE THEN "PIES " ++ PIPE;
# Wynik: PIES KERSIO #

# Klauzula OTHERWISE: wynik 0 jeśli nie znajdziemy produktu, lub nie ma ceny #
CALL getProductID WITH "banan" NOW THEN CALL getProductPrice WITH PIPE NOW THEN PIPE - discount OTHERWISE 0;

# Alternatywny zapis przykładu powyżej - klamry (ignorowane przez interpreter) mogą zwiększyć czytelność długich wyrażeń: #
{CALL getProductID WITH "banan" NOW} THEN {CALL getProductPrice WITH PIPE NOW} THEN {PIPE - discount} OTHERWISE 0;
```

## Wywołanie funkcji lub wzorca (CALL x WITH y NOW)

```
# Przykładowe wywołanie #
CALL funName WITH param1, param2 NOW;
```

Jeżeli wywołujemy w środku ewaluowanego wyrażenia funkcję bez typu zwrotnego (RETURNS NOTHING) lub nazwany wzorzec, otrzymamy błąd.

Argumenty domyślnie są przekazywane przez wartość - wyjątkiem jest sytuacja, gdy funkcja (lub wzorzec) ma zdefiniowany parametr mutowalny i przekazujemy do niej zmienną, która też jest mutowalna. W takim przypadku będziemy przekazywać wartość przez referencję i każda modyfikacja zmiennej wewnątrz funkcji spowoduje także modyfikację zmiennej, która została do tej funkcji przekazana.
