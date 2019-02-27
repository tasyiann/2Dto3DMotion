function [T,L] = CreateTextures(directory)
%CreateTextures
%
% Author: Modified by Anastasios Yiannakides
% Last Modified: 29-10-18
% -------------------------------------------------------------------------
% Take the directory as a parameter.
% For each file in that directory, save the joints positions in T.
% and merge files in L.
% -------------------------------------------------------------------------

disp('Files read:'); % Debugging.
files = dir(strcat(directory,'\*.p')); % Get all files from that directory.
% Iterate all files in that directory
row = 1;                    % Counter of rows.
for file = files'

    fid = fopen(strcat(directory,'\',file.name));  % Open file named file.name.
    tline = fgetl(fid);         % Get the first line from file.
    metadata = 3;               % Metadata in each line: rotFileID, frame, degrees.
    numOfJoints = 14;           % Number of joints in each line.

    % Iterate lines of the file.
    while ischar(tline)
        
        fields = sscanf(tline, '%f');   % Get fields from line.
        % Skip the metadata from line (from the start of the line).
        joints = fields(metadata+1:(3*numOfJoints+metadata))';
        % Save (joints) fields of that line.
        for col=1:(3*numOfJoints)
            T(col,row)=joints(col);
        end
        % Save all fields in L.
        for col=1:metadata+3*numOfJoints
            L(col,row)=fields(col);
        end
        tline = fgetl(fid);      % Go to next line in file.
        row = row +1;            % Go to next row in table T.
        
    end % Go to next line.
    disp(file.name); % Debugging.
    fclose(fid);
    
end % Go to next file.

%disp(T);
%disp(L);