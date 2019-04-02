function SGF = ApplySgolay(input_type, raw_input, order, framelen, visual, jointIndex)


if strcmp(input_type,'Skeleton') == 1
    skeleton = raw_input;
for joint_index=1:length(skeleton)
   Input_Rotations = skeleton(joint_index).rxyz';
   % Skip empty joints in skeleton structure
   if length(Input_Rotations) == 0
        continue;
   end
   
   x = Input_Rotations(:,1);
   y = Input_Rotations(:,2);
   z = Input_Rotations(:,3);
   Input_Rotations = [z y x];

   SGF{joint_index} = sgolayfilt(Input_Rotations, order, framelen);
   if visual == 1
    PlotSGF(skeleton(joint_index).name, Input_Rotations, SGF{joint_index});
   end
end
end

if strcmp(input_type,'Joint') == 1
    SGF = sgolayfilt(raw_input, order, framelen);
    if visual == 1
        PlotSGF(num2str(jointIndex), raw_input, SGF);
   end
end

end



function PlotSGF(JointName, Input_Rotations, SGF_Rotations)
    % Set Figure
    figure
    x0=500;
    y0=500;
    width=800;
    height=400;
    set(gcf,'position',[x0,y0,width,height]);

    % Set subplot1 : Input_Rotations
    subplot(1,2,1);
    curve1 = animatedline('LineWidth',1,'Color','b');
    title( strcat('Raw - ','Joint ',JointName) );
    view(43,24);
    grid on;
    hold on;

    % Set subplot2 : SGF_Rotations
    subplot(1,2,2);
    curve2 = animatedline('LineWidth',1,'Color','r');
    title( strcat('SGolay - ','Joint ',JointName) );
    view(43,24);
    grid on;
    hold on;
    
    drawAnimation(Input_Rotations,SGF_Rotations,curve1,curve2);

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
    color = rand(1,3);
    
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

