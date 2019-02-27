function dist = meanDistanceFromCentroid(idx, C, Y_full) 

sz = size(idx);
fprintf(1,'size of object list is %d\n',sz(1));
dist = 0;
for i=1:sz(1)                               % Iterate all objects.
    c_id = idx(i,1);                        % The centroid of the object.
    dist =  dist + norm(Y_full(i,1:2)-C(c_id,1:2)); % Distance between object and its cetroid.
end
dist = dist / sz(1);                        % Finally, calculate the mean.

