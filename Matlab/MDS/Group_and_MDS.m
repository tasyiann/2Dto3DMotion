% Splits the data into groups, and then applies MDS to each group.
function Group_and_MDS(inputDir, dirMatrices, groups)
% Read all Projections:
[T,L] = CreateTextures(inputDir);

% Split into Groups:
IntoGroups(groups,T,L,dirMatrices);
clear T;
clear L;

% MDS
for i=1:groups
   MDS_Projections(dirMatrices, dirMatrices, i);
end

end