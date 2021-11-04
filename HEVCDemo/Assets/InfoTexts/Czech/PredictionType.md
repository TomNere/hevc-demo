* v kodeku H.265 existují 2 typy predikce
* **intra-picture (vnitřno-snímková)** 
	* k predikování částí obrazu využívá pouze obrazová data, která se nacházejí v aktuálně kódovaném snímku
	* nepotřebuje znát data z předešlých snímků, takže na kódování prvního snímku se vždy využije tenhle typ predikce
	* příkladem je, když se ve snímku nachází velké plochy podobné barvy, takže se dá použít kopírování sousedních pixelů z aktuálního snímku
* **inter-picture (mezi-snímková)** 
	* k predikování využívá obrazová data získána z předešlých snímků
	* příkladem je pohyblivý objekt, kdy predikce využije toho, že se objekt mezi snímky posouvá ale nemění tvar a barvu 
* každá **jednotka predikce** je kódovaná za použití jednoho z těchto typů 