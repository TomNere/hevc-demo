* intra (spatial) prediction takes advantage from redundancy of informations in neighboring pixels within video frame to predict blocks of pixels from their surrounding pixels
* simple example: big areas of the same color in one frame, so copying of the neighboring pixels can be used

**Prediction units can be intra predicted by these modes:**

* __MODE 1 (DC)__ - to all pixels in red border (fig.1) is assigned the same color value, which is calculated as arithmetic mean of values out of red border	
* __MODES 2-34 (angular)__ - values of the reference pixels are distributed in specified direction (fig. 2) in the coded block. If the predicted pixel is located between reference samples, an interpolated value is used as the prediction
* __MODE 0 (planar)__ - it was designed to generate continuous surface without disruptions on the blocks borders. So, for pixel value evaluation, horizontal and vertical prediction is used and then, final value is average of this two predictions. Detail info at [https://www.elecard.com/page/spatial_intra_prediction_in_hevc](https://www.elecard.com/page/spatial_intra_prediction_in_hevc)

Results after applying all 35 modes are in figure 3, which comes from the book *High Efficiency Video Coding - Algorithms and Architectures*