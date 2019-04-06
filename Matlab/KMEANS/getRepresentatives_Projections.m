function Representatives = getRepresentatives_Projections(C,Y_full, idx, L, dirname, kMW)
% Find representatives of each cluster
%
%   D  - The distance matrix
%   C - Centroids (2D)
%   Y_full - Objects points on plot (2D)
%   idx - Cluster id for each Object
% -------------------------------------------------------------------------
% References
% -------------------------------------------------------------------------

% -------------------------------------------------------------------------
% Author: Anastasios Yiannakides
% -------------------------------------------------------------------------

% -------------------------------------------------------------------------

% Sort objects according to their cluster id
%  Oid - sorted objects: each row is a couple of (ObjectID, clusterID)
Representatives = cell(kMW,1);
Oid = zeros(size(idx));
Oid(:,2) = idx(:,1);
sz = size(idx);
num_of_Objects = sz(1);
Oid(:,1) = 1:num_of_Objects;
Oid = sortrows(Oid,2);

% INITIALIZE
row=1;
clusterId=1;        
% FIND REPRESENTATIVES
% Iterate all objects.
while row<=num_of_Objects
    % Find the representative object of that cluster.
    % Iterate all objects of the cluster numbered clusterId.
    min=intmax;
    repr=0;
    while row<=num_of_Objects && Oid(row,2)==clusterId
        projectionID = Oid(row,1);
        Distance = norm(C(clusterId,1:2)-Y_full(projectionID,1:2));
        if(Distance<min)
            min = Distance;
            repr = projectionID;
        end
        row = row + 1;
    end
    Representatives{clusterId} = L(:,repr)';
    % Go to the next cluster-id
    clusterId = clusterId + 1;
    % Write Representative in File
    %disp(repr);
    dlmwrite(strcat(dirname,'\Representatives'),L(:,repr)','delimiter',' ','-append');
end
%
