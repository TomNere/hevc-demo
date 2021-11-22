Cílem video komprese je odstranit opakující se obrazy nebo scény a tak zredukovat velikost souboru.
K tomu se využívá predikce obrazových informací. V kodeku H.265 existují 2 typy predikce.

**Intra picture (vnitřní predikce)** 

* predikce na základě podobných barevných hodnot pixelů nacházejících se v odlišných pozicích v rámci jednoho snímku
* nepotřebuje znát data z předešlých snímků, takže na kódování prvního snímku se vždy využije tenhle typ predikce
* příkladem je, když se ve snímku nachází velké plochy podobné barvy, takže se dá použít kopírování sousedních pixelů z aktuálního snímku

**Inter picture (vnější predikce)**

* predikce na základě podobných barevných hodnot pixelů nacházejících se v časově odlišných/sousedních snímcích
* příkladem je pohyblivý objekt, kdy predikce využije toho, že se objekt mezi snímky posouvá ale nemění tvar a barvu 

Každá **jednotka predikce** je kódovaná za použití jednoho z těchto typů.