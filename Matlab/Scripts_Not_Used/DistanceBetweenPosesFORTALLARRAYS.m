function D = DistanceBetweenPosesFORTALLARRAYS(T,display)
% DistanceBetweenPoses: Find the distance between poses.
%
%   D  - The distance matrix
%
% -------------------------------------------------------------------------
% References
% -------------------------------------------------------------------------

% -------------------------------------------------------------------------
% Author:
% -------------------------------------------------------------------------

% -------------------------------------------------------------------------
if nargin < 2 || nargin > 2
    error('Wrong number of input arguments')
end
fprintf(1,"Start computing the distances between poses");
% -------------------------------------------------------------------------
% Initialization
% -------------------------------------------------------------------------

% Get size of Matrix, and Initialise Distance Matrix
numFrames = length(T(1,:));
%D = sparse(numFrames,numFrames);

parfor i = 1:numFrames % i:loop variable    
    row = zeros(1,numFrames);
    %% Get task id
    task = getCurrentTask();
    id = get(task, 'ID');
    %% Calculate Distance of the pose
    if strcmp(display,'Yes') == 1
        clc
        fprintf('\n Processing pose: %d out of %d \n',i,numFrames)
    end
    for j = i:numFrames % to compute the half diagonal
        Distance = norm(T(1:2,i)-T(1:2,j))+norm(T(4:5,i)-T(4:5,j))+norm(T(7:8,i)-T(7:8,j))+norm(T(10:11,i)-T(10:11,j))+norm(T(13:14,i)-T(13:14,j))+norm(T(16:17,i)-T(16:17,j))+norm(T(19:20,i)-T(19:20,j))+norm(T(22:23,i)-T(22:23,j))+norm(T(25:26,i)-T(25:26,j))+norm(T(28:29,i)-T(28:29,j))+norm(T(31:32,i)-T(31:32,j))+norm(T(34:35,i)-T(34:35,j))+norm(T(37:38,i)-T(37:38,j))+norm(T(40:41,i)-T(40:41,j));
        if isnan(Distance) == 1
           Distance = 0;
        end
        row(j) = Distance;
        if i == j
            if row(j)~= 0 
              row(j) = Distance; 
          end
        end
    end
    %% Save this iteration
    parsave(row,i,id);
end



