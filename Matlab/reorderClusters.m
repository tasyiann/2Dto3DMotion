function reorderClusters(sourcePath, targetPath)


% Source path should contain only the directories
% that contains the clusters. Named as 1,2,3...
% Please remove representatives from the directories.
              
D = dir(sourcePath); % Is a struct ... first elements are '.' and '..' used for navigation.
% For each folder in path - It is important to iterate in ascending order!

index = 1;             % File index number.
for i = 3:length(D)     % Avoid using the first ones
    currD = [sourcePath,'\',D(i).name];  % Get the current subdirectory (or file) name
    
    display(['Processing files at: ',currD]);
    files = dir([currD,'\*']);    % Get all files of the dir.
    
	% I NEED TO TEST THIS BEFORE RUN THIS!
	% Get all names sorted
	N = natsortfiles({files.name}); 
	N; % Debug!
	
    % Move .clu files to targetPath
    for j=1:numel(N)
		fname = N{j}; % selected file name
        display(fname);
        if( strcmp(fname,'.') || strcmp(fname,'..'))
            continue;
        end
        sourceName = [currD,'\',fname];                         
        targetName = [targetPath,'\',num2str(index)];
        copyfile(sourceName, targetName);
        index = index +1;
    end
end

end
