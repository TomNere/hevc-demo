* basic processing unit of H.265 is **Coding tree unit (CTU)** (fig. 1)
* it is a part of the image of size 16x16 to 64x64 pixels
* __CTU__ is then divided to one or more **Coding units (CU)** which defines type of prediction (smallest size is 8x8)
* __CU__ can be too large for motion vectors, so it can be divided to **Prediction units (PU)**
