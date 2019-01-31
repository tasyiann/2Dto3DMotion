function dirname = writeClusters(L,idx, kMW, index) % index reveals the ORDER of matrices

dirname = ['Results\',num2str(kMW),'-clusters\',num2str(index)];
mkdir('Results');
mkdir(['Results\',num2str(kMW),'-clusters']);
mkdir(dirname);

LL = L';
for i=1:size(LL)
   id=idx(i,1);
   filename = [dirname,'\',num2str(id),'.clu'];
   dlmwrite(filename,LL(i,:),'delimiter',' ','-append');
end

