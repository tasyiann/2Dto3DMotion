function clusterProjections(kMW, dirOfInput, file, subMatrixIndex, extension)

    % Welcoming message
    disp(['Cluster Projections >> SUB-MATRIX:',num2str(subMatrixIndex)]);
    disp(['Loading ',dirOfInput,'\',file.name,' and ',dirOfInput,'\L_',file.name]);
    
    % Load Projections : T,L names should be the SAME as saved!
    load([dirOfInput,'\',file.name],'T');   % Matrix T.mat
    load([dirOfInput,'\L_',file.name],'L'); % Matrix L.mat
    
    % Calculate Distance Matrix
    
    if(subMatrixIndex~=7)
    D = DistanceBetweenPoses(T,'No');  
    % Save Distance Matrix
    ADirName = 'Results\Distances\';
    if(exist(ADirName,'dir')==0)
        mkdir(ADirName);
    end
    save([ADirName,'Distances',num2str(subMatrixIndex),'.mat'],'D');
    end
    
    if(subMatrixIndex==7)
       load(['Results\Distances\','Distances7.mat'],'D'); 
    end
    
    clear T;
    whos
    
    
    % Multi-dimensional Scaling of Distance Matrix
    options = optimset('Display','iter');
    dim = 2;
    Y_full = mdscale(D, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options);
    clear D;
    
    % Backup Y_full matrices for further examination of kMW variable
    YfullDirName = ['Results\',num2str(kMW),'-clusters\','Y_full'];
    ADirName = ['Results\',num2str(kMW),'-clusters\'];
    if(exist(ADirName,'dir')==0)
        mkdir(ADirName);
    end
    BDirName = ['Results\',num2str(kMW),'-clusters\','Y_full'];
    if(exist(BDirName,'dir')==0)
        mkdir(BDirName);
    end
    save([YfullDirName,'\Y_full',num2str(subMatrixIndex),'.mat'],'Y_full');
    
    % K-means
    [idx,C] = kmeans(Y_full,kMW);
    
    % Write Clusters
    dirname = ['Results\',num2str(kMW),'-clusters\',num2str(index)];
    mkdir(dirname);
    dirOfClusters = writeClusters(L,idx, kMW, subMatrixIndex, dirname, extension);
    
    % Write Representatives
    WriteRepresentatives(C,Y_full, idx, L, dirOfClusters);
    
end