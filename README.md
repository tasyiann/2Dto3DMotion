### NOTE:
- This repository was used to conduct my experiments for my **BSc Thesis**.
- It needs *polishing* & *documentation*. Will do soon - I promise. 😁


# Real-time 3D human pose and motion reconstruction from monocular RGB videos
### Links:
- BSc Thesis, University of Cyprus: https://graphics.cs.ucy.ac.cy/files/Thesis_Anastasios.pdf
- Publication DOI: https://onlinelibrary.wiley.com/doi/full/10.1002/cav.1887
- Graphics Lab: https://graphics.cs.ucy.ac.cy/home




![image](https://user-images.githubusercontent.com/31446189/140064054-ce6a6b21-d94c-4933-bd31-e4ebdfdd5fea.png)

### Abstract:
Real-time three-dimensional (3D) pose estimation is of high interest in interactive applications, virtual reality, activity recognition, and most importantly, in the growing gaming industry. In this work, we present a method that captures and reconstructs the 3D skeletal pose and motion articulation of multiple characters using a monocular RGB camera. Our method deals with this challenging, but useful, task by taking advantage of the recent development in deep learning that allows two-dimensional (2D) pose estimation of multiple characters and the increasing availability of motion capture data. We fit 2D estimated poses, extracted from a single camera via OpenPose, with a 2D multiview joint projections database that is associated with their 3D motion representations. We then retrieve the 3D body pose of the tracked character, ensuring throughout that the reconstructed movements are natural, satisfy the model constraints, are within a feasible set, and are temporally smooth without jitters. We demonstrate the performance of our method in several examples, including human locomotion, simultaneously capturing of multiple characters, and motion reconstruction from different camera views.
