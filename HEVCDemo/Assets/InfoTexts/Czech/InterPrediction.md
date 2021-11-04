* mezisnímková (inter) predikce čerpá výhodu z redudance informací ve video snímcích navazujících na sebe
* jednoduchý příklad je, když se kamera pouze jemně pohybuje, ale záběr zůstává stejný
* predikční jednotky jsou kódovány pomocí tzv. **vektorů pohybu** (motion vectors)
* vektor pohybu signalizuje ofset v kódovaném snímku od koordinátů v referenčním snímku
* jednotka může obsahovat seznam 2 vektorů a obsahuje také informaci, který vektor se použije (**uni-predikce**), nebo se použijou oba vektory (**bi-predikce**). V tom případě vzniká výsledná jednotka spojením dvou jednotek.
* **tvorba seznamu:**
	* bere se v potaz, že vektor pro aktuální jednotku bude mít malý rozdíl oproti vektorům sousedních jednotek
	* podobně když se dva snímky velmi podobají, tak se může použít vektor z jiného snímku ve stejné nebo podobné pozici
	* ve výsledku se do seznamu tedy můžou dostat vektory z jednotek CandA a CandB (viz. obr. 1) z aktuálního snímku, vektor z jiného snímku z tzv. *co-located* jednotky, nebo nulový vektor