## Funkcje wbudowane

* `FUNCTION Print WITH TEXT t RETURNS NOTHING` - wypisuje argument na wyjście
* `FUNCTION Quit RETURNS NOTHING` - kończy dzianie programu
* `FUNCTION First WITH TEXT t RETURNS TEXT` - zwraca pierwsze słowo (ciągi oddzielone spacją)
* `FUNCTION Last WITH TEXT t RETURNS TEXT` - zwraca ostatnie słowo 
* `FUNCTION Body WITH TEXT t RETURNS TEXT` - zwraca wszystkie słowa oprócz ostatniego
* `FUNCTION Tail WITH TEXT t RETURNS TEXT` - zwraca wszystkie słowa oprócz pierwszego
* `FUNCTION Split WITH TEXT source, MUTABLE TEXT head, MUTABLE TEXT tail RETURNS NOTHING` - rozdziela ciąg na pierwsze słowo i resztę, przypisuje do podanych zmiennych head i tail
* `FUNCTION SplitBack WITH TEXT source, MUTABLE TEXT body, MUTABLE TEXT tip RETURNS NOTHING`  - rozdziela ciąg na wszystkie słowa oprócz ostatniego i ostatnie, przypisuje do podanych zmiennych body i tip
* `PATTERN FizzBuzz WITH NUMBER n` - wykonuje FizzBuzz'a dla pojedyńczej liczby

## Składnia deklaracji funkcji / wzorca 

`FUNCTION funName WITH TEXT param1 RETURNS FACT;`

`PATTERN patternName WITH NUMBER n;`

Jeżeli funkcja / wzorzec nie zawiera ciała (pojedynczas instrukcja lub blok w przypadku funckji, blok wzorca w przypadku nazwanego wzorca), to jest to deklaracja funkcji / wzorca. Jeżeli zawiera ciało, to jest to definicja.

## Deklaracja funkcji / nazwanego wzroca

* Funkcję / wzorzec możemy zadeklarować lub zdefiniować w dowolnym bloku kodu.

* Deklaracja funkcji / wzorca jest unikatowa, gdy ma unikatową nazwę lub gdy ma nazwę współdzieloną z już zadeklarowaną funkcją / wzorcem, ale różną liczbą parametrów.

* Dopuszczalne jest zadeklarowanie funkcji / wzorca bez ciała, raz. Ponowna deklaracja wywoła błąd.

* Próba wywołania funkcji / wzorca, która jest niezadeklarowana wywoła błąd.

* Próba wywołania funkcji / wzorca, która jest zadeklarowana, lecz niezdefiniowana wywoła błąd.

* Próba zadeklarowania funkcji o jednym parametrze i o takiej samej nazwie co zadeklarowany wzorzec wywoła błąd.

* Nazwy parametrów funkcji nie mogą się powtarzać.

## Definicja funkcji / nazwanego wzorca

* Definicja funkcji / wzorca, która już została zadeklarowana jest poprawna, gdy cała sygnatura będzie się zgadzać (czyli także typ zwrotu, nazwy parametrów, mutowalność parametrów, typ parametrów ).

* Definicja funkcji / wzorca, która już została zdefiniowana wywoła błąd.

* Funkcja, która ma podany typ zwrotny inny niż NOTHING, nie może zawierać instrukcji skoku RETURN bez wartości.

* I odwrotnie, funkcja, która ma podany typ zwrotny NOTHING, nie może zwracać wartości.

* Definicja funkcji / wzorca, który został już zadeklarowany, przesłoni deklarację w obecnym zasięgu (bloku). Po wyjściu z zasięgu, funkcja z powrotem staje się tylko zadeklarowana. Taką sytuację nazywamy "miękkim przesłanianiem".

* Jeżeli przy przesłanianiu przed nazwą dodamy słowo kluczowe OVERRIDE, dokonamy "twardego przesłaniania" - wtedy funkcja / wzorzec pozostanie zdefiniowana do końca działania programu, nawet przy zmianie kontekstu.

## Przykłady

Funkcja przyjmująca liczbę i zwracająca jej kwadrat:

```
FUNCTION pow2 WITH NUMBER x RETURNS NUMBER
BEGIN
	RETURN x * x;
END

CALL pow2 WITH 3 NOW; # zwróci 9 #
```

Funkcja przyjmująca datę jako trzy liczby (dzień, miesiąc, rok) i zwracająca ją jako sformatowny ciąg znaków:

```
FUNCTION get_date WITH NUMBER day, NUMBER month, NUMBER year RETURNS TEXT
BEGIN
	RETURN day ++ "/" ++ month ++ "/" ++ year;
END

CALL get_date WITH 11, 10, 2000 NOW; # zwróci 11/10/2000 #
```

Funkcja nie przyjmująca argumentów i zwracająca liczbę pi z dokładnością do dwóch liczb po przecinku:

```
FUNCTION get_pi RETURNS NUMBER
BEGIN
    RETURN 3.14;
END

CALL get_pi NOW; # zwróci 3.14 #
```

Funkcja nie przyjmująca argumentów i nie zwracająca wartości. Wypisuje na wyjście napis "Hello World!":

```
FUNCTION hello_world RETURNS NOTHING
BEGIN
    CALL Print WITH "Hello World!" NOW;
END

```

Przykładowa funkcja dopasowania wzorca - w zależności od wartości zmiennej "gąska" modyfikuje wartość zmiennej "a":

```
MUTABLE NUMBER a IS 0;

PATTERN testGąska WITH TEXT gąska
BEGIN
    VALUE ?? DO a IS NONE;,
    VALUE == "balbinka" OR VALUE == "barbara" DO a IS 1;,
    VALUE == "kasia" DO a IS 2;,
    VALUE == CALL getSavedGoose NOW DO a IS 3;,
    DEFAULT a IS 0;,
END

CALL testGąska WITH "balbinka" NOW;  # ustawi zmienną a na 1 #
CALL Print WITH a NOW;
```
