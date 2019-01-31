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
Color = {[1 0 0], [0 0 0], [0 1 0], [0 0 1], [1 1 0], [1 0 1], [0 1 1], [0.5 0.5 0.5], [0,0.25,0], [0,0,0.25],[0.25,0.25,0],[0.25,0,0.25],[0,0.25,0.25],[0.5 0 0],[0.75,0.75,0.75],[0,0.75,0],[0,0.25,0.75],[0.75,0,0.75], [0.60 0.65 0.65],[0.8 0 0], [0.25,0.65,0], [0,0.10,0.65],[0.65,0.65,0],[0.65,0,0.65],[0,0.65,0.65],[0.5 0.5 0.5],[0.9 0 0], [0,0.85,0], [0,0,0.85],[0.85,0.85,0],[0.85,0,0.85],[0,0.85,0.85], [0 1 0], [0 0 1], [1 1 0], [1 0 1], [0 1 1]};
Marker = {'+','*','s','d','^','v','o','x', 'h', '<', '>', 'p'};

figure
for j = 1:k
    for i = 1:length(Y)
        if idx(i) == j
            
            m = mod(j,length(Color));
            if m == 0
                m = length(Color);
            end
            n = ceil(j/length(Color));
            if dim == 2
                plot(Y(i,1),Y(i,2),'Color',Color{m},'Marker',Marker{n}), hold on
                plot(C(j,1),C(j,2),'Color',Color{m},'Marker','o'), hold on
                plot(C(j,1),C(j,2),'Color',[1 0 0],'Marker','x'), hold on
            elseif dim == 3
                plot3(Y(i,1),Y(i,2),Y(i,3),'Color',Color{m},'Marker',Marker{n}), hold on
                plot3(C(j,1),C(j,2),C(j,3),'Color',Color{m},'Marker','o'), hold on
                plot3(C(j,1),C(j,2),C(j,3),'Color',[1 0 0],'Marker','o'), hold on
            end
        end
    end
end
