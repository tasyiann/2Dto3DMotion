function clusterProjections(kMW, dirOfInput, dirToSave, file, subMatrixIndex, extension)

    % Welcoming message
    disp(['Cluster Projections >> SUB-MATRIX:',num2str(subMatrixIndex)]);
    disp(['Loading ',dirOfInput,'\',file.name,' and ',dirOfInput,'\L_',file.name]);
    
    % Load Projections : T,L names should be the SAME as saved!
    load([dirOfInput,'\',file.name],'T');   % Matrix T.mat
    load([dirOfInput,'\L_',file.name],'L'); % Matrix L.mat
    
    % Calculate Distance Matrix
    D = DistanceBetweenPoses(T,'No');  
    % Save Distance Matrix
    ADirName = [dirToSave,'\Distances\'];
    if(exist(ADirName,'dir')==0)
        mkdir(ADirName);
    end
    save([ADirName,'Distances',num2str(subMatrixIndex),'.mat'],'D');
    clear T;
    whos
    
    
    % Multi-dimensional Scaling of Distance Matrix
    options = optimset('Display','iter');
    dim = 2;
    Y_full = mdscale(D, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
    clear D;
    
    % Backup Y_full matrices for further examination of kMW variable
    clustersDirName = [dirToSave,'\Results\',num2str(kMW),'-clusters\']
    YfullDirName = [clustersDirName,'Y_full\'];
    if(exist(clustersDirName,'dir')==0)
        mkdir(clustersDirName);
    end
    if(exist(YfullDirName,'dir')==0)
        mkdir(YfullDirName);
    end
    save([YfullDirName,num2str(subMatrixIndex),'.mat'],'Y_full');
    
    % K-means
    [idx,C] = kmeans(Y_full,kMW);
    
    % Write Clusters
    dirname = [clustersDirName,num2str(subMatrixIndex)];
    mkdir(dirname);
    dirOfClusters = writeClusters(L,idx, kMW, subMatrixIndex, dirname, extension);
    
    % Write Representatives
    WriteRepresentatives(C,Y_full, idx, L, dirOfClusters);
    
end