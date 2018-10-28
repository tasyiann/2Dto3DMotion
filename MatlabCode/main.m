% Main Function
% 
% -------------------------------------------------------------------------
% Paper:
% -------------------------------------------------------------------------
% Tassos Yiannakides, Andreas Aristidou, and Yiorgos Chrysanthou. 2019. 
% 3D Human Motion Reconstruction from 2D silhouttes. To be submitted at
% Computer Animation and Virtual Words.
% 
% -------------------------------------------------------------------------
% References:
% -------------------------------------------------------------------------
% [1] Jehee Lee, Jinxiang Chai, Paul S. A. Reitsma, Jessica K. Hodgins, and
%     Nancy S. Pollard. 2002. Interactive control of avatars animated with
%     human motion data. ACM Trans. Graph. 21, 3 (July 2002), 491-500.
% [4] Leonid Blouvshtein, and Daniel Cohen-Or. 2018. Outlier Detection for
%     Robust Multi-dimensional Scaling. IEEE Transactions on Pattern
%     Analysis and Machine Intelligence.
%
% -------------------------------------------------------------------------
% Author:
% -------------------------------------------------------------------------
% Andreas Aristidou (a.aristidou@ieee.org)
%     - Created: 2018.09.13
% - Last Edited: 2018.09.13
% -------------------------------------------------------------------------

%%
clear all
clc % Clean the screan

% -------------------------------------------------------------------------
% Select the desired dataset
% -------------------------------------------------------------------------
kMW = 20;  % Define the number of clusters for motion-words


tic; % Initialize time

    % ---------------------------------------------------------------------
    % For the CMU skeleton
    % ---------------------------------------------------------------------
    % Create a Table with just the joints.
    % Read a file,
    % for each line, skip the first 2 numbers, and get the 14 joints.
    % T is a 2d matrix. 
    % lines are the joint positions, and cols represent frames.
    T = CreateTextures('0'); 


clc % Clean the screan


%%
disp('-------------------------------------------')
disp('Start computing the distances between poses')
disp('-------------------------------------------')
% -------------------------------------------------------------------------
% Distances between poses:
% This procedure will return a distance matrix with the peer-to-peer
% distances between all poses.
% -------------------------------------------------------------------------

D = DistanceBetweenPoses(T,'Yes'); % Get the distance with eucledian - Done


%%
disp('-----------------------------------------------------')
disp('Start of Multi Dimension Scaling')
disp('-----------------------------------------------------')

% -------------------------------------------------------------------------
% Multi-dimension Scaling
% Position skeleton poses into p-dimensional space using Multi-Dimensional
% Scaling (MDS).
% - By default, use of the method described in [2]
% -------------------------------------------------------------------------
tic; % Initialize time

dim = 2; % Give the desired dimension of the data (e.g., dim = 20)

Y_full = triangleMDS(D,dim,0);
% Y_full = mdscale(D,dim);     % Alternative, we can use the MATLAB default MDS Scaling


%%
disp('-----------------------------------------------------')
disp('Start of Clustering Motion Words')
disp('-----------------------------------------------------')

% -------------------------------------------------------------------------
% Clustering skeleton poses:
% Having all skeleton poses presented in p-dimensional space as points, we
% can group similar points into clusters.
% -------------------------------------------------------------------------
tic; % Initialize time

% ---------------------------------------------------------------------
% - Use of the K-means method for clustering
% ---------------------------------------------------------------------
[idx,C] = kmeans(Y_full,kMW);

plotClusters(Y_full,idx,C,kMW,dim)

