function Clusters = getClusters_Projections(L,idx, kMW) 
Clusters = cell(kMW,1);
LL = L';
% Projection LL(i,:) belongs to Clusters{idx(i,1)}
for i=1:size(LL)
   id=idx(i,1);
   Clusters{id} = [Clusters{id} ; LL(i,:)];
end



