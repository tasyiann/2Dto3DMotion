function writeClusters(L,idx)

LL = L';


for i=1:size(LL)
    id=idx(i,1);
    filename = num2str(id);
    dlmwrite(filename,LL(i,:),'delimiter',' ','-append');
end

