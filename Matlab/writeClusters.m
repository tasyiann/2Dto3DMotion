function dirname = writeClusters(L,idx, kMW, index, dirname, extension) 
% index reveals the ORDER of matrices



LL = L';
for i=1:size(LL)
   id=idx(i,1);
   filename = [dirname,'\',num2str(id),extension];
   dlmwrite(filename,LL(i,:),'delimiter',' ','-append');
end

