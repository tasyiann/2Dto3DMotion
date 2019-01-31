clear all;  % clear all var
clc;        % Clear console

inputDir = 'BigDatabase\Projections';   % Directory of all projections
dirName = 'DB';             % Directory to save/load T.mat and L.mat Matrices
groups = 8;                 % Number of group to split projections
clusters = 500;             % Number of clusters for each group.
extension = '.clu';         % Extension on cluster files.
dirToSave = 'Results';      % Location of saving results. NOT USED

% Read all Projections:
%{
[T,L] = CreateTextures(inputDir);
save('T_FULL.mat','T');
save('L_FULL.mat','L');

% Split into Groups:
IntoGroups(groups,T,L,dirName);
clear T;
clear L;
%}

% For each .mat in the DB, do the clustering
files = dir([dirName,'\*.mat']);    % Get all .mat of the dir.
index = 1;                          % Index of .mat files. Used for arranging

for file = files'                   % Iterate only according to T.mat files
    if( startsWith(file.name,"T") )
        if (index >= 7)
            clusterProjections(clusters,dirName,file,index,extension);
        end
        index = index + 1;
    end
end

