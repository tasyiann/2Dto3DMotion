function reorderClusters(sourcePath, targetPath, groupsNum)


% Source path should contain only the directories
% that contains the clusters. Named as 1,2,3...
% Please remove representatives from the directories.
              


index = 1;             % File index number.
for i = 1:groupsNum
    currD = [sourcePath,'\',num2str(i)];  % Get the current subdirectory (or file) name
    disp(currD);
    display(['Processing files at: ',currD]);
    files = dir([currD,'\*']);    % Get all files of the dir.
    
	% I NEED TO TEST THIS BEFORE RUN THIS!
	% Get all names sorted
	N = natsortfiles({files.name}); 
	N; % Debug!
	
    % Move .clu files to targetPath
    for j=1:numel(N)
		fname = N{j}; % selected file name
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
