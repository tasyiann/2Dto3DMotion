
<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">Real-time 3D human pose and motion reconstruction from monocular RGB videos</h3>
  <p align="center">
    <br />
    <a href="https://onlinelibrary.wiley.com/doi/full/10.1002/cav.1887"><strong>:page_facing_up: Publication DOI »</strong></a>
    <br />
    <br />
    <a href="https://www.youtube.com/watch?v=vyZjVTGWUUk">:clapper: Video Demo</a>
    ·
    <a href="https://graphics.cs.ucy.ac.cy/files/Thesis_Anastasios.pdf">:blue_book: Thesis</a>
    ·
    <a href="https://graphics.cs.ucy.ac.cy/home">:1st_place_medal: Graphics Lab</a>
    ·
    <a href="https://github.com/CMU-Perceptual-Computing-Lab/openpose">:person_fencing: OpenPose</a>
    ·
    <a href="https://github.com/CMU-Perceptual-Computing-Lab/openpose_unity_plugin">:joystick: OpenPose Unity Plugin</a>
  </p>
</div>



<p align="center">
  <h3 align="center">Abstract</h3>
 </p>
Real-time three-dimensional (3D) pose estimation is of high interest in interactive applications, virtual reality, activity recognition, and most importantly, in the growing gaming industry. In this work, we present a method that captures and reconstructs the 3D skeletal pose and motion articulation of multiple characters using a monocular RGB camera. Our method deals with this challenging, but useful, task by taking advantage of the recent development in deep learning that allows two-dimensional (2D) pose estimation of multiple characters and the increasing availability of motion capture data. We fit 2D estimated poses, extracted from a single camera via OpenPose, with a 2D multiview joint projections database that is associated with their 3D motion representations. We then retrieve the 3D body pose of the tracked character, ensuring throughout that the reconstructed movements are natural, satisfy the model constraints, are within a feasible set, and are temporally smooth without jitters. We demonstrate the performance of our method in several examples, including human locomotion, simultaneously capturing of multiple characters, and motion reconstruction from different camera views.
    <br />
    <br />
<div align="center">
  <a href="https://github.com/othneildrew/Best-README-Template">
    <img src="https://user-images.githubusercontent.com/31446189/140064054-ce6a6b21-d94c-4933-bd31-e4ebdfdd5fea.png" width="85%">
  </a>
</div>
    <br />
    <br />

<!-- GETTING STARTED -->
## Getting Started

This project runs on **Unity Engine 2020.3.24f1 LTS**, on **Windows** Operating System.

To run the **Real-Time** feature, you will need:
- to have a webcam connected to your pc.
- to follow steps 2,3 from Installation below, to complete the OpenPose setup.

Otherwise, you can explore the offline features.

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/tasyiann/2Dto3DMotion
   ```
2. *(For Real-Time usage)* Install trained models for OpenPose, by running the script:
   ```
   Assets/StreamingAssets/getModels.bat
   ```
3. *(For Real-Time usage)* Install OpenPose plugins, by running the script:
   ```
   Assets/OpenPose/getPlugins.bat
   ```

<!-- LICENSE -->
## License

Distributed under the GNU Affero General Public License v3.0 License. See `LICENSE.txt` for more information.


<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [OpenPose](https://github.com/CMU-Perceptual-Computing-Lab/openpose)
* [OpenPose Unity Plugin](https://github.com/CMU-Perceptual-Computing-Lab/openpose_unity_plugin)
* [Winterdust](https://winterdust.itch.io/bvhimporterexporter)
