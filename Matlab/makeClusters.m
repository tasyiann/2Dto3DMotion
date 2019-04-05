function makeClusters(dirSource,index,num_clusters,dirTarget)

% Initialisation
load([dirSource,'\L_T',num2str(index),'.mat'],'L');         % Load L
load([dirSource,'\Y_full',num2str(index),'.mat'],'Y_full'); % Load Y_Full

% K - means
[idx,C] = kmeans(Y_full, num_clusters, 'Replicates', 10, 'Options', statset('UseParallel',1),'OnlinePhase', 'on');
% plotClusters(Y_full,idx,C,num_clusters,2);


% Get Clusters
dirname = [dirTarget,'\',num2str(num_clusters),'\',num2str(num_clusters),'-clusters\',num2str(index)];
mkdir(dirname); % Just in case it does not exist.
Clusters = getClusters_Projections(L,idx, num_clusters);

% Get Representatives
Representatives = getRepresentatives_Projections(C,Y_full, idx, L, [dirTarget,'\',num2str(num_clusters)], num_clusters);

% Write clusters in file
WriteClustersInFile(Clusters, Representatives, strcat(dirname,'\clusters',num2str(index)));

end

