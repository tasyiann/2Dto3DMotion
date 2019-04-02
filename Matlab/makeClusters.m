function makeClusters(dirSource,index,num_clusters,dirTarget)
%MAKECLUSTERS Summary of this function goes here




% Initialisation
extension = '';
load([dirSource,'\L_T',num2str(index),'.mat'],'L');         % Load L
load([dirSource,'\Y_full',num2str(index),'.mat'],'Y_full'); % Load Y_Full

% K - means
[idx,C] = kmeans(Y_full, num_clusters);
% Calculate dist from centroid


% Write Clusters
dirname = [dirTarget,'\',num2str(num_clusters),'\',num2str(num_clusters),'-clusters\',num2str(index)];
mkdir(dirname);
writeClusters(L,idx, num_clusters, index, dirname, extension);

% Write Representatives
WriteRepresentatives(C,Y_full, idx, L, [dirTarget,'\',num2str(num_clusters)]);

end

