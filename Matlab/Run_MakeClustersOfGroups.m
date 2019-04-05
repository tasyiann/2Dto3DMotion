clc;
% This method uses the matrices and exports the clusters.
sourceDir = 'Not-In-Git\Input'; % Where L_T, Yfull matrices are.
outputDir = 'Not-In-Git\Output2';% Where to save the clusters.
groupsNum = 10;
num_clusters = 500;

% Clusters
% for i=1:groupsNum                            % For each group of Data:
%     makeClusters(sourceDir, i, 500, outputDir);
% end
makeClusters(sourceDir, 1, 500, 'Not-In-Git\Output2\Clusters');