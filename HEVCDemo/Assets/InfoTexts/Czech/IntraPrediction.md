* vnitřní (prostorová) predikce čerpá výhodu z redundance informací v sousedních pixelech ve video snímku aby predikovala bloky pixelů z okolních pixelů
* díky tomu se dá místo samotných hodnot pixelů přenášet tzv. **predikční chyba**
* příkladem je, když se ve snímku nachází velké plochy podobné barvy, takže se dá použít kopírování sousedních pixelů z aktuálního snímku

**Predikční jednotky mohou být vnitřně predikovány těmito módy:**

* __MÓD 0 (DC - konstantní)__ - všem pixelům *p(x,y)* (notace na obr.1) je přiřazena stejná barevná hodnota. Ta je vypočtena jako průměr hodnot referenčních pixelů (obr.2)
* __MÓD 1 (planar - plochý)__ - všechny hodnoty *p(x,y)* jsou získány jako průměr dvou čísel *h(x,y)* a *v(x,y)*. Tato čísla jsou vypočtena jako lineární interpolace v horizontálním a vertikalním směru, viz. obr.3
* __MÓDY 2-34 (angular - směrový)__ - hodnoty referenčních pixelů jsou ve specifikovaném směru (obr. 4) distribuovány do kódovaného bloku. Pokud se predikovaný pixel *p(x,y)* nachází mezi referenčními vzorky, použije se interpolace