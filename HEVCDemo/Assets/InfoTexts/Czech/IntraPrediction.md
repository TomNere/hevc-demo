* vnitřní (prostorová) predikce čerpá výhodu z redudance informací v sousedních pixelech ve video snímcích aby predikovala bloky pixelů z okolních pixelů
* díky tomu se dá místo samotných hodnot pixelů přenášet tzv. **předikční chyba**
* predikční jednotky mohou být vnitřne predikovány těmito módy:
	* **MÓD 0 (DC)** - všem vzorkám *p(x,y)* je přiřazena stejná hodnota. Ta je vypočtena jako průměr referenčních vzorek
	* **MÓD 1 (Planar)** - všechny hodnoty *p(x,y)* jsou získány jako průměr dvou čísel *h(x,y)* a *v(x,y)*. Tyhle čísla jsou vypočtena jako lineární interpolace v horizontálním a vertikalním směru, viz. **obr.3**
	* **MÓDY 2-34 (Angular)** - hodnoty referenčních vzorků jsou ve specifikovaném směru distribuovány do kódovaného bloku. Pokud se predikovaný pixel *p(x,y)* nachází mezi referenčními vzorky, použije se interpolace
