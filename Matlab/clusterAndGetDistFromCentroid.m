function dist = clusterAndGetDistFromCentroid(dirSource,index,num_clusters)
%MAKECLUSTERS Summary of this function goes here

% Initialisation
load([dirSource,'\L_T',num2str(index),'.mat'],'L');         % Load L
load([dirSource,'\Y_full',num2str(index),'.mat'],'Y_full'); % Load Y_Full

% K - means
[idx,C] = kmeans(Y_full, num_clusters);
% Calculate dist from centroid
dist = meanDistanceFromCentroid(idx,C,Y_full);
plotClusters(Y_full,idx,C,num_clusters,2);

pause;