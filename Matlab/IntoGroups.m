%% Step1: Devide projections into smaller groups (eg. 216000 p into 3 groups)
function IntoGroups(amount, T_FULL, L_FULL, dirName)

    % Get size of Matrix
    sz = size(T_FULL);      % 43 x 216000
    sz = sz(2);             % 216000
    
    % Set the size of each sub-Matrix
    offset = sz/amount;     % 216000 / 3 = 72000
    m_start = 1;
    m_finish = offset;
    % Save each sub-Matrix in the directory
    for i=1:amount
       T = T_FULL(:,m_start:m_finish);
       L = L_FULL(:,m_start:m_finish);
       
       % save
       filenameT = [dirName,'\T',num2str(i),'.mat'];
       filenameL = [dirName,'\L_T',num2str(i),'.mat'];
       save(filenameT,'T');
       save(filenameL,'L');
       
       % Update indices
       m_start = m_finish + 1;
       m_finish = m_finish + offset;
    end
    
end
    