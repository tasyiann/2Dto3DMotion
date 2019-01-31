function parsave(row,i,workerID)
%% Backup the matrix (save as mat)
%dir = ['Backup\',num2str(workerID),'.mat'];
%save(dir,'temp');
%% Write rows in file
filename = ['Backup\',num2str(workerID),'.csv'];
dlmwrite(filename,[i,row],'-append');
%% Log the iteration
%logfile = fopen(['Backup\',num2str(workerID),'.txt'],'a');
%fprintf(logfile,'%s\n',num2str(i));
end

