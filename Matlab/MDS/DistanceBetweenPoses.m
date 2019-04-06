function D = DistanceBetweenPoses(T,display)

fprintf('Start computing the distances between poses...\n');

% Get size of Matrix, and Initialise Distance Matrix
numFrames = length(T(1,:));
DxD = sparse(numFrames,numFrames); % temp sparse matrix

parfor i = 2:numFrames % i:loop variable    
    % The worker uses this row to calculate distances, and in the end saves
    % it to D.
    row = zeros(1,numFrames);
    % Calculate Distance of the pose
    if strcmp(display,'Yes') == 1
        clc
        fprintf('\n Processing pose: %d out of %d \n',i,numFrames)
    end
    for j = 1:(i-1) % to compute the half diagonal
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
    
    % Save this iteration into the sparse matrix
    DxD(i,:) = row;
    
end


% Convert sparse matrix to vector
fprintf('\nConverting sparse matrix to vector...\n')
D = my_squareform(DxD);
fprintf('Convertion done! Distance vector is now ready to be used...\n');


end


function index = getIndex(i,j,m)
    index = ((i-1)*(m-i/2)+j-i);
    % fprintf(1,'i= %d, j= %d, m= %d, index is %d\n',i,j,m,index);
end