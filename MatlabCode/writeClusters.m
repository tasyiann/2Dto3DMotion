function dirname = writeClusters(L,idx)
dirname = tempname(pwd);
mkdir(dirname);
LL = L';
for i=1:size(LL)
    id=idx(i,1);
    filename = [dirname,'\',num2str(id)];
    dlmwrite(filename,LL(i,:),'delimiter',' ','-append');
end

