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
numJoints = length(T(:,1))/3;
numFrames = length(T);


for i = 1:numFrames
    
    if strcmp(display,'Yes') == 1
        clc
        fprintf('\n Processing pose: %d out of %d \n',i,numFrames)
    end
    
    
    for j = i:numFrames % to compute the half diagonal
        Distance = 0;
        for m = 1:numJoints
            % -------------------------------------------------------------
            % Compare joints using eucledean distance.
            % -------------------------------------------------------------
            
            posA = (T(3*(m-1)+1:3*(m-1)+3,i)');
            posA(3)=0; % set z to 0.
            posB = (T(3*(m-1)+1:3*(m-1)+3,j)');
            posB(3)=0; % set z to 0.
            Distance = Distance + pdist([posA;posB],'euclidean');
            
            if isnan(Distance) == 1
                Distance = 0;
            end
        end
        Dist(i,j) = Distance;
         if i == j
            if Dist(i,j) ~= 0
                warning on
                warning('Motion word %d do not returns zero when compared to itself \n',i)
                Dist(i,j) = 0;
                warning off
            end
        end
    end
end

% -------------------------------------------------------------------------
% Convert half diagonal to full diagonal
% -------------------------------------------------------------------------
D = Dist' + Dist;
D(1:numFrames+1:end) = diag(Dist);



