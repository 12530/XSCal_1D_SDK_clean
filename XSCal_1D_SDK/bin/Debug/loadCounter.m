function count = loadCounter()
% global runid;
% runid = 1;

load('.\AutomaticXSCal\Scripts\runid.mat')
load(['.\AutomaticXSCal\Scripts\run',num2str(runid),'\counter.mat'])
%introduce a break dependent on the current call (20 workers assumed) to
%avoid concurrent load/save of the counter .mat file
pause(counter/10 - floor(counter/10) + 0.1)
count = counter;
counter = counter + 1;
save(['.\AutomaticXSCal\Scripts\run',num2str(runid),'\counter.mat'],'counter')
end
