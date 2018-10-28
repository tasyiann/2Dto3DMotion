function T = CreateTextures(filename)
%CreateTextures
%
% Author:
% -------------------------------------------------------------------------

% -------------------------------------------------------------------------

fid = fopen(filename);
tline = fgetl(fid);
metadata = 2;
numOfJoints = 14;
row = 1;

while ischar(tline)
    
    
    fields = sscanf(tline, '%f');
    joints = fields(metadata+1:(3*numOfJoints+metadata))';
    for col=1:(3*numOfJoints)
        T(col,row)=joints(col);
    end
    % Go to next line
    tline = fgetl(fid);
    row = row +1;
end
    fclose(fid);






end