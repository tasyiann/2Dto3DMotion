 clc;
% This method uses the matrices and exports the clusters.
sourceDir = 'Source\v1-216000-Scaling-Bone-Length'; % Where L_T, Yfull matrices are.
outputDir = 'Source\v1-216000-Scaling-Bone-Length\Output';% Where to save the clusters.
groupsNum = 10;
num_clusters = 500;

fprintf("..Start kMeans..\n");
for i=1:groupsNum
    fprintf("Processing group %d...",i);
    makeClusters(sourceDir, i, 500, outputDir);
    fprintf(" Done!\n");
end
