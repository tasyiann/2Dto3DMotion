% This method uses the matrices and exports the clusters.
sourceDir = 'Big-Database-new-Scaling\Source_CLUSTERING_TESTS'; % Where L_T, Yfull matrices are.
outputDir = 'Big-Database-new-Scaling\NEW_output';              % Where to save the clusters.
groupsNum = 10;
num_clusters = 500;

% Clusters
% for i=1:groupsNum                            % For each group of Data:
%     makeClusters(sourceDir, i, 500, outputDir);
% end
makeClusters(sourceDir, 2, 500, 'Big-Database-new-Scaling\NEW_output\JustGroup2');