% Clusters analysis.

% This method uses the matrices and exports the clusters.
sourceDir = 'Big-Database-new-Scaling\Source_CLUSTERING_TESTS'; % Where L_T, Yfull matrices are.
outputDir = 'Big-Database-new-Scaling\Output_CLUSTERING_TESTS\Clustering_Analysis'; % Where to save the analysis of clustering.
groupsNum = 1;
clustersNumInitial = 400;
clustersOffset = 100;
minThreshold = 0.007;
beforeDist = 0;


% Clusters
for i=1:groupsNum                            % For each group of Data:
                                             % - - Initialization - -
    dataNumDist = [0 0 0];                   % Reset matrix.
    clustersNum = clustersNumInitial;        % Reset num of clusters.
    mean_last10items = realmax;              % Reset the mean of the dif between iterations.
    counter = 1;                             % Reset the counter.
                                             % - - - - - - - - - - --
    while mean_last10items > minThreshold    % Repeat until, mean of the dif between iterations is below thshld. 
        dist = clusterAndGetDistFromCentroid(sourceDir,i,clustersNum);  % Get mean dist between clusters and their centroid.
        valueChange = abs(beforeDist - dist);                           % Keep track of how much mean dist changes between iterations.
        dataNumDist = [ dataNumDist ; [clustersNum dist valueChange] ]; % Save data in matrix.
        beforeDist = dist;                                              % Keep track of the adjacent iterations.
        
        % Get mean of value changes of last 10 iterations.
        if(counter > 10)
            start = counter-9;
            last10items = dataNumDist(start:counter,:);
            last10items = last10items(:,3);
            mean_last10items = mean(last10items);
        end
        
        fprintf(1,'Group %d, With %d clusters, dist from centroid is %f <value change is %f>and mean value change (last 10) is %f\n',i, clustersNum, dist, valueChange, mean_last10items);
        clustersNum = clustersNum + clustersOffset;     % Next number of clusters.
        counter = counter + 1;                          % Next iteration.
    end
    fprintf(1,'Final table, Groupd %d and clustersNum %d',i,clustersNum-10);
    disp(dataNumDist);
    % Save info in disk.
    save([outputDir,'\ClustersINFO',num2str(i),'.mat'],'dataNumDist');
end


% In case we really want to actually make the clusters.
% makeClusters(sourceDir,i,clustersNum,outputDir);