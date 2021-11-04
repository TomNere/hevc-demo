* základní jednotka v kodeku H.265 se nazýva **Coding tree unit** - jednotka kódovacího stromu
* je to část obrazu s velikostí od 16x16 do 64x64 pixelů
* **CTU** pak obsahuje jednu nebo více **Coding units (CU)** - kódovací jednotky, pro které je definován typ predikce (můžou se dále dělit, nejmenší velikost je 8x8)
* **CU** ale může být pořád príliš velká pro vektory pohybu, takže může být rozdělena na tzv. **Prediction units (PU)** - jednotky predikce