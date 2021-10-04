- intra (spatial) prediction takes advantage of redundancy in the information of neighboring pixel values within video frames to predict blocks of pixels from their surrounding pixels
- thus, it allows to transmit the prediction errors instead of the pixel values themselves
- **MODE 0 (DC)** - all *p(x,y)* samples are assigned the same value, which is calculated as arithmetic mean of of referenced samples
- **MODE 1 (Planar)** - each *p(x,y)* value is obtained as arithmetic mean of two numbers which are calculated as linear interpolation in the horizontal and vertical directions
- **MODES 2-34 (angular)** - the values of the reference samples in the specified direction are distributed inside the block to be coded. 
  If the pixel *p(x,y)* being predicted is located between reference samples, an interpolated value is used as the prediction