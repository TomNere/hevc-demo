* vnitřní (prostorová) predikce čerpá výhodu z redundance informací v sousedních pixelech ve video snímku aby predikovala bloky pixelů z okolních bloků
* příkladem je, když se ve snímku nachází velké plochy podobné barvy, takže se dá použít kopírování sousedních bloků z aktuálního snímku

**Predikční jednotky mohou být vnitřně predikovány těmito módy:**

* __MÓD 1 (DC - konstantní)__ - všem pixelům v červeném obdélníku (obr.1) je přiřazena stejná barevná hodnota. Ta je vypočtena jako průměr hodnot referenčních pixelů (mimo obdélníku)
* __MÓDY 2-34 (angular - směrové)__ - hodnoty referenčních pixelů jsou ve specifikovaném směru (obr. 2) distribuovány do kódovaného bloku. Pokud se predikovaný pixel nachází mezi referenčními vzorky, použije se interpolace
* __MÓD 0 (planar - plochý)__ - byl navržen, aby dokázal generovat spojitý povrch bez narušení na hranicích bloků (k čemuž dochází u konstantního a směrových módu). Pro výpočet hodnot pixelů se tedy využívá průměr 2 hodnot. Jedna hodnota je predikce v horizontálním směru a druhá ve vertikalním směru. Detailní výpočet viz. [https://www.elecard.com/page/spatial_intra_prediction_in_hevc](https://www.elecard.com/page/spatial_intra_prediction_in_hevc).

Výsledky po aplikaci těchto 35 módů jsou na obrázku 3, který pochází z knihy *High Efficiency Video Coding - Algorithms and Architectures*