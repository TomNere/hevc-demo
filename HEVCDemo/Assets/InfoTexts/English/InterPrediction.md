* inter prediction takes advantage from redundancy of informations in consecutive video frames
* simple example: some object in the scene is moving but has the same color, shape, ...
* prediction units are coded using **motion vectors**
* motion vector signalizes offset in coded frame from coordinates in reference frame
* prediction unit can contain 1 or 2 vectors a contains also information which vector is used (**uni-prediction**), or both vectors can be used (**bi-prediction**). In this case, final unit is created by combining two units.

**Selecting vectors:**

* it is considered, that vector for current unit will have small difference comparing to vectors of neighbouring units
* when to frames are very similar, vector from another frame on similar position can be used
* finally, vectors from units CandA and CandB (fig. 1) from current frame, vector from another frame from *co-located* unit and zero vector can be selected