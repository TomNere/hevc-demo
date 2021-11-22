* vnejší (inter) predikce čerpá výhodu z redundance informací ve video snímcích navazujících na sebe
* příkladem je pohyblivý objekt, kdy predikce využije toho, že se objekt mezi snímky posouvá ale nemění tvar a barvu 
* predikční jednotky jsou kódovány pomocí tzv. **vektorů pohybu** (motion vectors)
* vektor pohybu definuje posun predikční jednotky v aktuálním snímku vzhledem k pozici podobné predikční jednotky v referenčním snímku
* jednotka může obsahovat 1 nebo 2 vektory a obsahuje také informaci, který vektor se použije (**jednostranná predikce**), nebo se použijou oba vektory (**oboustranná predikce**). V tom případě vzniká výsledná jednotka spojením dvou jednotek.

**Výběr vektorů:**

* bere se v potaz, že vektor pro aktuální predikční jednotku bude mít malý rozdíl oproti vektorům sousedních jednotek
* když se dva snímky velmi podobají, tak se může použít vektor z jiného snímku ve stejné nebo podobné pozici - tzv. *co-located* jednotka
* ve výsledku se tedy může použít jeden vektor z jednotek Ax a jeden z jednotek Bx (viz. obr. 1) z aktuálního snímku, vektor z jiného snímku, nebo nulový vektor
* bližší info viz. [https://www.elecard.com/page/motion_compensation_in_hevc](https://www.elecard.com/page/motion_compensation_in_hevc)