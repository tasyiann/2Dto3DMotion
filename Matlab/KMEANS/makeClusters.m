function makeClusters(dirSource,index, Replicates, parallel, dirOutput)

% Initialisation
load([dirSource,'\L',num2str(index),'.mat'],'L');           % Load L
load([dirSource,'\Y_full',num2str(index),'.mat'],'Y_full'); % Load Y_Full

fprintf('\tEstimate best k...\n');
num_clusters = EstimateBestK(Y_full, Replicates, parallel);
fprintf('\tk=%d\n',num_clusters);

% K - means
[idx,C] = kmeans(Y_full, num_clusters, 'Replicates', Replicates, 'Options', statset('UseParallel',parallel),'OnlinePhase', 'on');
% plotClusters(Y_full,idx,C,num_clusters,2);


% Get Clusters
if ~exist(dirOutput, 'dir')
    mkdir(dirOutput)
end
Clusters = getClusters_Projections(L,idx, num_clusters);

% Get Representatives
Representatives = getRepresentatives_Projections(C,Y_full, idx, L, dirOutput, num_clusters);

% Write clusters in file
WriteClustersInFile(Clusters, Representatives, strcat(dirOutput,'\clusters',num2str(index)));

end

