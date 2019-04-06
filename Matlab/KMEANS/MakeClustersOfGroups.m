function MakeClustersOfGroups(sourceDir, outputDir, groupsNum, replicates, parallel)
fprintf('..Start kMeans..\n');
for i=1:groupsNum
    fprintf('Processing group %d...\n',i);
    makeClusters(sourceDir, i, replicates, parallel, outputDir);
    fprintf(' Done!\n');
end
