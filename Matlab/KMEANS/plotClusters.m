function plotClusters(Y,idx,C,k,dim)
% plotMotionWordsClusters
%
%   plotMotionWordsClusters(D,Y,idx,k)
%
% -------------------------------------------------------------------------
% Author:
% -------------------------------------------------------------------------
% Andreas Aristidou (a.aristidou@ieee.org)
%     - Created: 2017.03.01
% - Last Edited: 2017.03.01
% -------------------------------------------------------------------------
% make more of these
Marker = {'+','*','s','d','^','v','o','x', 'h', '<', '>', 'p'};

figure
for j = 1:k
    color = rand(1,3);
    m = mod(j,length(Marker));
    if m == 0
        m = 1;
    end
    marker = Marker{m};
    for i = 1:length(Y)
        if idx(i) == j
            if dim == 2
                plot(Y(i,1),Y(i,2),'Color',color,'Marker',marker), hold on
                plot(C(j,1),C(j,2),'Color',color,'Marker','o'), hold on
                plot(C(j,1),C(j,2),'Color',[1 0 0],'Marker','x'), hold on
            elseif dim == 3
                plot3(Y(i,1),Y(i,2),Y(i,3),'Color',color,'Marker',marker), hold on
                plot3(C(j,1),C(j,2),C(j,3),'Color',color,'Marker','o'), hold on
                plot3(C(j,1),C(j,2),C(j,3),'Color',[1 0 0],'Marker','o'), hold on
            end
        end
    end
end
