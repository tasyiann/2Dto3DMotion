function WriteClustersInFile(Clusters,Representatives, filename)
% Create empty file
fid = fopen(filename, 'w');
fclose(fid);
% Write data in file
for i=1:length(Representatives)
   seperator = ['C',num2str(i-1)];
   dlmwrite(filename, seperator,'delimiter','', '-append');
   dlmwrite(filename, Representatives{i}, 'delimiter',' ', '-append');
   dlmwrite(filename, Clusters{i}, 'delimiter',' ', '-append');
end

end

