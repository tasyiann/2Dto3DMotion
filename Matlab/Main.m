addpath(genpath('KMEANS\'));
addpath(genpath('MDS\'));

clear all;  % clear all var
clc;        % Clear console


inputDir_Projections = '..\Databases\v1_height\Projections1';    % Directory of all projections.
dirMatrices = 'v1_height_matrices';                              % Directory to save L, and Y_full (from MDS).
groups = 4;                                                      % Number of group to split projections.


outputDir_Clusters = '..\Databases\v1_height\Clusters';         % Where to save the clusters.
replicates = 1;
parallel = 0;

fprintf('Input: %s\n',inputDir_Projections);
logOutput = strcat(outputDir_Clusters,'\log.txt');
diary logOutput

%Group_and_MDS(inputDir_Projections, dirMatrices, groups);
MakeClustersOfGroups(dirMatrices, outputDir_Clusters, groups, replicates, parallel);
