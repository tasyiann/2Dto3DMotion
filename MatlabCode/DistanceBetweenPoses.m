function D = DistanceBetweenPoses(T,display)
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

% -------------------------------------------------------------------------
% Initialization
% -------------------------------------------------------------------------
numFrames = length(T(1,:));

% Preallocate space for speed
D = zeros(numFrames,numFrames);

for i = 1:numFrames
    
    if strcmp(display,'Yes') == 1
        clc
        fprintf('\n Processing pose: %d out of %d \n',i,numFrames)
    end
    
    for j = i:numFrames % to compute the half diagonal
        Distance = norm(T(1:2,i)-T(1:2,j))+norm(T(4:5,i)-T(4:5,j))+norm(T(7:8,i)-T(7:8,j))+norm(T(10:11,i)-T(10:11,j))+norm(T(13:14,i)-T(13:14,j))+norm(T(16:17,i)-T(16:17,j))+norm(T(19:20,i)-T(19:20,j))+norm(T(22:23,i)-T(22:23,j))+norm(T(25:26,i)-T(25:26,j))+norm(T(28:29,i)-T(28:29,j))+norm(T(31:32,i)-T(31:32,j))+norm(T(34:35,i)-T(34:35,j))+norm(T(37:38,i)-T(37:38,j))+norm(T(40:41,i)-T(40:41,j));
        if isnan(Distance) == 1
           Distance = 0;
        end
        D(i,j) = Distance;
        D(j,i) = Distance;
        if i == j
          if D(i,j) ~= 0
              D(i,j) = 0;
              D(j,i) = 0;
          end
        end
    end
end



