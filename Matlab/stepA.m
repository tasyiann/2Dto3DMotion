% Use this script to spit into groups, then make clusters.

clear all;  % clear all var
clc;        % Clear console

mainDir = 'Big-Database-new-Scaling';
inputDir = [mainDir,'\Projections'];   % Directory of all projections
dirNameT_L = [mainDir,'\T_L'];         % Directory to save/load T.mat and L.mat Matrices
groups = 10;                 % Number of group to split projections
clusters = 500;              % Number of clusters for each group.
extension = '';              % Extension on cluster files.
dirToSave = [mainDir,'\Output'];      % Location of saving results. NOT USED

% Read all Projections:
[T,L] = CreateTextures(inputDir);
if(exist([mainDir,'\T_L_FULL'],'dir')==0)
   mkdir([mainDir,'\T_L_FULL']);
end
save([mainDir,'\T_L_FULL\','T_FULL.mat'],'T');
save([mainDir,'\T_L_FULL\','L_FULL.mat'],'L');

% Split into Groups:
IntoGroups(groups,T,L,dirNameT_L);
clear T;
clear L;


% For each .mat in the DB, do the clustering
files = dir([dirNameT_L,'\*.mat']);     % Get all .mat of the dir.
index = 1;                              % Index of .mat files. Used for arranging

for file = files'                       % Iterate only according to T.mat files
    if( startsWith(file.name,"T") )
       if(index >=7) 
        clusterProjections(clusters,dirNameT_L, dirToSave, file,index,extension);
       end
       index = index + 1;
    end
end

