* intra (spatial) prediction takes advantage from redundancy of informations in neighboring pixels within video frame to predict blocks of pixels from their surrounding pixels
* thus, it allows to transmit the prediction errors instead of the pixel values themselves
* simple example: big areas of the same color in one frame, so copying of the neighboring pixels can be used

** Prediction units can be intra predicted by these modes:**

* __MODE 0 (DC)__ - all *p(x,y)* pixels (see notation at fig.1) are assigned the same color value, which is calculated as arithmetic mean of values of reference pixels (fig. 2)
* __MODE 1 (planar)__ - each *p(x,y)* value is obtained as arithmetic mean of two numbers *h(x,y)* and *v(x,y)*. These numbers are calculated as linear interpolation in the horizontal and vertical direction. see fig. 3
* __MODES 2-34 (angular)__ - values of the reference pixels are distributed in specified direction (fig. 4) in the coded block. If the predicted pixel *p(x,y)* is located between reference samples, an interpolated value is used as the prediction