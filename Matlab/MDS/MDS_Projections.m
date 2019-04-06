function MDS_Projections(dirOfInput, dirToSave, groupIndex)
    % Welcoming message
    fprintf('Group %d:\n\tLoading matrices...\n',groupIndex);
    
    % Load Projections : T,L names should be the SAME as saved!
    load(strcat(dirOfInput,'\T',num2str(groupIndex),'.mat'),'T');   % Matrix T.mat
    load(strcat(dirOfInput,'\L',num2str(groupIndex), '.mat'),'L');  % Matrix L.mat
    
    % Calculate Distance Matrix
    fprintf('\tCalculating Distance Matrix...\n');
    D = DistanceBetweenPoses(T,'No');  
    % Save Distance Matrix
    ADirName = [dirToSave,'\Distance\'];
    if(exist(ADirName,'dir')==0)
        mkdir(ADirName);
    end
    save(strcat(ADirName,'Distances',num2str(groupIndex),'.mat'),'D');
    clear T;
    whos
    
    % Multi-dimensional Scaling of Distance Matrix
    fprintf('\tApplying MDS...');
    options = optimset('Display','iter');
    dim = 2;
    Y_full = mdscale(D, dim, 'Criterion', 'metricstress', 'Start', 'random', 'Options', options); %#ok<*NASGU>
    clear D;
    save(strcat(dirToSave,'\Y_full',num2str(groupIndex),'.mat'), 'Y_full');
    fprintf('Done!\n');
end