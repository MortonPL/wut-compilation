## Deklaracja i inicjalizacja zmiennych

`<mutowalność> <typ> varName <inicjalizacja>`, np. `MUTABLE TEXT a IS "a";`

* Dopuszczalna jest deklaracja zmiennej bez podania wartości początkowej.

* Niedopuszczalna jest próba ewaluacji zmiennej, która nie ma przypisanej wartości.

* Niedopuszczalne jest przypisanie wartości do zmiennej niemutowalnej, która została zadeklarowana.

* Niedopuszczalne jest zadeklarowanie zmiennej o nazwie zarezerwowanej (PIPE lub VALUE).

* Niedopuszczalna jest ponowna deklaracja zmiennej.
