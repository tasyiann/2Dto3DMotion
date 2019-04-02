function [filtM] = triangle_filter(similarities, threshold, sample)
% returns a matrix that has zeros where the distance broken more than (>=)
% 'threshold' triangles.
%
% @in - similarities: NxN distances matrix
% @in - threshold: threshold  (0 for automatic selection)
% @out - filtM: NxN matrix of zeros and ones. ones represent non outliers.

N = size(similarities, 1);
if nargin == 2
    cntMat = bad_triangle_count(similarities); % Use this one.
else
    cntMat = bad_triangle_count(similarities, sample);
end
filtM = ones(size(similarities)) - eye(N, N);
if threshold == 0
   
    % manually find threshold
    % threshold = graythresh(cntMat);
   
   h =histcounts(cntMat);
   edgeCnt = 0;
   
   for th = 1:size(h,2)-1
       
       if edgeCnt > (N-1)*N/2 && h(th) < h(th+1) && threshold == 0
          threshold = th;
          break
          %disp(['found thrshold: '  num2str(threshold)]);
       end
       edgeCnt = edgeCnt + h(th);
   end    
end
for i = 1:N
    for j = i+1:N
        % disp(['i j' num2str(i) ' ' num2str(j)]);
        if cntMat(i, j) > threshold
            % disp(['removing i, j: ' num2str(i) ' ' num2str(j) ]);
            filtM(i, j) = 0;
            filtM(j, i) = 0; 
        end
    end
end
end