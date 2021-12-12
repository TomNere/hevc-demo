* inter (temporal) prediction takes advantage from redundancy of informations in time neighboring video frames
* simple example: some object in the scene is moving but has the same color, shape, ...
* prediction units are coded using **motion vectors**
* motion vector signalizes shift of the coded unit in current frame from similar unit in time neighboring frame
* in H.265, for inter prediction can be used 1 (**uni-prediction**) or 2 (**bi-prediction**) vectors
* in bi-prediction, one vector is pointing at unit in previous frame and second vector at unit in following frame. Predicted unit is then combination of these two units.
* unit from following frame is available thanks to fact, that frames are not coded in the same order as they are shown
* as the amount of prediction units with motion vectors can be huge, H.265 takes advantage from similarity of the vectors. Thus, not all vectors need to be coded in full form which helps to data savings. More info at [https://www.elecard.com/page/motion_compensation_in_hevc](https://www.elecard.com/page/motion_compensation_in_hevc).