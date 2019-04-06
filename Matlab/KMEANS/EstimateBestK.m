function K = EstimateBestK(Y_full, Replicates, parallel)
clustersNumInitial = 10;
clustersOffset = 10;
minThreshold = 0.007;
beforeDist = 0;

% Clusters
% - - Initialization - -
dataNumDist = [0 0 0];                   % Reset matrix.
clustersNum = clustersNumInitial;        % Reset num of clusters.
mean_last10items = realmax;              % Reset the mean of the dif between iterations.
counter = 1;                             % Reset the counter.
% - - - - - - - - - - --
while mean_last10items > minThreshold    % Repeat until, mean of the dif between iterations is below thshld.
    [idx,C, dist] = kmeans(Y_full, clustersNum, 'Replicates', Replicates, 'Options', statset('UseParallel',parallel),'OnlinePhase', 'on');                    % Execute k-means.
    dist = mean(dist);                                              % Get mean dist between clusters and their centroid.
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
    
    %fprintf(1,'Group %d, With %d clusters, dist from centroid is %f <value change is %f>and mean value change (last 10) is %f\n',i, clustersNum, dist, valueChange, mean_last10items);
    clustersNum = clustersNum + clustersOffset;     % Next number of clusters.
    counter = counter + 1;                          % Next iteration.
end
fprintf('Number of cluster table:\n');
disp(dataNumDist);
K=clustersNum-clustersOffset;
end
