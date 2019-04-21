%%
% y = sgolayfilt(x,k,f) applies a Savitzky-Golay FIR smoothing filter to the data in vector x.
% If x is a matrix, sgolayfilt operates on each column.
% The polynomial order k must be less than the frame size,
% f, which must be odd. If k = f-1, the filter produces no smoothing.
% 
% y = sgolayfilt(x,k,f,w) specifies a weighting vector w with length f,
% which contains the real, positive-valued weights to be used during
% the least-squares minimization. If w is not specified or if it is specified as empty,
% [], w defaults to an identity matrix.
% 
% y = sgolayfilt(x,k,f,w,dim) specifies the dimension, dim,
% along which the filter operates. If dim is not specified,
% sgolayfilt operates along the first non-singleton dimension;
% that is, dimension 1 for column vectors and nontrivial matrices, and dimension 2 for row vectors.


% Smoothing using Savitzky-Golay filter:
% >Tracks signal more closely and account for transient effects
% >Preserves high frequency components of data while smoothing it

function ApplySgolayToFrames(dirname,visual)
numberOfJoints = 14;
order = 3;                  % Polynomial order : example 2nd, 3rd, 4th                 
framelen = 11;              % The length of the window frame.
%% Using Joint 3D Position
for i = 0 : (numberOfJoints-1)
    filename = strcat(dirname,'\',num2str(i),"_joint.3D");
    fprintf(1, 'Applying Sgolay to %s\n',filename);
    x = load(filename);
    sgf = ApplySgolay('Joint',x,order,framelen,visual,i);
    fprintf(1,'Number of frames: %d, %d\n',size(x,1),size(sgf,1));
    % Write in file
    outputDirName = strcat(dirname,'\SgolayApplied");
    mkdir(outputDirName);
    outputFilePath = strcat(outputDirName,'\',num2str(i),"_joint_SGolayed.3D");
    dlmwrite(outputFilePath,sgf,'delimiter',' ');
end


%% Implementation of 3D Rotations (?) Euler, Quaternion (?)