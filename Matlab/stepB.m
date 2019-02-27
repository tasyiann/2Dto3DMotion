%% Merge Clusters
reorderClusters('Big-Database-new-Scaling\Output\Results\500-clusters\ToBeMerged','Big-Database-new-Scaling\Output\Results\500-clusters\Merged');

%% PREPARATION OF MERGING: Dimensionality reduction on representatives
% 
% % Load Representatives - Important: File should have .p extension!
% path = 'Results\Test\Merged';
% T = CreateTextures(path); % Argument should be a dir
% 
% % Calculate Distance Matrix
% D = DistanceBetweenPoses(T,'Yes');
% 
% % Multi-dimensional Scaling of Distance Matrix
% options = optimset('Display','iter');
% dim = 2;
% Y_full = mdscale(D, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
% 
% % Plot
% f = figure()
% scatter(Y_full(:,1),Y_full(:,2),20,[0,0,0],'filled');
% saveas(f,[path,'before_merging.png']);

%% MERGE CLUSTERS
