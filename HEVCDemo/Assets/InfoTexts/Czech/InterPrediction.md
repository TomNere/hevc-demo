* vnejší (časová) predikce čerpá výhodu z redundance informací v časově sousedných video snímcích
* příkladem je pohyblivý objekt, kdy predikce využije toho, že se objekt mezi snímky posouvá, ale nemění tvar a barvu 
* predikční jednotky jsou kódovány pomocí tzv. **vektorů pohybu** (motion vectors)
* vektor pohybu definuje posun predikční jednotky v aktuálním snímku vzhledem k pozici podobné predikční jednotky v časově sousedném snímku
* v H.265 se k vnejší predikci může použít 1 (**jednostranná predikce**) nebo 2 (**oboustranná predikce**) vektory
* v případě oboustranné predikce ukazuje jeden vektor na jednotku v předchozím snímku a druhý vektor na jednotku v následujícím snímku. Predikovaná jednotka pak vznikne spojením těchto dvou jednotek.
* jednotka z následujícího snímku je dostupná díky tomu, že snímky nemusí být kódovány v pořadí v jakém se zobrazují
* jelikož predikčních jednotek s vektory se může ve snímku nacházet hodně, využívá H.265 podobnost mezi vektory. Tím pádem nemusí být všechny vektory kódovány v plném tvaru, což přispívá k úspoře dat. Víc info na [https://www.elecard.com/page/motion_compensation_in_hevc](https://www.elecard.com/page/motion_compensation_in_hevc).