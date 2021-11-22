* vnitřní (prostorová) predikce čerpá výhodu z redundance informací v sousedních pixelech ve video snímku aby predikovala bloky pixelů z okolních pixelů
* díky tomu se dá místo samotných hodnot pixelů přenášet tzv. **predikční chyba**
* příkladem je, když se ve snímku nachází velké plochy podobné barvy, takže se dá použít kopírování sousedních pixelů z aktuálního snímku

**Predikční jednotky mohou být vnitřně predikovány těmito módy:**

* __MÓD 0 (DC - konstantní)__ - všem pixelům v červeném obdélníku (obr.1) je přiřazena stejná barevná hodnota. Ta je vypočtena jako průměr hodnot referenčních pixelů (mimo obdélníku)
* __MÓD 1 (planar - plochý)__ - v tomhle případě se pro výpočet hodnot pixelů využívá lineární interpolace v horizontálním a vertikalním směru, bližší info viz. [https://www.elecard.com/page/spatial_intra_prediction_in_hevc](https://www.elecard.com/page/spatial_intra_prediction_in_hevc)
* __MÓDY 2-34 (angular - směrový)__ - hodnoty referenčních pixelů jsou ve specifikovaném směru (obr. 2) distribuovány do kódovaného bloku. Pokud se predikovaný pixel nachází mezi referenčními vzorky, použije se interpolace