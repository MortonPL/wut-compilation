# PW TKOM 22L

### Szybki start

Na repozytorium znajduje się archiwum ze zbudowanym projektem, gotowym do włączenia.

Budowanie projektu: `dotnet build` (wymaga .NET SDK 5.0). Zbudowane pliki wykonywalne zostaną automatycznie skopiowane do folderu `Build`, wraz z przykładowym plikiem lokalizacyjnym oraz przykładowym programem.

Uruchamianie dla zbudowanego projektu: `VerboseCLI.exe -f <path_to_file>`

Uruchamianie testów automatycznych: `dotnet test`

### Język

Przykładowe fragmenty kodu źródłowego tworzonego języka można znaleźć w plikach `expressions.md`, `statements.md`, `definitions.md`, definicję gramatyki w `grammar.md`, a tabelę z priorytetami operatorów w `operators.md`.

Język ogólnego zastosowania. Narzucone typowanie: statyczne, słabe, niemutowalne, opcjonalne. Ideą tego języka było stworzenie języka programowania o składni o dość ograniczonym zasobie specjalnych znaków i możliwie prostej do zrozumienia przez człowieka znającego język angielski. Taki język mógłby być np. użyty do uczenia programowania w przystępny sposób osób bez wiedzy informatycznej.

### Implementacja

Użyty został język C# i platforma .NET. Do implementacji interpretera użyto wzorcu wizytatora. Parser - rekursywnie zstępujący.

### Sposób działania programu

```
Użytkownik (wywołuje dostarcza kod źródłowy)
|
... Warstwa aplikacji / VerboseCLI
|
|-->Aplikacja konsolowa - Parsuje argumenty całego programu, otwiera plik jeśli trzeba, przekazuje uchwyt do strumienia, tworzy interpreter. Logger - Tłumaczy komunikaty wysyłane przez niższe warstwy, wypisuje je na wyjście.
    |
    |-->Interpreter - Woła Parser o kompletne (!) drzewo instrukcji, wykonuje zparsowany kod, odpowiedzialny za błędy semantyczne.
        |
        ... Warstwa biblioteki parsera / VerboseCore
        |
        |-->Parser - Woła ParserScanner o kolejne tokeny, próbuje konstruować instrukcje, odpowiedzialny za błędy składniowe.
            |
            |-->ParserScanner - Woła Lexer o kolejne tokeny, przekazuje je Parserowi.
                |
                |-->Lexer - Próbuje konstruować tokeny, woła LexerScanner o znaki, odpowiedzialny za błędy leksykalne.
                    |
                    |-->LexerScanner - otrzymuje uchwyt do strumienia kodu źródłowego, odczytuje kolejne znaki.
```

Na każdym etapie działania programu, komponenty mogą wywołać swój Logger (w praktyce korzystają z tego samego, dostarczonego przez aplikację konsolową), który wypisuje do strumienia wyjściowego informacje o postępach lub do strumienia błędów informacje o błędach w interpretowanym kodzie źródłowym.

Warstwa biblioteki / VerboseCore była zaplanowana, aby możliwa była wymiana leksera, parsera lub obu na inną implementację i zachowanie kompatybliności. Zostało to prawie zrealizowane - interpreter jest w bardzo ograniczony sposób zależny od implementacji biblioteki.

### Obsługa błędów

Wyznaczono następujący podział błędów:

* Leksykalne - znalezienie nielegalnych znaków lub kombinacji znaków podczas analizy leksykalnej

* Składniowe - znalezenie kombinacji tokenów nie należącej do żadnej produkcji podczas analizy składniowej

* Semantyczne - błędy powstałe podczas wykonywania programu użytkownika, **w tym błąd przepełnienie stosu**

* Inne - błędy działania aplikacji konsolowej, podanie ścieżki do nieistniejącego pliku itp.

Użytkownik jest informowany o wszystkich błędach niezależnie od konfiguracji poprzez wyświetlenie komunikatów na ekranie. Komunikaty te zawierają co najmniej pozycję błędu w kodzie źródłowym (wiersz, kolumna), kod błędu i czytelny opis.

### Użycie, argumenty wejściowe i źródła danych

Interpreter uruchamia się, włączając program VerboseCLI.exe. Aby osiągnąć jakiekolwiek efekty, należy podać co najmniej flagę `-i` lub `-f`!

Uruchomienie programu interpretera bez dodatkowych flag wyświetli krótki komunikat o dostępnych opcjach:

* `-f | --file <path_to_file>` - podanie ścieżki pliku z kodem źródłowym do odczytu

* `-i | --input <code>` - użycie standardowego strumienia wejściowego jako źródła

* `-l | --locale <path_to_file>` - podanie ścieżki pliku z lokalizacją językową

* `-o | --out <path_to_file>` - podanie ścieżki pliku, który będzie standardowym buforem wyjścia (w przeciwnym wypadku jest to konsola)

* `-e | --err <path_to_file>` - podanie ścieżki pliku, który będzie standardowym buforem błędów (w przeciwnym wypadku jest to konsola)

Interpreter nie wymaga, by pliki z kodem źródłowym miały ustalone rozszerzenie.

### Testy

Testy zostaną przeprowadzone przy użyciu biblioteki MSTest, FluentAssertions i manualnie.

Testy jednostkowe:

* Lekser - sprawdzanie poprawności / niepoprawności tokenizacji przygotowanego tekstu (sprawdzenie dla każdego tokenu + częste błędy)

* Parser - sprawdzanie poprawności / niepoprawności gramatycznej ciągów tokenów i budowanego drzewa (sprawdzenie dla każdej produkcji gramatyki + częste błędy)

* Interpreter - sprawdzenie poprawności przeparsowanych instrukcji, błędy runtime

Automatyczne testy akceptacyjne:

* Interpreter - Sprawdzanie poprawności / niepoprawności działania całego rurociągu Lekser-Parser-Interpreter za pomocą przykładowego kodu źródłowego i spodziewanego wyniku
