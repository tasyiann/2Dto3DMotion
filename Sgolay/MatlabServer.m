clc
clear all
tcpipServer = tcpip('0.0.0.0',55000,'NetworkRole','Server');

while(1)
clear rawData
flushinput(tcpipServer);
fopen(tcpipServer);

fprintf(1,"Waiting for client's message...");
rawData = fread(tcpipServer,100,'char');        % get raw data.
for i=1:length(rawData) 
   s(i)= char(rawData(i));                      % convert data into char.
end
path = strcat(s);                               % convert into string.

try
   fprintf(1,"Message: %s\n",path);
   ApplySgolayToFrames(path,0);
   fprintf(1,"Applied sgolay to %s\n",path);       % print path
   
catch ME
   fprintf(1,"Could not applied sgolay to %s\n",path);% print path
end
fclose(tcpipServer);
end
