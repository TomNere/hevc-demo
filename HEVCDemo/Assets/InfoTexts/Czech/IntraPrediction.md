* vnitřní (prostorová) predikce čerpá výhodu z redundance informací v sousedních pixelech ve video snímcích aby predikovala bloky pixelů z okolních pixelů
* díky tomu se dá místo samotných hodnot pixelů přenášet tzv. **predikční chyba**

**Predikční jednotky mohou být vnitřně predikovány těmito módy:**

* __MÓD 0 (DC)__ - všem pixelům *p(x,y)* (notace na obr.1) je přiřazena stejná barevná hodnota. Ta je vypočtena jako průměr hodnot referenčních pixelů (obr.2)
* __MÓD 1 (Planar)__ - všechny hodnoty *p(x,y)* jsou získány jako průměr dvou čísel *h(x,y)* a *v(x,y)*. Tato čísla jsou vypočtena jako lineární interpolace v horizontálním a vertikalním směru, viz. obr.3
* __MÓDY 2-34 (Angular)__ - hodnoty referenčních vzorků jsou ve specifikovaném směru (obr. 4) distribuovány do kódovaného bloku. Pokud se predikovaný pixel *p(x,y)* nachází mezi referenčními vzorky, použije se interpolace