* basic unit in H.265 codec is called **Coding tree unit (CTU)** (fig. 1)
* it is a part of the image of size from 16x16 to 64x64 pixels
* __CTU__ contains one or more **Coding units (CU)** for which type of prediction is defined (CU can be recursively divided to smallest size 8x8)
* __CU__ can be too large (e.g. for motion vectors), so it can be divided to **Prediction units (PU)**
