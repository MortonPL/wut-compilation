# Operatory

| Priorytet 	| Operator  		   | Opis                       | Stronność 	  | L. arg.   | Typ danych  |
|-----------	|------------------    |---------------------       |-----------	  |---------  |------------ |
| 1         	| CALL x NOW		   | Wywołanie metody           | Lewo            | 1         | -           |
| 2         	| x ??       		   | Test pustości              | Lewo     	      | 1         | -           |
| 3         	| -x        		   | Zmiana znaku               | Prawo     	  | 1         | NUMBER      |
| 3         	| NOT x     		   | Negacja logiczna           | Prawo     	  | 1         | FACT        |
| 4         	| x * y     		   | Mnożenie                   | Lewo      	  | 2         | NUMBER      |
| 4         	| x / y     		   | Dzielenie                  | Lewo      	  | 2         | NUMBER      |
| 4         	| x % y     		   | Reszta z dzielenia         | Lewo      	  | 2         | NUMBER      |
| 5         	| x + y     		   | Dodawanie                  | Lewo      	  | 2         | NUMBER      |
| 5         	| x - y     		   | Odejmowanie                | Lewo      	  | 2         | NUMBER      |
| 5         	| x ++ y     		   | Konkatencja tekstu         | Lewo      	  | 2         | TEXT        |
| 6         	| x < y     		   | Nierówność ostra           | Lewo      	  | 2         | -           |
| 6         	| x > y     		   | Nierówność ostra           | Lewo      	  | 2         | -           |
| 6         	| x <= y    		   | Nierówność nieostra        | Lewo      	  | 2         | -           |
| 6         	| x >= y    		   | Nierówność nieostra        | Lewo      	  | 2         | -           |
| 7         	| x == y    		   | Równość                    | Lewo      	  | 2         | -           |
| 7         	| x != y    		   | Brak równości              | Lewo      	  | 2         | -           |
| 7         	| x === y    		   | Równość łańcuchów          | Lewo      	  | 2         | TEXT        |
| 7         	| x !== y    		   | Brak równości łańcuchów    | Lewo      	  | 2         | TEXT        |
| 8         	| x AND y   		   | Koniunkcja                 | Lewo      	  | 2         | FACT        |
| 9         	| x OR y    		   | Alternatywa                | Lewo      	  | 2         | FACT        |
| 10         	| x ? y : z 		   | Operator ternarny          | Lewo      	  | 3         | FACT        |
| 11            | x THEN y OTHERWISE z | Rurociąg                   | Lewo            | 2         | -           |
| 12         	| x IS y	    	   | Przypisanie                | Prawo     	  | 2         | -           |
