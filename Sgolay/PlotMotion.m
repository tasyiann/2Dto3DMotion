function PlotMotion( dirname1, dirname2 )
numberOfJoints=2;
starting_frame=30;
endin_frame=80;

joints = [4,7,10,13];

for i = joints
    filenames1 = strcat(dirname1,'\',num2str(i),'_joint.3D');
    filename2 = strcat(dirname2,'\',num2str(i),'_joint.3D');
    fprintf('Plotting %s\n',filenames1);
    datastarting_frame = load(filenames1);
    data2 = load(filename2);
    
    
    figure
    %% x-axis
    plot(starting_frame:endin_frame,data2(starting_frame:endin_frame,1),'r');
    hold on
    scatter(starting_frame:endin_frame,data2(starting_frame:endin_frame,1),'filled','o','r');
    plot(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,1),'black');
    title(strcat('3D Positions - Joint ',num2str(i)));
    set(gcf,'position',[500,500,1000,300]);
    xlabel('Frames');
    ylabel('x(black), y(green), z(blue)');
    hold on
    scatter(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,1),'filled','o','black');
    %% y-axis
    hold on
    plot(starting_frame:endin_frame,data2(starting_frame:endin_frame,2),'r');
    hold on
    scatter(starting_frame:endin_frame,data2(starting_frame:endin_frame,2),'filled','diamond','r');
    hold on
    plot(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,2),'g');
    hold on
    scatter(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,2),'filled','diamond','g');
    
    %% z-axis
    plot(starting_frame:endin_frame,data2(starting_frame:endin_frame,3),'r');
    hold on
    scatter(starting_frame:endin_frame,data2(starting_frame:endin_frame,3),'filled','s','r');
    hold on
    plot(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,3),'b');  
    hold on
    scatter(starting_frame:endin_frame,datastarting_frame(starting_frame:endin_frame,3),'filled','s','b');
end

end

