%% Main Function
function main(kMW,path)
tic; % Initialize time
%% Step1: Initialise Poses and get size of data
[T,L] = CreateTextures(path);
sz = size(T);
sz = sz(2);
%save('T.mat','T');
%% Step2: Caluclate distances between Poses, and save in Disk as .csv files.
%DistanceBetweenPoses(T,'Yes');
%% Step3: Load Distances and sort them, and save tall array for future use.
ds = tabularTextDatastore('Backup');
tt = tall(ds);
% tt = sortrows(tt,1);
% tt(:,1)=[];
%% Fill nans
% tt = fillmissing(tt,'constant',0)

save('sortedTallArray.mat','tt');


%% Extra: Convert to csv [NOT USED]
%fprintf(1,"Converting to csv...");
%ds = datastore('Backup\1');
%YourData = load('Backup\1.txt');
%csvwrite('YourNewFile.csv', YourData);
%% Extra: Load tall array
%tt = load('sortedTallArray.mat');
%% Step4: Scale Datastore.

%options = optimset('Display','iter');
%dim = 2;
%Y_full = mdscale(tt, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
[coeff, score, latent, tsquared, explained, mu] = pca(tt); %% tall arrays are suitable
%Y_full = tsne(tt);




%{
%% Step5: Kmeans and Write Clusters
[idx,C] = kmeans(Y_full,kMW);
directoryName = writeClusters(L,idx, kMW);
%% Step6: Write Representatives
WriteRepresentatives(C,Y_full, idx, L, directoryName);
plotClusters(Y_full,idx,C,kMW,dim);
%% Execution success : time
ttime = toc;
fprintf(1,"Executed in %f seconds",ttime);
%}