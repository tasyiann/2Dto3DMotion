function sgf = Sgolay_Joint_Position(x, order, framelen, index, visual)
%SGOLAY_JOINT_POSITION Applies sgolay to the position of the joint.
sgf = sgolayfilt(x,order,framelen);
if visual == 1

% Visual
figure
x0=500;
y0=500;
width=800;
height=400;
set(gcf,'position',[x0,y0,width,height]);




subplot(1,2,1);
curve1 = animatedline('LineWidth',1,'Color','b');
title( strcat('Raw - ','Joint ',num2str(index)));
view(43,24);
grid on;
hold on;

%plot3(x(:,1),x(:,2),x(:,3));




subplot(1,2,2);
curve2 = animatedline('LineWidth',1,'Color','r');
title( strcat('SGolay - ','Joint ',num2str(index)));
view(43,24);
grid on;
hold on;


drawAnimation(x,sgf,curve1,curve2);
%plot3(sgf(:,1),sgf(:,2),sgf(:,3));


end
end

function drawAnimation(x,sgf,curve1,curve2)
r=0;
g=0;
b=0;

hold on;
for i=1:length(x)
    r = mod(r + i*0.02,0.9);
    %g = mod(g + i*0.0.2,0.9);
    %b = mod(b + i*0.0.2,0.9);
    color = [r g b];
    
    addpoints(curve1,x(i,1),x(i,2),x(i,3));
    subplot(1,2,1);
    head1 = scatter3(x(i,1),x(i,2),x(i,3),'filled','MarkerFaceColor',color);
    
    addpoints(curve2,sgf(i,1),sgf(i,2),sgf(i,3));
    subplot(1,2,2);
    head2 = scatter3(sgf(i,1),sgf(i,2),sgf(i,3),'filled','MarkerFaceColor',color);
    
    drawnow
    pause(0.05);
end


end

