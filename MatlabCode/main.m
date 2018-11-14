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
% - Last Edited: 2018.10.31 - By Anastasios Yiannakides
% -------------------------------------------------------------------------
try
%%
clear all
clc % Clean the screan

% -------------------------------------------------------------------------
% Select the desired dataset
% -------------------------------------------------------------------------
kMW = 30;  % Define the number of clusters for motion-words


tic; % Initialize time

% ---------------------------------------------------------------------
% For the CMU skeleton
% ---------------------------------------------------------------------
% Create a Table with just the joints.
% Read a file,
% for each line, skip the metadata numbers, and get the 14 joints.
% T is a 2d matrix. 
% lines are the joint positions, and cols represent frames.

[T,L] = CreateTextures('VerySmallSample\Database\Projections');
% [T,L] = CreateTextures('oneP');
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
%load('Distances.mat','D'); 
disp('   >exec time (s):')
disp(toc);
try
save('Distances.mat','D')
catch MES
    savingerrorfile = fopen('error_saving_log.txt','a');
    fprintf(savingerrorfile,'%s\n',MES.identifier);
end
%% CLEAR TABLE T
clear T;
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
disp('   >exec time (s):')
disp(toc);
% Y_full = mdscale(D,dim);     % Alternative, we can use the MATLAB default MDS Scaling
try
save('Y_full.mat','Y_full')
catch MES
    savingerrorfile = fopen('error_saving_log.txt','a');
    fprintf(savingerrorfile,'%s\n',MES.identifier);
end
%% CLEAR TABLE D
clear D;
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
disp('   >exec time (s):')
disp(toc);
%%
% ---------------------------------------------------------------------
% - Save clusters and representitives
% ---------------------------------------------------------------------
disp('-------------------------------------------')
disp('Start Saving clusters')
disp('-------------------------------------------')
directoryName = writeClusters(L,idx);
disp('-------------------------------------------')
disp('Start Saving representatives')
disp('-------------------------------------------')
WriteRepresentatives(C,Y_full, idx, L, directoryName);
disp('>directory output:');
disp(directoryName);
disp('-------------------------------------------')
disp('Start Plotting')
disp('-------------------------------------------')
plotClusters(Y_full,idx,C,kMW,dim);
catch ME
    disp('-----------------------------------------------------')
    disp('* * * * ERROR. Execution has ended. * * * * ')
    disp('-----------------------------------------------------')
    errorfile = fopen('error_log.txt','a');
    fprintf(errorfile,'%s\n',ME.identifier);
    rethrow(ME);
end