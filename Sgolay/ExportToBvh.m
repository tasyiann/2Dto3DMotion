function ExportToBvh(SGF_Rotations, filename_input, filename_output)

fid = fopen(strcat(filename_input,'.bvh'));
i=1;
isMotionSection = 0;
frameIndex = 1;


tline = fgetl(fid);
while ischar(tline)

    % Header Section
    if isMotionSection==0
       Header{i} = tline; 
    end
    
    % Motion Section
    if isMotionSection == 1
        tuple = str2num(tline);
        tuple = tuple(1:3);
        for joint_index = 1: length(SGF_Rotations)
            if length(SGF_Rotations{joint_index})~=0
                tuple = [tuple, SGF_Rotations{joint_index}(frameIndex,:)];
            end
        end
        Motion{frameIndex} = tuple;
        frameIndex = frameIndex + 1;
    end
    
    % Start changing the rot in file, when pass the Frame Time.
    if isMotionSection==0 && strncmpi(Header{i},'Frame Time',10)
        isMotionSection = 1;
    end
    
    % Read next line
    i = i+1;
    tline = fgetl(fid);

end
fclose(fid);


outputFileName = strcat(filename_input,'_SFG.bvh');
fid = fopen(outputFileName,'w');
fprintf(fid,'%s\r\n',Header{:});
for i=1:length(Motion)
    dlmwrite(outputFileName,Motion{i},'delimiter',' ','precision','%.4f','newline','pc', '-append');
end
fclose(fid);



end

