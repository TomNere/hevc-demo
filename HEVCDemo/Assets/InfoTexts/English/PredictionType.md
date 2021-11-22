Video compression removes repetitive images or scenes from video a this way reduces the size of video file.
To achieve this, prediction of image informations is used. In H.265 codec, there are 2 types of prediction.

**Intra picture**

* prediction based on similar pixel color values on different positions in one frame
* does not need to know data from previous frames, so to encode the first frame of video always this type of prediction is used
* simple example: big areas of the same color in one frame, so copying of the neighboring pixels can be used

**Inter picture**

* prediction based on similar pixel color values in time different/neighboring frames
* simple example: some object in the scene is moving but has the same color, shape, ...

Each **prediction unit** is coded using one of these types.