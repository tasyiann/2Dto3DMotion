function cntMat = bad_triangle_count(similarities, sample)
% returns a matrix that for every edge (i,j) returns the number of triangles it broke.
% also supports sampling when sample (Sx3 matrix) is provided.

N = size(similarities, 1);
epsilon = min(similarities(similarities>0)); % used for numeric purposes. otherwise is not needed.

cntMat = zeros(size(similarities));
if nargin == 2
    % sampling triangles in order to achiver N^2 complexity.
    for ijk = sample
        i=ijk(1); j=ijk(2); k=ijk(3);
        d1 = similarities(i, j);
        d2 = similarities(j, k);
        d3 = similarities(i, k);
        if (d1 + d2 + epsilon < d3|| d2 + d3 + epsilon < d1|| d1 + d3 + epsilon< d2)
            cntMat(i, j) = cntMat(i, j) + 1;
            cntMat(j, i) = cntMat(i, j);
            cntMat(j, k) = cntMat(j, k) + 1;
            cntMat(k, j) = cntMat(j, k);
            cntMat(i, k) = cntMat(i, k) + 1;
            cntMat(k, i) = cntMat(i, k);
        end
    end
else 
    for i = 1:N
        for j = i+1:N
            for k = j+1:N
                d1 = similarities(i, j);
                d2 = similarities(j, k);
                d3 = similarities(i, k);
                if (d1 + d2 + epsilon < d3|| d2 + d3 + epsilon < d1|| d1 + d3 + epsilon < d2)
                    cntMat(i, j) = cntMat(i, j) + 1;
                    cntMat(j, i) = cntMat(i, j);
                    cntMat(j, k) = cntMat(j, k) + 1;
                    cntMat(k, j) = cntMat(j, k);
                    cntMat(i, k) = cntMat(i, k) + 1;
                    cntMat(k, i) = cntMat(i, k);
                end
            end
        end
    end

end
end